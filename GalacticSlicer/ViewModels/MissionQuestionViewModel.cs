namespace GalacticSlicer.ViewModels;

public class MissionQuestionViewModel
{
    public int ChallengeId { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}