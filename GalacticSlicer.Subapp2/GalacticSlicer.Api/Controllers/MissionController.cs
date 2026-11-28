using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Models;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/missions")]
public class MissionsController : ControllerBase
{
    private readonly GalacticSlicerDbContext _db;
    private readonly ChallengeCompletionService _completionService;
    private readonly PlayerProgressService _progressService;
    private readonly PlanetProgressService _planetProgressService;
    private readonly MissionQuestionStore _questionStore;

    public MissionsController(
        GalacticSlicerDbContext db,
        ChallengeCompletionService completionService,
        PlayerProgressService progressService,
        PlanetProgressService planetProgressService,
        MissionQuestionStore questionStore)
    {
        _db = db;
        _completionService = completionService;
        _progressService = progressService;
        _planetProgressService = planetProgressService;
        _questionStore = questionStore;
    }

    private string? UserId =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    private static bool IsFinalExam(Planet planet) =>
        planet.Name.Equals(
            "Endor", StringComparison.OrdinalIgnoreCase)
        && planet.Subject == null;

    // GET /api/missions/1
    // 10 random questions, sent without the correct answers.
    // Completed missions can be played again (without new rewards).
    [HttpGet("{planetId:int}")]
    public async Task<IActionResult> GetMission(int planetId)
    {
        var userId = UserId;

        if (userId == null) return Unauthorized();

        var planet = await _db.Planets.FindAsync(planetId);
        if (planet == null) return NotFound();

        var progress = await _progressService
            .GetOrCreateProgressAsync(userId);

        if (!await IsPlanetUnlockedAsync(
            planet,
            progress,
            userId))
        {
            return StatusCode(403, new
            {
                message = "Planet is locked."
            });
        }

        var challenges = await GetMissionQuestionsAsync(planet);

        if (challenges.Count == 0)
        {
            return NotFound(new
            {
                message = "No mission questions are configured."
            });
        }

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

        var planet = await _db.Planets.FindAsync(planetId);
        if (planet == null) return NotFound();

        var progress = await _progressService
            .GetOrCreateProgressAsync(userId);

        if (!await IsPlanetUnlockedAsync(
            planet,
            progress,
            userId))
        {
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
        var questions = await _db.Challenges
            .AsNoTracking()
            .Where(c => handedOut.Contains(c.ChallengeId))
            .ToListAsync();

        int correctCount = questions.Count(c => string.Equals(
            dto.Answers[c.ChallengeId],
            c.CorrectAnswer,
            StringComparison.OrdinalIgnoreCase));

        bool passed = questions.Count > 0 &&
            correctCount * 100.0 / questions.Count >=
                ChallengeCompletionService.PassPercent;

        bool alreadyCompleted = await _db.CompletedMissions
            .AnyAsync(m =>
                m.UserId == userId &&
                m.PlanetId == planetId);

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
                return StatusCode(500, new
                {
                    message = "Mission results could not be saved."
                });
            }
        }

        // The answers are graded, so the same set can't be submitted again
        _questionStore.Remove(userId, planetId);

        var updated = await _progressService
            .GetOrCreateProgressAsync(userId);

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

    // 10 random official questions, like QuestionService in Subapp 1:
    // the planet's own subject, or every subject before it for Endor.
    private async Task<List<Challenge>> GetMissionQuestionsAsync(
        Planet planet)
    {
        var query = _db.Challenges
            .AsNoTracking()
            .Where(c =>
                c.SourceId != null &&
                c.CreatedByUserId == null &&
                c.PlanetId.HasValue);

        if (IsFinalExam(planet))
        {
            var previousPlanetIds = await _db.Planets
                .Where(p =>
                    p.Order < planet.Order &&
                    p.Subject != null)
                .Select(p => p.PlanetId)
                .ToListAsync();

            query = query.Where(c =>
                previousPlanetIds.Contains(c.PlanetId!.Value));
        }
        else
        {
            query = query.Where(c =>
                c.PlanetId == planet.PlanetId &&
                c.Subject == planet.Subject);
        }

        var pool = await query.ToListAsync();

        return pool
            .OrderBy(_ => Random.Shared.Next())
            .Take(ChallengeCompletionService.QuestionsPerMission)
            .ToList();
    }

    private async Task<bool> IsPlanetUnlockedAsync(
        Planet planet,
        PlayerProgress progress,
        string userId)
    {
        var planets = await _db.Planets
            .AsNoTracking()
            .OrderBy(p => p.Order)
            .ToListAsync();

        var completedPlanetIds = await _completionService
            .GetCompletedPlanetIdsAsync(userId);

        return _planetProgressService.IsPlanetUnlocked(
            planet,
            progress,
            planets,
            completedPlanetIds);
    }
}
