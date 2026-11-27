
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

    public MissionsController(
        GalacticSlicerDbContext db,
        ChallengeCompletionService completionService,
        PlayerProgressService progressService,
        PlanetProgressService planetProgressService)
    {
        _db = db;
        _completionService = completionService;
        _progressService = progressService;
        _planetProgressService = planetProgressService;
    }

    private string? UserId =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    private static bool IsFinalExam(Planet planet) =>
        planet.Name.Equals(
            "Endor", StringComparison.OrdinalIgnoreCase)
        && planet.Subject == null;

    // GET /api/missions/1
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
            return StatusCode(403, "Planet is locked.");
        }

        var challenges = await GetMissionQuestions(planet);

        if (!HasRequiredQuestionCount(planet, challenges))
        {
            if (IsFinalExam(planet))
            {
                return Conflict(new
                {
                    message =
                        "The final mission requires three official " +
                        "questions from each of the five subjects."
                });
            }

            return NotFound(new
            {
                message = "No mission questions are configured."
            });
        }

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

    // Prevent replaying completed missions.
    var alreadyCompleted = await _db.CompletedMissions
        .AnyAsync(m =>
            m.UserId == userId &&
            m.PlanetId == planetId);

    if (alreadyCompleted)
    {
        return Conflict(new
        {
            message = "Mission already completed."
        });
    }

    var questions = await GetMissionQuestions(planet);

    if (!HasRequiredQuestionCount(planet, questions))
    {
        if (IsFinalExam(planet))
        {
            return Conflict(new
            {
                message =
                    "The final mission requires three official " +
                    "questions from each of the five subjects."
            });
        }

        return NotFound(new
        {
            message = "No mission questions are configured."
        });
    }

    if (dto?.Answers == null || dto.Answers.Count == 0)
    {
        return BadRequest(new
        {
            message = "Mission answers are required."
        });
    }

    var expectedIds = questions
        .Select(c => c.ChallengeId)
        .ToHashSet();

    var submittedIds = dto.Answers.Keys.ToHashSet();

    // All mission questions must be answered.
    if (!expectedIds.SetEquals(submittedIds))
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

    var success = await _completionService
        .CompletePassedMissionAsync(
            userId, planetId, dto.Answers);

    if (!success)
    {
        return Ok(new
        {
            passed = false,
            message = "Mission not passed or could not be saved."
        });
    }

    var updated = await _progressService
        .GetOrCreateProgressAsync(userId);

    return Ok(new
    {
        passed = true,
        credits = updated.Credits,
        experiencePoints = updated.ExperiencePoints,
        rank = updated.Rank
    });
}


    private async Task<List<Challenge>> GetMissionQuestions(
        Planet planet)
    {
        if (IsFinalExam(planet))
        {
            return await GetFinalMissionQuestions(planet);
        }

        return await _db.Challenges
            .AsNoTracking()
            .Where(c =>
                c.PlanetId == planet.PlanetId &&
                c.Subject == planet.Subject &&
                c.SourceId != null &&
                c.CreatedByUserId == null)
            .OrderBy(c => c.ChallengeId)
            .Take(ChallengeCompletionService.QuestionsPerMission)
            .ToListAsync();
    }

    private async Task<List<Challenge>> GetFinalMissionQuestions(
        Planet endor)
    {
        var subjectPlanets = await _db.Planets
            .AsNoTracking()
            .Where(p =>
                p.Order < endor.Order &&
                p.Subject != null)
            .OrderBy(p => p.Order)
            .Take(ChallengeCompletionService.FinalMissionSubjectCount)
            .Select(p => new
            {
                p.PlanetId,
                Subject = p.Subject!
            })
            .ToListAsync();

        if (subjectPlanets.Count !=
                ChallengeCompletionService.FinalMissionSubjectCount ||
            subjectPlanets
                .Select(p => p.Subject)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() !=
                ChallengeCompletionService.FinalMissionSubjectCount)
        {
            return new List<Challenge>();
        }

        var planetIds = subjectPlanets
            .Select(p => p.PlanetId)
            .ToList();

        var pool = await _db.Challenges
            .AsNoTracking()
            .Where(c =>
                c.PlanetId.HasValue &&
                planetIds.Contains(c.PlanetId.Value) &&
                c.SourceId != null &&
                c.CreatedByUserId == null)
            .OrderBy(c => c.ChallengeId)
            .ToListAsync();

        var subjectQuestions = subjectPlanets
            .Select(planet => pool
                .Where(c => string.Equals(
                    c.Subject,
                    planet.Subject,
                    StringComparison.OrdinalIgnoreCase))
                .Take(ChallengeCompletionService.QuestionsPerFinalSubject)
                .ToList())
            .ToList();

        if (subjectQuestions.Any(questions =>
            questions.Count !=
                ChallengeCompletionService.QuestionsPerFinalSubject))
        {
            return new List<Challenge>();
        }

        return Enumerable
            .Range(
                0,
                ChallengeCompletionService.QuestionsPerFinalSubject)
            .SelectMany(index => subjectQuestions
                .Select(questions => questions[index]))
            .ToList();
    }

    private static bool HasRequiredQuestionCount(
        Planet planet,
        IReadOnlyCollection<Challenge> questions)
    {
        return IsFinalExam(planet)
            ? questions.Count ==
              ChallengeCompletionService.QuestionsPerFinalMission
            : questions.Count > 0;
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
