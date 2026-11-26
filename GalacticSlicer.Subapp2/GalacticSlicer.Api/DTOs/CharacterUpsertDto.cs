
using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Api.DTOs;

public class CharacterUpsertDto
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    [Range(0, 100000)]
    public int Price { get; set; }

    [Url]
    public string? ImageUrl { get; set; }
}
