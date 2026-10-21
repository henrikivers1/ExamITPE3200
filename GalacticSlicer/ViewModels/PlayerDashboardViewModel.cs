namespace GalacticSlicer.ViewModels;

// Everything the Progress (dashboard) page shows for the logged-in player
public class PlayerDashboardViewModel
{
    public string Username { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public int Credits { get; set; }

    public int ChallengesCompleted { get; set; }
    public int ChallengesTotal { get; set; }

    public int PlanetsUnlocked { get; set; }
    public int PlanetsTotal { get; set; }

    // Percentages for the progress bars, calculated here instead of in the view.
    // The "== 0 ? 0" check avoids dividing by zero when there is no data yet.
    public int PlanetPercent => PlanetsTotal == 0 ? 0 : (int)Math.Round(100.0 * PlanetsUnlocked / PlanetsTotal);
    public int ChallengePercent => ChallengesTotal == 0 ? 0 : (int)Math.Round(100.0 * ChallengesCompleted / ChallengesTotal);
}