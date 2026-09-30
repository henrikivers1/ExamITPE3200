namespace GalacticSlicer.Models;
public class PlanetViewModel
{
    public required string Name { get; set; }
    public required string CssClass { get; set; }
    public required string StepLabel { get; set; }
    public required int ChallengeCount { get; set; }
    public bool IsUnlocked { get; set; }
}
