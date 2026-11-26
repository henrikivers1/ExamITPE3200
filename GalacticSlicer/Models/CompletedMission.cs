using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Models;

// A planet mission the player has passed at least once (shown as "Completed" on the Galaxy page)
public class CompletedMission
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int PlanetId { get; set; }

    public Planet Planet { get; set; } = null!;

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}