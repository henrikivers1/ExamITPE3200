using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Models;

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


/*
 * Id               - Unique ID for the ownership record.
 * UserId           - ID of the Identity user who owns the character.
 * PlayerProgressId - Foreign key to the player's progress record (nullable for older purchases).
 * PlayerProgress   - Navigation property to the PlayerProgress model.
 * CharacterId      - Foreign key to the owned character.
 * Character        - Navigation property to the Character model.
 * PurchasedAt      - Date and time when the character was purchased.
 */

