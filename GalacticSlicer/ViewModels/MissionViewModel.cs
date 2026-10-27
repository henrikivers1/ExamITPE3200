using GalacticSlicer.Models;

namespace GalacticSlicer.ViewModels;

public class MissionViewModel
{
    public Planet Planet { get; set; } = null!;
    public List<MissionQuestionViewModel> Questions { get; set; } = new();
    public int PassPercent { get; set; } = 80;
}