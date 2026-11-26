
using GalacticSlicer.Models;
using GalacticSlicer.ViewModels;

namespace GalacticSlicer.Services;

public class PlanetProgressService : IPlanetProgressService
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

    // XP required to unlock each planet (also shown on the Progress page):
    // one passed mission's XP per chapter, so passing a planet's mission always unlocks the next one.
    // Tatooine 0, Naboo 2000, Coruscant 4000, Kamino 6000, Hoth 8000, Endor 10000,
    // and planets added by an admin (chapter 7+) continue with 2000 more per chapter.
    public int GetRequiredXp(int planetOrder)
    {
        return Math.Max(0, planetOrder - 1) * ChallengeCompletionService.XpPerMission;
    }
}
