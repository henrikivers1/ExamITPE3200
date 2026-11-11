
namespace GalacticSlicer.Api.DTOs;

public class PlanetDto
{
    public int PlanetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public string CssClass { get; set; } = string.Empty;
    public string? Subject { get; set; }
}
