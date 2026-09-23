namespace ExamITPE3200.Models;
public class ProgressViewModel
{
    public required string Username { get; set; }
    public DateTime MemberSince { get; set; }
    public int Credits { get; set; }
    public int ChallengesCompleted { get; set; }
    public int ChallengesTotal { get; set; }
    public int PlanetsUnlocked { get; set; }
    public int PlanetsTotal { get; set; }
    public required string Rank { get; set; }
}
