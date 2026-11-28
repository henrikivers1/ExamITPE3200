namespace GalacticSlicer.Api.DTOs;

// One row in the mission history on the Progress page,
// like MissionAttemptViewModel in Subapp 1.
public class MissionAttemptDto
{
    public string PlanetName { get; set; } = string.Empty;

    public int CorrectCount { get; set; }

    public int TotalQuestions { get; set; }

    public bool Passed { get; set; }

    // UTC; the client shows it in the player's local time
    public DateTime AttemptedAt { get; set; }
}
