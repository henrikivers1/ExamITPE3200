
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/progress")]
public class ProgressController : ControllerBase
{
    private readonly PlayerProgressService _progressService;

    public ProgressController(
        PlayerProgressService progressService)
    {
        _progressService = progressService;
    }

    // GET /api/progress/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProgress()
    {
        var userId = User.FindFirstValue(
            JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var progress = await _progressService
            .GetOrCreateProgressAsync(userId);

        return Ok(new PlayerProgressDto
        {
            Credits = progress.Credits,
            ExperiencePoints = progress.ExperiencePoints,
            Rank = progress.Rank,
            ActiveCharacterId = progress.ActiveCharacterId
        });
    }
}
