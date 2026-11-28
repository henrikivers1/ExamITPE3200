using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Api.Services;

// Builds everything the Progress page shows (GET /api/progress/me),
// so ProgressController never touches the DbContext.
public class ProgressDashboardService
{
    // Same as the Progress page in Subapp 1
    public const int HistoryLength = 5;

    private readonly GalacticSlicerDbContext _context;
    private readonly PlayerProgressService _progressService;
    private readonly RankService _rankService;
    private readonly PlanetProgressService _planetProgressService;
    private readonly ChallengeCompletionService _completionService;

    public ProgressDashboardService(
        GalacticSlicerDbContext context,
        PlayerProgressService progressService,
        RankService rankService,
        PlanetProgressService planetProgressService,
        ChallengeCompletionService completionService)
    {
        _context = context;
        _progressService = progressService;
        _rankService = rankService;
        _planetProgressService = planetProgressService;
        _completionService = completionService;
    }

    public async Task<PlayerProgressDto> GetDashboardAsync(
        string userId,
        string username)
    {
        var progress = await _progressService
            .GetOrCreateProgressAsync(userId);

        var xpRange = _rankService
            .GetXpRange(progress.ExperiencePoints);

        var challengesCompleted = await _context.CompletedChallenges
            .CountAsync(cc => cc.UserId == userId);

        var challengesTotal = await _context.Challenges
            .CountAsync(c => c.SourceId != null);

        var planets = await _context.Planets
            .AsNoTracking()
            .OrderBy(p => p.Order)
            .ToListAsync();

        var completedPlanetIds = await _completionService
            .GetCompletedPlanetIdsAsync(userId);

        var planetStatuses = _planetProgressService
            .GetPlanetStatuses(
                planets,
                progress,
                completedPlanetIds);

        var recentAttempts = await _completionService
            .GetRecentAttemptsAsync(userId, HistoryLength);

        CharacterDto? activeCharacter = null;

        if (progress.ActiveCharacterId.HasValue)
        {
            var character = await _context.Characters
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.CharacterId == progress.ActiveCharacterId.Value);

            if (character != null)
            {
                activeCharacter = new CharacterDto
                {
                    CharacterId = character.CharacterId,
                    Name = character.Name,
                    Description = character.Description,
                    Price = character.Price,
                    ImageUrl = character.ImageUrl
                };
            }
        }

        return new PlayerProgressDto
        {
            Username = username,
            Credits = progress.Credits,
            ExperiencePoints = progress.ExperiencePoints,
            Rank = _rankService.GetRank(progress.ExperiencePoints),
            XpForCurrentRank = xpRange.Current,
            XpForNextRank = xpRange.Next,
            ChallengesCompleted = challengesCompleted,
            ChallengesTotal = challengesTotal,
            ActiveCharacterId = progress.ActiveCharacterId,
            ActiveCharacter = activeCharacter,
            Planets = planetStatuses,
            RecentAttempts = recentAttempts
                .Select(a => new MissionAttemptDto
                {
                    PlanetName = a.Planet.Name,
                    CorrectCount = a.CorrectCount,
                    TotalQuestions = a.TotalQuestions,
                    Passed = a.Passed,
                    // Saved as UTC; SQLite returns it without a time zone,
                    // so mark it, and the JSON then ends with "Z"
                    AttemptedAt = DateTime.SpecifyKind(
                        a.AttemptedAt,
                        DateTimeKind.Utc)
                })
                .ToList()
        };
    }
}
