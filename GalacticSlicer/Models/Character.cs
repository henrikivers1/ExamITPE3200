using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Models;

public class Character
{
    public int CharacterId { get; set; }

    [Required(ErrorMessage = "Please enter a name.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Description can be at most 300 characters.")]
    public string? Description { get; set; }

    [Range(0, 100000, ErrorMessage = "Price must be between 0 and 100000 Credits.")]
    public int Price { get; set; }

    public string? ImageUrl { get; set; }
}

/*
 * CharacterId - Unique ID for each character.
 * Name        - Character name, e.g. Kael Voss.
 * Description - Short description of the character.
 * Price       - Number of Credits required to purchase the character.
 * ImageUrl    - Path to the character image, e.g. /images/characters/kael-voss.png.
 */