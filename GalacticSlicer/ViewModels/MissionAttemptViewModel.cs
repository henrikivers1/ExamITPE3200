namespace GalacticSlicer.ViewModels;

// One row in the mission history on the Progress page
public class MissionAttemptViewModel
{
    public string PlanetName { get; set; } = string.Empty;
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public bool Passed { get; set; }
    public DateTime AttemptedAt { get; set; }
}
