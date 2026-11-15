using GalacticSlicer.Models;

namespace GalacticSlicer.ViewModels;

public class MissionViewModel
{
    public Planet Planet { get; set; } = null!;
    public List<MissionQuestionViewModel> Questions { get; set; } = new();
    public int PassPercent { get; set; }
    public int TotalCreditReward { get; set; }

    // Most XP the mission can give (every question answered correctly)
    public int MaxXpReward { get; set; }

    // The player's rank and XP before the mission, for the XP bar on the result screen
    // (all 0 / "Outsider" for visitors, who can't submit anyway)
    public string Rank { get; set; } = "Outsider";
    public int ExperiencePoints { get; set; }
    public int XpForCurrentRank { get; set; }
    public int XpForNextRank { get; set; }
}
