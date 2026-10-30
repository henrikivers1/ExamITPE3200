
namespace GalacticSlicer.Models;

using System.ComponentModel.DataAnnotations;


public class Challenge
{
    public int ChallengeId { get; set; }

    // Short version of the question, made by the server (not typed in by the user)
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Question { get; set; } = string.Empty;

    // The text of the correct option (the form picks it with a dropdown)
    [Required]
    [Display(Name = "Correct answer")]
    public string CorrectAnswer { get; set; } = string.Empty;

    // Credits for completing it in a mission (questions.json only; user-made challenges give 0)
    [Range(0, 10000)]
    public int CreditReward { get; set; }

    // Foreign key: which planet's mission this question belongs to.
    // Null for challenges made by users, since they are never used in missions.
    [Display(Name = "Planet")]
    public int? PlanetId { get; set; }

    // Navigation property: lets us write challenge.Planet.Name
    public Planet? Planet { get; set; }


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

    // Subject, e.g. "Network Security": from questions.json, or picked in the form for user-made challenges
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