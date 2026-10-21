using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Models;

public class Planet
{
    public int PlanetId { get; set; }

    [Required(ErrorMessage = "Please enter a name.")]
    [StringLength(50, ErrorMessage = "Name can be at most 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Description can be at most 300 characters.")]
    public string? Description { get; set; }

    // Chapter number: 1 = first planet. Task 14 uses it to unlock planets in order.
    [Range(1, 100, ErrorMessage = "Order must be between 1 and 100.")]
    public int Order { get; set; }

    // Matches Filip's CSS classes, e.g. "tatooine" -> .gs-planet--tatooine
    [Required]
    [StringLength(30)]
    public string CssClass { get; set; } = string.Empty;
    // Navigation property: all challenges on this planet
    public List<Challenge>? Challenges { get; set; }
}