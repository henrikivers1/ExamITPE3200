using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Models;

// Join table for the many-to-many relation: which characters each player owns
public class UserCharacter
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    // Connect ownership to the player's domain progress record
    public int? PlayerProgressId { get; set; }

    public PlayerProgress? PlayerProgress { get; set; }

    public int CharacterId { get; set; }

    public Character Character { get; set; } = null!;

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
}
