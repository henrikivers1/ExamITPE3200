
using GalacticSlicer.Models;

namespace GalacticSlicer.Services;

public class PlanetProgressService
{
    public bool IsPlanetUnlocked(Planet planet, PlayerProgress progress)
    {
        if (planet.Order == 1)
            return true;

        return progress.ExperiencePoints >= GetRequiredXp(planet.Order);
    }

    // Every planet in chapter order with its locked / unlocked / completed status
    // (used by the Home page's Continue button and the Progress page's starchart)
    public List<PlanetViewModel> GetPlanetStatuses(
        IEnumerable<Planet> planets, PlayerProgress progress, HashSet<int> completedPlanetIds)
    {
        return planets
            .OrderBy(p => p.Order)
            .Select(p => new PlanetViewModel
            {
                PlanetId = p.PlanetId,
                Name = p.Name,
                CssClass = p.CssClass,
                StepLabel = $"Chapter {p.Order}",
                IsUnlocked = IsPlanetUnlocked(p, progress),
                IsCompleted = completedPlanetIds.Contains(p.PlanetId)
            })
            .ToList();
    }

    // Define the XP required to unlock each planet (also shown on the Progress page)
    public int GetRequiredXp(int planetOrder)
    {
        return planetOrder switch
        {
            1 => 0,         // Tatooine
            2 => 500,       // Naboo
            3 => 1500,      // Coruscant
            4 => 3000,      // Kamino
            5 => 5000,      // Hoth
            6 => 6000,      // Endor
            _ => int.MaxValue // Unknown planet orders stay locked
        };
    }
}
