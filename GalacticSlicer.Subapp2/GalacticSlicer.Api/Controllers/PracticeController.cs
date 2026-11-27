
using System.ComponentModel.DataAnnotations;
using GalacticSlicer.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/practice")]
public class PracticeController : ControllerBase
{
    private readonly GalacticSlicerDbContext _db;
    private readonly ILogger<PracticeController> _logger;

    private const int QuestionsPerRound = 10;

    public PracticeController(
        GalacticSlicerDbContext db,
        ILogger<PracticeController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET /api/practice/questions?subject=Linux%20Command%20Line
    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] string? subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            return BadRequest(new
            {
                message = "A subject is required."
            });
        }

        var questions = await _db.Challenges
            .AsNoTracking()
            .Where(c =>
                c.SourceId != null &&
                c.CreatedByUserId == null &&
                c.PlanetId != null &&
                c.Subject == subject)
            .Select(c => new PracticeQuestionDto
            {
                ChallengeId = c.ChallengeId,
                Question = c.Question,
                Subject = c.Subject,
                OptionA = c.OptionA,
                OptionB = c.OptionB,
                OptionC = c.OptionC,
                OptionD = c.OptionD
            })
            .ToListAsync();

        // Randomise the question order in memory.
        // SQLite does not need any special random SQL support.
        Random.Shared.Shuffle(
            System.Runtime.InteropServices.CollectionsMarshal
                .AsSpan(questions));

        var selected = questions
            .Take(QuestionsPerRound)
            .ToList();

        return Ok(selected);
    }

    // POST /api/practice/check
    [HttpPost("check")]
    public async Task<IActionResult> CheckAnswer(
        [FromBody] PracticeAnswerDto dto)
    {
        if (dto.ChallengeId <= 0 ||
            string.IsNullOrWhiteSpace(dto.Answer))
        {
            return BadRequest(new
            {
                message = "A challenge ID and answer are required."
            });
        }

        var challenge = await _db.Challenges
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.ChallengeId == dto.ChallengeId &&
                c.SourceId != null &&
                c.CreatedByUserId == null &&
                c.PlanetId != null);

        if (challenge == null)
        {
            return NotFound(new
            {
                message = "Practice question not found."
            });
        }

        // Compare the supplied answer with the server-side
        // correct answer stored in SQLite.
        bool isCorrect = string.Equals(
            dto.Answer.Trim(),
            challenge.CorrectAnswer.Trim(),
            StringComparison.OrdinalIgnoreCase);

        _logger.LogInformation(
            "Practice answer checked for challenge {ChallengeId}",
            challenge.ChallengeId);

        return Ok(new PracticeAnswerResultDto
        {
            IsCorrect = isCorrect,
            CorrectAnswer = challenge.CorrectAnswer,
            Explanation = challenge.Explanation
        });
    }
}

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
