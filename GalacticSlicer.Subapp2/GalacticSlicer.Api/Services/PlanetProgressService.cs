
using GalacticSlicer.Api.Models;
using GalacticSlicer.Api.DTOs;

namespace GalacticSlicer.Api.Services;

public class PlanetProgressService
{
    public bool IsPlanetUnlocked(
        Planet planet,
        PlayerProgress progress,
        IReadOnlyCollection<Planet> planets,
        IReadOnlySet<int> completedPlanetIds)
    {
        if (planet.Order == 1)
        {
            return true;
        }

        var previousPlanets = planets
            .Where(p => p.Order < planet.Order)
            .ToList();

        if (previousPlanets.Count == 0)
        {
            return false;
        }

        return previousPlanets.All(p =>
                   completedPlanetIds.Contains(p.PlanetId)) &&
               progress.ExperiencePoints >=
                   GetRequiredXp(planet.Order);
    }

    // Every planet in chapter order with its locked / unlocked / completed status
    // (used by the Home page's Continue button and the Progress page's starchart)
    public List<PlanetStatusDto> GetPlanetStatuses(
        IEnumerable<Planet> planets, PlayerProgress progress, HashSet<int> completedPlanetIds)
    {
        var orderedPlanets = planets
            .OrderBy(p => p.Order)
            .ToList();

        return orderedPlanets
            .Select(p => new PlanetStatusDto
            {
                PlanetId = p.PlanetId,
                Name = p.Name,
                Description = p.Description,
                Order = p.Order,
                CssClass = p.CssClass,
                Subject = p.Subject,
                StepLabel = $"Chapter {p.Order}",
                IsUnlocked = IsPlanetUnlocked(
                    p,
                    progress,
                    orderedPlanets,
                    completedPlanetIds),
                IsCompleted = completedPlanetIds.Contains(p.PlanetId),
                RequiredXp = GetRequiredXp(p.Order)
            })
            .ToList();
    }

    // Same rule as Subapp 1: each chapter needs one more mission's worth of XP
    // (Tatooine 0, Naboo 2000, Coruscant 4000 ... Endor 10000), so passing every
    // mission once always unlocks the next planet. The previous missions must
    // also be completed, so XP alone can never skip a chapter.
    public int GetRequiredXp(int planetOrder)
    {
        if (planetOrder < 1)
        {
            return int.MaxValue; // Invalid planet orders stay locked
        }

        return (planetOrder - 1) * ChallengeCompletionService.XpPerMission;
    }
}

