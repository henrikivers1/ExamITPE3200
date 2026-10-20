using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Models;

public class UserCharacter
{
    public int Id {get; set;}

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int CharacterId {get; set;}

    public Character Character {get; set;} = null!;

    public DateTime PurchasedAt {get; set;} = DateTime.UtcNow;
}

/*
 * Id          - Unique ID for the ownership record.
 * UserId      - ID of the player who owns the character.
 * CharacterId - ID of the owned character.
 * Character   - Navigation property to the Character model.
 * PurchasedAt - Date and time when the character was purchased.
 */
 