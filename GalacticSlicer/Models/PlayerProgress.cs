using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Models;

public class PlayerProgress
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Credits { get; set; } = 0;

    public string Rank { get; set; } = "Rookie";

    public int? ActiveCharacterId { get; set; }

    public Character? ActiveCharacter { get; set; }

    [Range(0, int.MaxValue)]
    public int ExperiencePoints { get; set; } = 0;
}

/*
 * Id                - Unique ID for the player's progress.
 * UserId            - ID of the player (connected to ASP.NET Identity later).
 * Credits           - Number of Credits the player currently owns.
 * Rank              - Current player rank, starting at Rookie.
 * ActiveCharacterId - ID of the character currently equipped.
 * ActiveCharacter   - Navigation property to the selected Character.
 * ExpiriencePoints  - For your Rank
 */