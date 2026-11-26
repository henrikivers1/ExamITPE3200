using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Models;

// An official question the player has already been rewarded for, so rewards are only given once
public class CompletedChallenge
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int ChallengeId { get; set; }

    public Challenge Challenge { get; set; } = null!;

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}