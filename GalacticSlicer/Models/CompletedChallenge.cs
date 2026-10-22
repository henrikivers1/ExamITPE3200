using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Models;

public class CompletedChallenge
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int ChallengeId { get; set; }

    public Challenge Challenge { get; set; } = null!;

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

}

/*
 * Id          - Unique ID for the completed challenge record.
 * UserId      - ID of the player who completed the challenge.
 * ChallengeId - ID of the completed challenge.
 * Challenge   - Navigation property to the Challenge model.
 * CompletedAt - Date and time when the challenge was completed.
 */