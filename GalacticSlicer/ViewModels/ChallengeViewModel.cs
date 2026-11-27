namespace GalacticSlicer.ViewModels;

// One of the player's own challenges, as shown on the Challenges list, Details and Delete pages
public class ChallengeViewModel
{
    public int ChallengeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Question { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
}
