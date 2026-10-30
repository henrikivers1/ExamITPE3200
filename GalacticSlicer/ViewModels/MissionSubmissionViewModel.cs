using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.ViewModels;

// What the Mission page's JavaScript sends to PlanetController.SubmitMission
public class MissionSubmissionViewModel
{
    [Range(1, int.MaxValue)]
    public int PlanetId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one answer is required.")]
    public List<MissionAnswerViewModel> Answers { get; set; } = new();
}

public class MissionAnswerViewModel
{
    [Range(1, int.MaxValue)]
    public int ChallengeId { get; set; }

    [Required]
    [StringLength(200)]   // same max length as OptionA-D on Challenge
    public string SelectedAnswer { get; set; } = string.Empty;
}
