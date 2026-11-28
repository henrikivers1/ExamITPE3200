using Microsoft.Extensions.Caching.Memory;

namespace GalacticSlicer.Api.Services;

// Remembers which questions GET /api/missions/{planetId} handed out to a player,
// so SubmitMission only grades those (like TempData in Subapp 1's PlanetController).
// Without it a player could pick the questions they already know and submit those instead.
// Kept in memory: after an API restart the player just starts the mission again.
public class MissionQuestionStore
{
    // long enough for a slow player, short enough that old missions don't pile up
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(2);

    private readonly IMemoryCache _cache;

    public MissionQuestionStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    private static string Key(string userId, int planetId) =>
        $"mission:{userId}:{planetId}";

    // A new GET replaces the previous set, so only the newest mission can be submitted
    public void Save(string userId, int planetId, IEnumerable<int> challengeIds)
    {
        _cache.Set(
            Key(userId, planetId),
            challengeIds.ToHashSet(),
            Lifetime);
    }

    // null when no mission was started (or it expired)
    public IReadOnlySet<int>? Get(string userId, int planetId)
    {
        return _cache.TryGetValue(Key(userId, planetId), out HashSet<int>? ids)
            ? ids
            : null;
    }

    // Called after grading, so the same answers can't be submitted twice
    public void Remove(string userId, int planetId)
    {
        _cache.Remove(Key(userId, planetId));
    }
}
