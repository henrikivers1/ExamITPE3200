
namespace GalacticSlicer.Api.DTOs;

public class PlanetStatusDto : PlanetDto
{
    public string StepLabel { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; }
    public bool IsCompleted { get; set; }
    public int RequiredXp { get; set; }
}
