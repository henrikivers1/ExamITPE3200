
using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Api.DTOs;

public class MissionSubmitDto
{
    [Required]
    public Dictionary<int, string> Answers { get; set; } = new();
}
