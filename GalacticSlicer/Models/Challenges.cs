
namespace ExamITPE3200.Models;
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
    [StringLength(100)]
    public string PlanetName { get; set; } = string.Empty;

}