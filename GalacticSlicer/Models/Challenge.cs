
namespace GalacticSlicer.Models;

using System.ComponentModel.DataAnnotations;


public class Challenge
{
    public int ChallengeId { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string CorrectAnswer { get; set; } = string.Empty;

    [Range(100, 10000)]
    public int CreditReward { get; set; }

    [Required]
    // Foreign key: which planet this challenge belongs to
    [Display(Name = "Planet")]
    [Range(1, int.MaxValue, ErrorMessage = "Please choose a planet.")]
    public int PlanetId { get; set; }

    // Navigation property: lets us write challenge.Planet.Name
    public Planet? Planet { get; set; }


    [Required]
    [StringLength(400)]
    public string OptionA { get; set; } = string.Empty;

    [Required]
    [StringLength(400)]
    public string OptionB { get; set; } = string.Empty;

    [Required]
    [StringLength(400)]
    public string OptionC { get; set; } = string.Empty;

    [Required]
    [StringLength(400)]
    public string OptionD { get; set; } = string.Empty;

    // Subject from questions.json, e.g. "Network Security" (null for questions made by users)
    [StringLength(100)]
    public string? Subject { get; set; }

    // Why the correct answer is right, shown after answering
    [StringLength(500)]
    public string? Explanation { get; set; }

    // The question's id in questions.json, e.g. "net-03".
    // Only questions with a SourceId are used in planet missions, so questions made by users never end up there.
    [StringLength(50)]
    public string? SourceId { get; set; }

    // The user who created the question (null for questions.json). Only they can see, edit and delete it.
    [StringLength(450)]
    public string? CreatedByUserId { get; set; }

}