
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

    // Existing XP limits remain compatible with progress earned before
    // mission rewards were standardized. The previous mission must also
    // be completed, so XP alone can never skip a chapter.
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
            >= 7 => 6000 + (planetOrder - 6) * 2000,
            _ => int.MaxValue // Invalid planet orders stay locked
        };
    }
}

