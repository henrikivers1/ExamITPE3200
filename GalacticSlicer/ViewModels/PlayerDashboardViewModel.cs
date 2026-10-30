using GalacticSlicer.Models;

namespace GalacticSlicer.ViewModels;

// Everything the Progress (dashboard) page shows for the logged-in player
public class PlayerDashboardViewModel
{
    public string Username { get; set; } = string.Empty;

    // Equipped character, shown as a badge next to the username (null = nothing equipped)
    public Character? ActiveCharacter { get; set; }
    public string Rank { get; set; } = string.Empty;
    public int Credits { get; set; }

    public int ChallengesCompleted { get; set; }
    public int ChallengesTotal { get; set; }

    public int PlanetsUnlocked { get; set; }
    public int PlanetsTotal { get; set; }

    public int ExperiencePoints { get; set; }

    // XP thresholds for current and next rank
    public int XpForCurrentRank { get; set; }
    public int XpForNextRank { get; set; }

    // Percentages for progress bars
    public int PlanetPercent =>
        PlanetsTotal == 0 ? 0 :
        (int)Math.Round(100.0 * PlanetsUnlocked / PlanetsTotal);

    public int ChallengePercent =>
        ChallengesTotal == 0 ? 0 :
        (int)Math.Round(100.0 * ChallengesCompleted / ChallengesTotal);

    // Progress towards the next rank
    public int XpPercent =>
        XpForNextRank > XpForCurrentRank
            ? Math.Clamp(
                (int)((ExperiencePoints - XpForCurrentRank) * 100.0 /
                (XpForNextRank - XpForCurrentRank)), 0, 100)
            : 100;
}
