using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Api.Models;

// Every graded mission, passed or failed (the mission history on the Progress page),
// like Models/MissionAttempt.cs in Subapp 1.
// CompletedMission only stores the first pass, which decides the rewards.
public class MissionAttempt
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int PlanetId { get; set; }

    public Planet Planet { get; set; } = null!;

    public int CorrectCount { get; set; }

    public int TotalQuestions { get; set; }

    public bool Passed { get; set; }

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}
