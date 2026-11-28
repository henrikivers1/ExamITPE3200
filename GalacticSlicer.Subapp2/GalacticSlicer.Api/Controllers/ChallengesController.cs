
using System.IdentityModel.Tokens.Jwt;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/challenges")]
public class ChallengesController : ControllerBase
{
    private readonly IChallengeService _service;
    private readonly ILogger<ChallengesController> _logger;

    public ChallengesController(
        IChallengeService service,
        ILogger<ChallengesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private string? UserId =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    // GET /api/challenges
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (UserId == null) return Unauthorized();

        return Ok(await _service.GetAllAsync(UserId));
    }

    // GET /api/challenges/search?searchString=TLS&subject=Network
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchString,
        [FromQuery] string? subject)
    {
        if (UserId == null) return Unauthorized();

        var results = await _service.SearchAsync(
            searchString, subject, UserId);

        return Ok(results);
    }

    // GET /api/challenges/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (UserId == null) return Unauthorized();

        var challenge = await _service.GetByIdAsync(id, UserId);

        return challenge == null ? NotFound() : Ok(challenge);
    }

    // POST /api/challenges
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ChallengeUpsertDto dto)
    {
        if (UserId == null) return Unauthorized();

        try
        {
            var challenge = await _service.CreateAsync(dto, UserId);

            _logger.LogInformation(
                "Challenge {ChallengeId} created by {UserId}",
                challenge.ChallengeId,
                UserId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = challenge.ChallengeId },
                challenge);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(
                "Challenge rejected ({Error}): {Message}",
                "ValidationException",
                ex.Message);

            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT /api/challenges/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] ChallengeUpsertDto dto)
    {
        if (UserId == null) return Unauthorized();

        try
        {
            var updated = await _service.UpdateAsync(
                id, dto, UserId);

            _logger.LogInformation(
                "Update of challenge {ChallengeId} by {UserId}: {Updated}",
                id,
                UserId,
                updated);

            return updated ? NoContent() : NotFound();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(
                "Challenge rejected ({Error}): {Message}",
                "ValidationException",
                ex.Message);

            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE /api/challenges/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (UserId == null) return Unauthorized();

        var deleted = await _service.DeleteAsync(id, UserId);

        _logger.LogInformation(
            "Delete of challenge {ChallengeId} by {UserId}: {Deleted}",
            id,
            UserId,
            deleted);

        return deleted ? NoContent() : NotFound();
    }
}
