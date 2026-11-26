using GalacticSlicer.Models;

namespace GalacticSlicer.Services;

// Which planets a player has unlocked, and how much XP each planet needs
public interface IPlanetProgressService
{
    bool IsPlanetUnlocked(Planet planet, PlayerProgress progress);
    List<PlanetViewModel> GetPlanetStatuses(IEnumerable<Planet> planets, PlayerProgress progress, HashSet<int> completedPlanetIds);
    int GetRequiredXp(int planetOrder);
}

