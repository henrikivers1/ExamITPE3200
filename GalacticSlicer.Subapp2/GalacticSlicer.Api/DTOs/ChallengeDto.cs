
namespace GalacticSlicer.Api.DTOs;

public class ChallengeDto
{
    public int ChallengeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;

    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;

    public string? Subject { get; set; }
    public int? PlanetId { get; set; }
    public int CreditReward { get; set; }
}
