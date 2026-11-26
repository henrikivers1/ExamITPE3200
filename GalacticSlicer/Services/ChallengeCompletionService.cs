
using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Services;

public class ChallengeCompletionService
{
    // Game rules in one place, so the controller, the view and this service always agree
    public const int PassPercent = 80;          // % correct needed to pass a mission
    public const int XpPerCorrectAnswer = 150;  // XP for each correctly answered official question

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

                       // A planet without a subject is a final exam (Endor): it mixes questions from every subject.
            // Decided by the data, not the name, so an admin can rename or add planets.
            bool isFinalExam = planet.Subject == null;

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
                    // Same rule as QuestionService.GetPlanetPoolAsync: a normal planet only accepts its own subject
                    (!isFinalExam && c.Subject != planet.Subject));

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

            // Reward correct answers.
            // Endor can reward previously completed questions as a final exam.
            foreach (var challenge in correctChallenges)
            {
                bool alreadyRewarded = completedSet.Contains(challenge.ChallengeId);

                if (isFinalExam)
                {
                    // Final exam rewards are independent of previous planet rewards
                    awardedCredits += challenge.CreditReward;
                    awardedXp += XpPerCorrectAnswer;
                }
                else if (!alreadyRewarded)
                {
                    // Normal planets reward each challenge only once
                    awardedCredits += challenge.CreditReward;
                    awardedXp += XpPerCorrectAnswer;
                }

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
