using GalacticSlicer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Api.Services;

// Data access for MissionsController, so the controller never touches the DbContext.
// Picks the mission questions, checks whether a planet is unlocked and
// loads the questions that were handed out for grading.
public class MissionService
{
    private readonly GalacticSlicerDbContext _context;
    private readonly ChallengeCompletionService _completionService;
    private readonly PlanetProgressService _planetProgressService;
    private readonly ILogger<MissionService> _logger;

    public MissionService(
        GalacticSlicerDbContext context,
        ChallengeCompletionService completionService,
        PlanetProgressService planetProgressService,
        ILogger<MissionService> logger)
    {
        _context = context;
        _completionService = completionService;
        _planetProgressService = planetProgressService;
        _logger = logger;
    }

    // Endor is the final mission: no subject of its own, questions from every subject
    public static bool IsFinalExam(Planet planet) =>
        planet.Name.Equals(
            "Endor", StringComparison.OrdinalIgnoreCase)
        && planet.Subject == null;

    public async Task<Planet?> GetPlanetAsync(int planetId)
    {
        return await _context.Planets
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PlanetId == planetId);
    }

    // Same rule as the Galaxy page: the previous missions passed and enough XP
    public async Task<bool> IsUnlockedAsync(
        Planet planet,
        PlayerProgress progress,
        string userId)
    {
        var planets = await _context.Planets
            .AsNoTracking()
            .OrderBy(p => p.Order)
            .ToListAsync();

        var completedPlanetIds = await _completionService
            .GetCompletedPlanetIdsAsync(userId);

        return _planetProgressService.IsPlanetUnlocked(
            planet,
            progress,
            planets,
            completedPlanetIds);
    }

    // 10 random official questions, like QuestionService in Subapp 1:
    // the planet's own subject, or every subject before it for Endor.
    public async Task<List<Challenge>> PickQuestionsAsync(Planet planet)
    {
        var query = _context.Challenges
            .AsNoTracking()
            .Where(c =>
                c.SourceId != null &&
                c.CreatedByUserId == null &&
                c.PlanetId.HasValue);

        if (IsFinalExam(planet))
        {
            var previousPlanetIds = await _context.Planets
                .Where(p =>
                    p.Order < planet.Order &&
                    p.Subject != null)
                .Select(p => p.PlanetId)
                .ToListAsync();

            query = query.Where(c =>
                previousPlanetIds.Contains(c.PlanetId!.Value));
        }
        else
        {
            query = query.Where(c =>
                c.PlanetId == planet.PlanetId &&
                c.Subject == planet.Subject);
        }

        var pool = await query.ToListAsync();

        if (pool.Count == 0)
        {
            _logger.LogWarning(
                "No mission questions for planet {PlanetId}",
                planet.PlanetId);
        }

        return pool
            .OrderBy(_ => Random.Shared.Next())
            .Take(ChallengeCompletionService.QuestionsPerMission)
            .ToList();
    }

    // The questions that were handed out, with their correct answers, for grading
    public async Task<List<Challenge>> GetQuestionsAsync(
        IReadOnlySet<int> challengeIds)
    {
        return await _context.Challenges
            .AsNoTracking()
            .Where(c => challengeIds.Contains(c.ChallengeId))
            .ToListAsync();
    }

    // true when the player has passed this mission before (a replay gives no rewards)
    public async Task<bool> IsCompletedAsync(string userId, int planetId)
    {
        return await _context.CompletedMissions
            .AnyAsync(m =>
                m.UserId == userId &&
                m.PlanetId == planetId);
    }
}
