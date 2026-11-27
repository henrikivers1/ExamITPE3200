using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.ViewModels;

// The Create and Edit forms for a player's own challenge (Views/Challenge/_ChallengeForm.cshtml).
// Only the fields the player fills in: the owner, title, source, planet and reward are set by the server.
public class ChallengeFormViewModel
{
    public int ChallengeId { get; set; }

    [Required(ErrorMessage = "Please choose a subject.")]
    [StringLength(100)]
    public string? Subject { get; set; }

    [Required]
    [StringLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required]
    [StringLength(400)]
    [Display(Name = "Option A")]
    public string OptionA { get; set; } = string.Empty;

    [Required]
    [StringLength(400)]
    [Display(Name = "Option B")]
    public string OptionB { get; set; } = string.Empty;

    [Required]
    [StringLength(400)]
    [Display(Name = "Option C")]
    public string OptionC { get; set; } = string.Empty;

    [Required]
    [StringLength(400)]
    [Display(Name = "Option D")]
    public string OptionD { get; set; } = string.Empty;

    // Which option is correct, as a letter. The controller turns it into the option's text.
    [Required(ErrorMessage = "Please choose the correct answer.")]
    [RegularExpression("^[ABCD]$", ErrorMessage = "Please choose the correct answer.")]
    [Display(Name = "Correct answer")]
    public string? CorrectOption { get; set; }
}
