
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
        if (UserId == null) return Unauthorized();

        var planet = await _db.Planets.FindAsync(planetId);
        if (planet == null) return NotFound();

        // Endor needs a separate question-selection flow.
        if (IsFinalExam(planet))
            return Conflict("Final exam is not configured yet.");

        var progress = await _progressService
            .GetOrCreateProgressAsync(UserId);

        if (!_planetProgressService.IsPlanetUnlocked(
            planet, progress))
        {
            return StatusCode(403, "Planet is locked.");
        }

        var challenges = await GetMissionQuestions(planet);

        if (challenges.Count == 0)
            return NotFound("No mission questions configured.");

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
    if (UserId == null) return Unauthorized();

    var planet = await _db.Planets.FindAsync(planetId);
    if (planet == null) return NotFound();

    if (IsFinalExam(planet))
        return Conflict(new
        {
            message = "Final exam is not configured yet."
        });

    var progress = await _progressService
        .GetOrCreateProgressAsync(UserId);

    if (!_planetProgressService.IsPlanetUnlocked(
        planet, progress))
    {
        return StatusCode(403, new
        {
            message = "Planet is locked."
        });
    }

    // Prevent replaying completed missions.
    var alreadyCompleted = await _db.CompletedMissions
        .AnyAsync(m =>
            m.UserId == UserId &&
            m.PlanetId == planetId);

    if (alreadyCompleted)
    {
        return Conflict(new
        {
            message = "Mission already completed."
        });
    }

    var questions = await GetMissionQuestions(planet);

    if (questions.Count == 0)
        return NotFound(new
        {
            message = "No mission questions configured."
        });

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
            UserId, planetId, dto.Answers);

    if (!success)
    {
        return Ok(new
        {
            passed = false,
            message = "Mission not passed or could not be saved."
        });
    }

    var updated = await _progressService
        .GetOrCreateProgressAsync(UserId);

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
        return await _db.Challenges
            .AsNoTracking()
            .Where(c =>
                c.PlanetId == planet.PlanetId &&
                c.Subject == planet.Subject &&
                c.SourceId != null &&
                c.CreatedByUserId == null)
            .OrderBy(c => c.ChallengeId)
            .ToListAsync();
    }
}
