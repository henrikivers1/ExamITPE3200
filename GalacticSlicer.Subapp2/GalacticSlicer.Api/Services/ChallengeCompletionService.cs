
using GalacticSlicer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Api.Services;

public class ChallengeCompletionService
{
    // Every mission, Endor included, hands out 10 random questions (like Subapp 1)
    public const int QuestionsPerMission = 10;
    public const int PassPercent = 80;
    public const int XpPerMission = 2000;
    public const int CreditsPerChapter = 500;

    private readonly GalacticSlicerDbContext _context;
    private readonly PlayerProgressService _progressService;
    private readonly PlanetProgressService _planetProgressService;
    private readonly RankService _rankService;
    private readonly ILogger<ChallengeCompletionService> _logger;

    public ChallengeCompletionService(
        GalacticSlicerDbContext context,
        PlayerProgressService progressService,
        PlanetProgressService planetProgressService,
        RankService rankService,
        ILogger<ChallengeCompletionService> logger)
    {
        _context = context;
        _progressService = progressService;
        _planetProgressService = planetProgressService;
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

                return false;
            }

            // Identify the planet before validating its questions
            var planet = await _context.Planets
                .FirstOrDefaultAsync(p => p.PlanetId == planetId);

            if (planet == null)
            {
                _logger.LogWarning(
                    "Mission reward rejected: Planet {PlanetId} does not exist",
                    planetId);

                return false;
            }

            var progress = await _context.PlayerProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (progress == null)
            {
                _logger.LogWarning(
                    "Mission reward rejected: Player progress not found");

                return false;
            }

            var planets = await _context.Planets
                .AsNoTracking()
                .OrderBy(p => p.Order)
                .ToListAsync();

            var completedPlanetIds = await GetCompletedPlanetIdsAsync(
                userId);

            if (!_planetProgressService.IsPlanetUnlocked(
                planet,
                progress,
                planets,
                completedPlanetIds))
            {
                _logger.LogWarning(
                    "Mission reward rejected: Planet {PlanetId} is locked",
                    planetId);

                return false;
            }

            // Endor is the final exam and can use official questions
            // from all previous planets.
            bool isFinalExam =
                string.Equals(
                    planet.Name,
                    "Endor",
                    StringComparison.OrdinalIgnoreCase) &&
                planet.Subject == null;

            var previousPlanetIds = planets
                .Where(p => p.Order < planet.Order)
                .Select(p => p.PlanetId)
                .ToHashSet();

            // Load the submitted questions from the database
            var challenges = await _context.Challenges
                .Where(c => challengeIds.Contains(c.ChallengeId))
                .ToListAsync();

            // Reject missing or unofficial questions.
            // Normal planets only accept their own subject questions.
            // Endor accepts official questions from previous planets.
            bool invalidQuestions =
                challenges.Count != submittedAnswers.Count ||
                challenges.Any(c =>
                    string.IsNullOrWhiteSpace(c.SourceId) ||
                    c.CreatedByUserId != null ||
                    !c.PlanetId.HasValue ||
                    (isFinalExam
                        ? !previousPlanetIds.Contains(
                            c.PlanetId.Value)
                        : c.PlanetId != planetId ||
                          c.Subject != planet.Subject));

            if (invalidQuestions)
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
            if (percent < PassPercent)
            {
                _logger.LogInformation(
                    "Mission failed on Planet {PlanetId}: {Correct}/{Total} correct",
                    planetId,
                    correctChallenges.Count,
                    challenges.Count);

                return false;
            }

            // Find previously completed questions so they are not stored twice.
            var completedIds = await _context.CompletedChallenges
                .Where(c => c.UserId == userId &&
                            challengeIds.Contains(c.ChallengeId))
                .Select(c => c.ChallengeId)
                .ToListAsync();

            var completedSet = completedIds.ToHashSet();

            int awardedCredits = CreditsForMission(planet.Order);
            int awardedXp = XpPerMission;

            // Remember every correctly answered official question.
            foreach (var challenge in correctChallenges)
            {
                bool alreadyRewarded = completedSet.Contains(challenge.ChallengeId);

                // Never insert duplicate CompletedChallenge records
                if (!alreadyRewarded)
                {
                    _context.CompletedChallenges.Add(new CompletedChallenge
                    {
                        UserId = userId,
                        ChallengeId = challenge.ChallengeId,
                        CompletedAt = DateTime.UtcNow
                    });
                }
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

    public static int CreditsForMission(int planetOrder)
    {
        return CreditsPerChapter * planetOrder;
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

