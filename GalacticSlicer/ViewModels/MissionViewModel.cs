using GalacticSlicer.Models;

namespace GalacticSlicer.ViewModels;

public class MissionViewModel
{
    public Planet Planet { get; set; } = null!;
    public List<MissionQuestionViewModel> Questions { get; set; } = new();
    public int PassPercent { get; set; }
    // Credits for passing the mission (fixed, the same for 8/10 and 10/10)
    public int CreditReward { get; set; }

    // XP for passing the mission (fixed, the same for 8/10 and 10/10)
    public int XpReward { get; set; }

    // The player's rank and XP before the mission, for the XP bar on the result screen
    // (all 0 / "Outsider" for visitors, who can't submit anyway)
    public string Rank { get; set; } = "Outsider";
    public int ExperiencePoints { get; set; }
    public int XpForCurrentRank { get; set; }
    public int XpForNextRank { get; set; }
}
