
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Models;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/progress")]
public class ProgressController : ControllerBase
{
    private readonly PlayerProgressService _progressService;
    private readonly GalacticSlicerDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RankService _rankService;
    private readonly PlanetProgressService _planetProgressService;
    private readonly ChallengeCompletionService _completionService;

    public ProgressController(
        PlayerProgressService progressService,
        GalacticSlicerDbContext context,
        UserManager<IdentityUser> userManager,
        RankService rankService,
        PlanetProgressService planetProgressService,
        ChallengeCompletionService completionService)
    {
        _progressService = progressService;
        _context = context;
        _userManager = userManager;
        _rankService = rankService;
        _planetProgressService = planetProgressService;
        _completionService = completionService;
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

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Unauthorized();
        }

        var progress = await _progressService
            .GetOrCreateProgressAsync(userId);

        var xpRange = _rankService
            .GetXpRange(progress.ExperiencePoints);

        var challengesCompleted = await _context.CompletedChallenges
            .CountAsync(cc => cc.UserId == userId);

        var challengesTotal = await _context.Challenges
            .CountAsync(c => c.SourceId != null);

        var planets = await _context.Planets
            .AsNoTracking()
            .OrderBy(p => p.Order)
            .ToListAsync();

        var completedPlanetIds = await _completionService
            .GetCompletedPlanetIdsAsync(userId);

        var planetStatuses = _planetProgressService
            .GetPlanetStatuses(
                planets,
                progress,
                completedPlanetIds);

        CharacterDto? activeCharacter = null;

        if (progress.ActiveCharacterId.HasValue)
        {
            var character = await _context.Characters
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.CharacterId == progress.ActiveCharacterId.Value);

            if (character != null)
            {
                activeCharacter = new CharacterDto
                {
                    CharacterId = character.CharacterId,
                    Name = character.Name,
                    Description = character.Description,
                    Price = character.Price,
                    ImageUrl = character.ImageUrl
                };
            }
        }

        return Ok(new PlayerProgressDto
        {
            Username = user.UserName ?? user.Email ?? string.Empty,
            Credits = progress.Credits,
            ExperiencePoints = progress.ExperiencePoints,
            Rank = _rankService.GetRank(progress.ExperiencePoints),
            XpForCurrentRank = xpRange.Current,
            XpForNextRank = xpRange.Next,
            ChallengesCompleted = challengesCompleted,
            ChallengesTotal = challengesTotal,
            ActiveCharacterId = progress.ActiveCharacterId,
            ActiveCharacter = activeCharacter,
            Planets = planetStatuses
        });
    }
}
