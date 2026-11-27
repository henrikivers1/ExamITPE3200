
using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Services;

public class ChallengeCompletionService : IChallengeCompletionService
{
    // Game rules in one place, so the controller, the view and this service always agree
    public const int PassPercent = 80;          // % correct needed to pass a mission
    // Fixed XP for passing a planet's mission the first time, no matter if it was 8/10 or 10/10.
    // Each planet unlocks at 2000 XP more than the one before (PlanetProgressService.GetRequiredXp),
    // so one pass always unlocks the next planet, and all 6 missions give 12000 XP = the top rank "Galactic Slicer".
    public const int XpPerMission = 2000;

    // Credits for passing a planet's mission the first time: 500 per chapter
    // (Tatooine 500, Naboo 1000, Coruscant 1500, Kamino 2000, Hoth 2500, Endor 3000).
    // Each pass pays for the next badge in the Shop, which cost 500, 1000, 1500 and 2000.
    public const int CreditsPerChapter = 500;

    public static int CreditsForMission(int planetOrder) => CreditsPerChapter * planetOrder;

    private readonly GalacticSlicerDbContext _context;
    private readonly IRankService _rankService;
    private readonly ILogger<ChallengeCompletionService> _logger;

    public ChallengeCompletionService(
        GalacticSlicerDbContext context,
        IRankService rankService,
        ILogger<ChallengeCompletionService> logger)
    {
        _context = context;
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

            // Find challenges the player has already answered correctly, so they aren't stored twice
            var completedIds = await _context.CompletedChallenges
                .Where(c => c.UserId == userId &&
                            challengeIds.Contains(c.ChallengeId))
                .Select(c => c.ChallengeId)
                .ToListAsync();

            var completedSet = completedIds.ToHashSet();

            // Fixed XP and chapter-based Credits per passed mission (this point is only reached on the first pass)
            int awardedCredits = CreditsForMission(planet.Order);
            int awardedXp = XpPerMission;

            // Remember which questions the player has answered correctly (for the Progress page)
            foreach (var challenge in correctChallenges)
            {
                // Never insert duplicate CompletedChallenge records
                if (!completedSet.Contains(challenge.ChallengeId))
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

    // Saves one graded mission (passed or failed) in the player's mission history.
    // Returns false instead of throwing, so a failed save never breaks the mission result.
    public async Task<bool> SaveAttemptAsync(
        string userId,
        int planetId,
        int correctCount,
        int totalQuestions,
        bool passed)
    {
        if (string.IsNullOrWhiteSpace(userId) ||
            totalQuestions <= 0 ||
            correctCount < 0 ||
            correctCount > totalQuestions)
        {
            _logger.LogWarning(
                "Mission attempt rejected for Planet {PlanetId}: {Correct}/{Total}",
                planetId,
                correctCount,
                totalQuestions);

            return false;
        }

        try
        {
            _context.MissionAttempts.Add(new MissionAttempt
            {
                UserId = userId,
                PlanetId = planetId,
                CorrectCount = correctCount,
                TotalQuestions = totalQuestions,
                Passed = passed,
                AttemptedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save mission attempt for Planet {PlanetId}",
                planetId);

            _context.ChangeTracker.Clear();
            return false;
        }
    }

    // The player's latest mission attempts, newest first, with the planet for its name
    public async Task<List<MissionAttempt>> GetRecentAttemptsAsync(string userId, int count)
    {
        return await _context.MissionAttempts
            .Include(a => a.Planet)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AttemptedAt)
            .ThenByDescending(a => a.Id)
            .Take(count)
            .ToListAsync();
    }
}
