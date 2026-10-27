
namespace GalacticSlicer.ViewModels;

public class MissionSubmissionViewModel
{
    public int PlanetId { get; set; }

    public List<MissionAnswerViewModel> Answers { get; set; } = new();
}

public class MissionAnswerViewModel
{
    public int ChallengeId { get; set; }

    public string SelectedAnswer { get; set; } = string.Empty;
}
