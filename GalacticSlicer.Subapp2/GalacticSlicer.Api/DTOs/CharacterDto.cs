
namespace GalacticSlicer.Api.DTOs;

public class CharacterDto
{
    public int CharacterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Price { get; set; }
    public string? ImageUrl { get; set; }
}