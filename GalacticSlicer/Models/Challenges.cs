
namespace ExamITPE3200.Models;


public class Challenge
{
    public int ChallengeId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Question { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;

    public int CreditReward { get; set; }

    public string PlanetName { get; set; } = string.Empty;


}