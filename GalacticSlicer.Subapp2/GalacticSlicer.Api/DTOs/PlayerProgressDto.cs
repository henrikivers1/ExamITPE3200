
namespace GalacticSlicer.Api.DTOs;

public class PlayerProgressDto
{
    public string Username { get; set; } = string.Empty;

    public int Credits { get; set; }
    public int ExperiencePoints { get; set; }
    public string Rank { get; set; } = string.Empty;

    public int XpForCurrentRank { get; set; }
    public int XpForNextRank { get; set; }

    public int ChallengesCompleted { get; set; }
    public int ChallengesTotal { get; set; }

    public int? ActiveCharacterId { get; set; }

    public CharacterDto? ActiveCharacter { get; set; }

    public List<PlanetStatusDto> Planets { get; set; } = new();

    // The latest graded missions, newest first (the mission history)
    public List<MissionAttemptDto> RecentAttempts { get; set; } = new();
}
