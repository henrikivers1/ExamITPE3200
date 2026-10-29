
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
}
