
namespace GalacticSlicer.Api.DTOs;

public class PlayerProgressDto
{
    public int Credits { get; set; }
    public int ExperiencePoints { get; set; }
    public string Rank { get; set; } = string.Empty;
    public int? ActiveCharacterId { get; set; }
}
