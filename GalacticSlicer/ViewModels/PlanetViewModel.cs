namespace GalacticSlicer.Models;
public class PlanetViewModel
{
    public required string Name { get; set; }
    public required string CssClass { get; set; }
    public required string StepLabel { get; set; }
    public bool IsUnlocked { get; set; }
    // True once the player has passed this planet's mission
    public bool IsCompleted { get; set; }
    public int PlanetId { get; set; }
}
