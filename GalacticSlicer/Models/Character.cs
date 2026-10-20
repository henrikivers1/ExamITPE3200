using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Models;

public class Character
{
    public int Id {get; set;}

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Name {get; set;} = string.Empty;

    [StringLength(100, MinimumLength = 3)]
    public string? Description {get; set;}

    [Range(0, int.MaxValue)]
    public int Price {get; set;}

    public string? ImageUrl {get; set;}

}

/*
 * Id          - Unique ID for each character.
 * Name        - Character name, e.g. Kael Voss.
 * Description - Short description of the character.
 * Price       - Number of Credits required to purchase the character.
 * ImageUrl    - Path to the character image, e.g. /images/characters/kael-voss.png.
 */