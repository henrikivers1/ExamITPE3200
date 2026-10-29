
using GalacticSlicer.Models;

namespace GalacticSlicer.Services;

public class PlanetProgressService
{
    public bool IsPlanetUnlocked(Planet planet, PlayerProgress progress)
    {
        if (planet.Order == 1)
            return true;

        // Define the XP required to unlock each planet
        int requiredXp = planet.Order switch
        {
            2 => 500,       // Naboo
            3 => 1500,      // Coruscant
            4 => 3000,      // Kamino
            5 => 5000,      // Hoth
            6 => 8000,      // Endor
            _ => int.MaxValue // Unknown planet orders stay locked
        };

        return progress.ExperiencePoints >= requiredXp;
    }
}
