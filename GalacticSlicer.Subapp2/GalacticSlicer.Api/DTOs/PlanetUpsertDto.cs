
using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Api.DTOs;

public class PlanetUpsertDto
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    [Range(1, 100)]
    public int Order { get; set; }

    [Required]
    [StringLength(30)]
    public string CssClass { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Subject { get; set; }
}
