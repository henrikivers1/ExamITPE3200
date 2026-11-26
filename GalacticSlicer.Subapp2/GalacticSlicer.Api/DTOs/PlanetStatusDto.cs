
namespace GalacticSlicer.Api.DTOs;

public class PlanetStatusDto
{
    public int PlanetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public string StepLabel { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; }
    public bool IsCompleted { get; set; }
}
