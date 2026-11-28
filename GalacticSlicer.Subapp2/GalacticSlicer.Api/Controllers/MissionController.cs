using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Models;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/missions")]
public class MissionsController : ControllerBase
{
    private readonly MissionService _missionService;
    private readonly ChallengeCompletionService _completionService;
    private readonly PlayerProgressService _progressService;
    private readonly MissionQuestionStore _questionStore;
    private readonly ILogger<MissionsController> _logger;

    public MissionsController(
        MissionService missionService,
        ChallengeCompletionService completionService,
        PlayerProgressService progressService,
        MissionQuestionStore questionStore,
        ILogger<MissionsController> logger)
    {
        _missionService = missionService;
        _completionService = completionService;
        _progressService = progressService;
        _questionStore = questionStore;
        _logger = logger;
    }

    private string? UserId =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    // GET /api/missions/1
    // 10 random questions, sent without the correct answers.
    // Completed missions can be played again (without new rewards).
    [HttpGet("{planetId:int}")]
    public async Task<IActionResult> GetMission(int planetId)
    {
        var userId = UserId;

        if (userId == null) return Unauthorized();

        var planet = await _missionService.GetPlanetAsync(planetId);
        if (planet == null) return NotFound();

        var progress = await _progressService
            .GetOrCreateProgressAsync(userId);

        if (!await _missionService.IsUnlockedAsync(
            planet,
            progress,
            userId))
        {
            _logger.LogWarning(
                "User {UserId} tried locked planet {PlanetId}",
                userId,
                planetId);

            return StatusCode(403, new
            {
                message = "Planet is locked."
            });
        }

        var challenges = await _missionService.PickQuestionsAsync(planet);

        if (challenges.Count == 0)
        {
            return NotFound(new
            {
                message = "No mission questions are configured."
            });
        }

        _logger.LogInformation(
            "Mission {PlanetId} started by {UserId} with {Count} questions",
            planetId,
            userId,
            challenges.Count);

        // Only these questions can be submitted for this mission
        _questionStore.Save(
            userId,
            planetId,
            challenges.Select(c => c.ChallengeId));

        return Ok(challenges.Select(c => new ChallengeDto
        {
            ChallengeId = c.ChallengeId,
            Title = c.Title,
            Question = c.Question,
            OptionA = c.OptionA,
            OptionB = c.OptionB,
            OptionC = c.OptionC,
            OptionD = c.OptionD,
            Subject = c.Subject,
            PlanetId = c.PlanetId,
            CreditReward = c.CreditReward
        }));
    }

    // POST /api/missions/1/submit
    // Grades the questions GetMission handed out. Rewards are only given
    // the first time a mission is passed; a replay shows the score only.
    [HttpPost("{planetId:int}/submit")]
    public async Task<IActionResult> SubmitMission(
        int planetId,
        [FromBody] MissionSubmitDto dto)
    {
        var userId = UserId;

        if (userId == null) return Unauthorized();

        var planet = await _missionService.GetPlanetAsync(planetId);
        if (planet == null) return NotFound();

        var progress = await _progressService
            .GetOrCreateProgressAsync(userId);

        if (!await _missionService.IsUnlockedAsync(
            planet,
            progress,
            userId))
        {
            _logger.LogWarning(
                "User {UserId} tried locked planet {PlanetId}",
                userId,
                planetId);

            return StatusCode(403, new
            {
                message = "Planet is locked."
            });
        }

        if (dto?.Answers == null || dto.Answers.Count == 0)
        {
            return BadRequest(new
            {
                message = "Mission answers are required."
            });
        }

        var handedOut = _questionStore.Get(userId, planetId);

        if (handedOut == null)
        {
            _logger.LogWarning(
                "Mission {PlanetId} submitted by {UserId} without an active mission",
                planetId,
                userId);

            return Conflict(new
            {
                // already submitted, expired, or never started
                message =
                    "This mission is no longer active. " +
                    "Start it again to get new questions."
            });
        }

        // Exactly the handed-out questions, all answered
        if (!handedOut.SetEquals(dto.Answers.Keys))
        {
            _logger.LogWarning(
                "Mission {PlanetId} submitted by {UserId} with questions that were not handed out",
                planetId,
                userId);

            return BadRequest(new
            {
                message = "You must answer all mission questions."
            });
        }

        if (dto.Answers.Values.Any(string.IsNullOrWhiteSpace))
        {
            return BadRequest(new
            {
                message = "Answers cannot be empty."
            });
        }

        // Grade on the server, for the result screen.
        // CompletePassedMissionAsync checks the answers again before giving rewards.
        var questions = await _missionService.GetQuestionsAsync(handedOut);

        int correctCount = questions.Count(c => string.Equals(
            dto.Answers[c.ChallengeId],
            c.CorrectAnswer,
            StringComparison.OrdinalIgnoreCase));

        bool passed = questions.Count > 0 &&
            correctCount * 100.0 / questions.Count >=
                ChallengeCompletionService.PassPercent;

        bool alreadyCompleted = await _missionService
            .IsCompletedAsync(userId, planetId);

        int creditsBefore = progress.Credits;
        int xpBefore = progress.ExperiencePoints;

        // Rewards only on the first pass, so replays can't farm XP and Credits
        if (passed && !alreadyCompleted)
        {
            bool saved = await _completionService
                .CompletePassedMissionAsync(
                    userId, planetId, dto.Answers);

            if (!saved)
            {
                _logger.LogError(
                    "Mission {PlanetId} rewards for {UserId} could not be saved",
                    planetId,
                    userId);

                return StatusCode(500, new
                {
                    message = "Mission results could not be saved."
                });
            }
        }

        // Every graded attempt goes into the mission history, passed or not.
        // A failed save is logged by the service and doesn't stop the player from seeing the result.
        await _completionService.SaveAttemptAsync(
            userId,
            planetId,
            correctCount,
            questions.Count,
            passed);

        // The answers are graded, so the same set can't be submitted again
        _questionStore.Remove(userId, planetId);

        var updated = await _progressService
            .GetOrCreateProgressAsync(userId);

        _logger.LogInformation(
            "Mission {PlanetId} by {UserId}: {Correct}/{Total}, passed {Passed}, replay {Replay}",
            planetId,
            userId,
            correctCount,
            questions.Count,
            passed,
            alreadyCompleted);

        return Ok(new
        {
            passed,
            correctCount,
            totalQuestions = questions.Count,
            // true when this was a replay of a mission passed before
            alreadyCompleted,
            // the real rewards (0 on a failed mission or a replay)
            awardedCredits = updated.Credits - creditsBefore,
            awardedXp = updated.ExperiencePoints - xpBefore,
            credits = updated.Credits,
            experiencePoints = updated.ExperiencePoints,
            rank = updated.Rank
        });
    }
}
