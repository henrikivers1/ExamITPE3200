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


