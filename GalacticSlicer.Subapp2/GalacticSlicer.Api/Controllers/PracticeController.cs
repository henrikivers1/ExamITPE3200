using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/practice")]
public class PracticeController : ControllerBase
{
    private readonly PracticeService _practiceService;
    private readonly ILogger<PracticeController> _logger;

    public PracticeController(
        PracticeService practiceService,
        ILogger<PracticeController> logger)
    {
        _practiceService = practiceService;
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

        var selected = await _practiceService.GetRoundAsync(subject);

        _logger.LogInformation(
            "Practice round for {Subject}: {Count} questions",
            subject,
            selected.Count);

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

        var result = await _practiceService.CheckAnswerAsync(
            dto.ChallengeId,
            dto.Answer);

        if (result == null)
        {
            _logger.LogWarning(
                "Practice answer for unknown question {ChallengeId}",
                dto.ChallengeId);

            return NotFound(new
            {
                message = "Practice question not found."
            });
        }

        _logger.LogInformation(
            "Practice answer checked for challenge {ChallengeId}",
            dto.ChallengeId);

        return Ok(result);
    }
}
