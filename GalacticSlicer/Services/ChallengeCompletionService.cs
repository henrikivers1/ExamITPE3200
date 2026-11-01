
using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Services;

public class ChallengeCompletionService
{
    private readonly GalacticSlicerDbContext _context;
    private readonly PlayerProgressService _progressService;
    private readonly RankService _rankService;
    private readonly ILogger<ChallengeCompletionService> _logger;

    public ChallengeCompletionService(
        GalacticSlicerDbContext context,
        PlayerProgressService progressService,
        RankService rankService,
        ILogger<ChallengeCompletionService> logger)
    {
        _context = context;
        _progressService = progressService;
        _rankService = rankService;
        _logger = logger;
    }

    public async Task<bool> CompleteChallengeAsync(
        string userId,
        int challengeId)
    {
        try
        {
            // Find the challenge
            var challenge = await _context.Challenges
                .FirstOrDefaultAsync(c => c.ChallengeId == challengeId);

            if (challenge == null)
            {
                _logger.LogWarning(
                    "Challenge {ChallengeId} does not exist",
                    challengeId);

                return false;
            }

            // Only official questions imported from questions.json
            // and assigned to a planet are eligible for Credits and XP.
            if (string.IsNullOrWhiteSpace(challenge.SourceId) ||
                !challenge.PlanetId.HasValue ||
                challenge.CreatedByUserId != null)
            {
                _logger.LogWarning(
                    "Reward rejected: Challenge {ChallengeId} is not an official planet question",
                    challengeId);

                return false;
            }

            // Check whether the player has already completed it
            bool alreadyCompleted = await _context.CompletedChallenges
                .AnyAsync(c => c.UserId == userId &&
                               c.ChallengeId == challengeId);

            if (alreadyCompleted)
            {
                _logger.LogInformation(
                    "Challenge {ChallengeId} has already been completed",
                    challengeId);

                return false;
            }

            // Get the player's progress
            var progress = await _progressService
                .GetOrCreateProgressAsync(userId);

            // Register completion
            _context.CompletedChallenges.Add(new CompletedChallenge
            {
                UserId = userId,
                ChallengeId = challengeId,
                CompletedAt = DateTime.UtcNow
            });

            // Award Credits and XP
            progress.Credits += challenge.CreditReward;
            progress.ExperiencePoints += 150;
            progress.Rank = _rankService.GetRank(progress.ExperiencePoints);

            // Save completion and rewards together
            await _context.SaveChangesAsync();

            // Log the successful completion and awarded rewards
            _logger.LogInformation(
                "Challenge {ChallengeId} completed. Awarded {Credits} Credits and {XP} XP",
                challengeId,
                challenge.CreditReward,
                150);

            return true;
        }
        catch (Exception ex)
        {
            // Log unexpected errors with their exception details
            _logger.LogError(
                ex,
                "Failed to complete Challenge {ChallengeId}",
                challengeId);

            // Remove any tracked changes left after the failed operation
            _context.ChangeTracker.Clear();

            return false;
        }
    }

    public async Task<bool> CompleteMissionAsync(
        string userId,
        int planetId)
    {
        try
        {
            // Only the first pass is recorded
            bool alreadyCompleted = await _context.CompletedMissions
                .AnyAsync(m => m.UserId == userId &&
                               m.PlanetId == planetId);

            if (alreadyCompleted)
            {
                return false;
            }

            _context.CompletedMissions.Add(new CompletedMission
            {
                UserId = userId,
                PlanetId = planetId,
                CompletedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Mission on Planet {PlanetId} completed",
                planetId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to complete mission on Planet {PlanetId}",
                planetId);

            _context.ChangeTracker.Clear();

            return false;
        }
    }

    public async Task<bool> CompletePassedMissionAsync(
        string userId,
        int planetId,
        IReadOnlyDictionary<int, string> submittedAnswers)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                submittedAnswers == null ||
                submittedAnswers.Count == 0)
            {
                return false;
            }

            var challengeIds = submittedAnswers.Keys.ToList();

            // Start a transaction so rewards and mission completion are atomic
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            // Prevent duplicate mission rewards
            bool missionCompleted = await _context.CompletedMissions
                .AnyAsync(m => m.UserId == userId &&
                               m.PlanetId == planetId);

            if (missionCompleted)
            {
                _logger.LogInformation(
                    "Mission on Planet {PlanetId} already completed",
                    planetId);

                return true;
            }

            // Load official questions from the database
            var challenges = await _context.Challenges
                .Where(c => challengeIds.Contains(c.ChallengeId))
                .ToListAsync();

            // Reject missing, unofficial, or incorrectly assigned questions
            if (challenges.Count != submittedAnswers.Count ||
                challenges.Any(c =>
                    c.PlanetId != planetId ||
                    string.IsNullOrWhiteSpace(c.SourceId) ||
                    c.CreatedByUserId != null))
            {
                _logger.LogWarning(
                    "Invalid mission questions for Planet {PlanetId}",
                    planetId);

                return false;
            }

            // Validate answers on the server
            var correctChallenges = challenges
                .Where(c => string.Equals(
                    submittedAnswers[c.ChallengeId],
                    c.CorrectAnswer,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            double percent =
                correctChallenges.Count * 100.0 / challenges.Count;

            // Failed missions must not award permanent rewards
            if (percent < 80)
            {
                return false;
            }

            // Get the player's existing progress
            var progress = await _context.PlayerProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (progress == null)
            {
                _logger.LogWarning(
                    "Mission reward rejected: Player progress not found");

                return false;
            }

            // Find previously rewarded challenges to prevent duplicate rewards
            var completedIds = await _context.CompletedChallenges
                .Where(c => c.UserId == userId &&
                            challengeIds.Contains(c.ChallengeId))
                .Select(c => c.ChallengeId)
                .ToListAsync();

            var completedSet = completedIds.ToHashSet();

            int awardedCredits = 0;
            int awardedXp = 0;

            // Only correctly answered, previously uncompleted questions earn rewards
            foreach (var challenge in correctChallenges)
            {
                if (completedSet.Contains(challenge.ChallengeId))
                {
                    continue;
                }

                _context.CompletedChallenges.Add(new CompletedChallenge
                {
                    UserId = userId,
                    ChallengeId = challenge.ChallengeId,
                    CompletedAt = DateTime.UtcNow
                });

                awardedCredits += challenge.CreditReward;
                awardedXp += 150;
            }

            // Update Credits, XP, and rank
            progress.Credits += awardedCredits;
            progress.ExperiencePoints += awardedXp;
            progress.Rank = _rankService.GetRank(progress.ExperiencePoints);

            // Record the passed mission
            _context.CompletedMissions.Add(new CompletedMission
            {
                UserId = userId,
                PlanetId = planetId,
                CompletedAt = DateTime.UtcNow
            });

            // Save every change as one transaction
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Mission completed on Planet {PlanetId}. Awarded {Credits} Credits and {XP} XP",
                planetId,
                awardedCredits,
                awardedXp);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save mission rewards for Planet {PlanetId}",
                planetId);

            // Discard tracked changes after a failed transaction
            _context.ChangeTracker.Clear();

            return false;
        }
    }


    // IDs of the planets whose mission the player has passed
    public async Task<HashSet<int>> GetCompletedPlanetIdsAsync(string userId)
    {
        var planetIds = await _context.CompletedMissions
            .Where(m => m.UserId == userId)
            .Select(m => m.PlanetId)
            .ToListAsync();

        return planetIds.ToHashSet();
    }
}
