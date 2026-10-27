using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Services;

public class ChallengeCompletionService
{
    private readonly GalacticSlicerDbContext _context;
    private readonly PlayerProgressService _progressService;
    private readonly RankService _rankService;

    public ChallengeCompletionService(
        GalacticSlicerDbContext context,
        PlayerProgressService progressService,
        RankService rankService)
    {
        _context = context;
        _progressService = progressService;
        _rankService = rankService;
    }

    public async Task<bool> CompleteChallengeAsync(
        string userId,
        int challengeId)
    {
        // Find the challenge
        var challenge = await _context.Challenges
            .FirstOrDefaultAsync(c => c.ChallengeId == challengeId);

        if (challenge == null)
            return false;

        // Check whether the player has already completed it
        bool alreadyCompleted = await _context.CompletedChallenges
            .AnyAsync(c => c.UserId == userId &&
                           c.ChallengeId == challengeId);

        if (alreadyCompleted)
            return false;

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

        return true;
    }
}
