
using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Api.DTOs;

public class ChallengeUpsertDto
{
    [Required]
    [StringLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required, StringLength(400)]
    public string OptionA { get; set; } = string.Empty;

    [Required, StringLength(400)]
    public string OptionB { get; set; } = string.Empty;

    [Required, StringLength(400)]
    public string OptionC { get; set; } = string.Empty;

    [Required, StringLength(400)]
    public string OptionD { get; set; } = string.Empty;

    [Required]
    public string CorrectAnswer { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Subject { get; set; }

    [StringLength(500)]
    public string? Explanation { get; set; }
}
