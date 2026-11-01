using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Models;

public class PlayerProgress
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Credits { get; set; } = 0;

    public string Rank { get; set; } = "Outsider";

    public int? ActiveCharacterId { get; set; }

    public Character? ActiveCharacter { get; set; }

    // Characters owned by this player
    public ICollection<UserCharacter> OwnedCharacters { get; set; }
        = new List<UserCharacter>();

    [Range(0, int.MaxValue)]
    public int ExperiencePoints { get; set; } = 0;
}


/*
 * Id                - Unique ID for the player's progress record.
 * UserId            - ID of the associated Identity user.
 * Credits           - Current Credits balance of the player.
 * Rank              - Current rank based on the player's experience.
 * ActiveCharacterId - Foreign key to the currently equipped character (nullable).
 * ActiveCharacter   - Navigation property to the equipped character.
 * ExperiencePoints  - Total XP earned by the player.
 * OwnedCharacters   - Collection of ownership records linking the player to characters (N:M).
 */
