using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Api.DTOs;

// DTO sent to React before the user answers.
// It intentionally contains no correct answer.
public class PracticeQuestionDto
{
    public int ChallengeId { get; set; }

    public string Question { get; set; } = string.Empty;

    public string? Subject { get; set; }

    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
}

// DTO received when the user submits an answer.
public class PracticeAnswerDto
{
    [Range(1, int.MaxValue)]
    public int ChallengeId { get; set; }

    [Required]
    public string Answer { get; set; } = string.Empty;
}

// DTO sent back only after an answer is submitted.
public class PracticeAnswerResultDto
{
    public bool IsCorrect { get; set; }

    public string CorrectAnswer { get; set; } = string.Empty;

    public string? Explanation { get; set; }
}
