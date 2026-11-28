using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/progress")]
public class ProgressController : ControllerBase
{
    private readonly ProgressDashboardService _dashboardService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ProgressController> _logger;

    public ProgressController(
        ProgressDashboardService dashboardService,
        UserManager<IdentityUser> userManager,
        ILogger<ProgressController> logger)
    {
        _dashboardService = dashboardService;
        _userManager = userManager;
        _logger = logger;
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
            // a valid token for a user that no longer exists
            _logger.LogWarning(
                "Progress requested for unknown user {UserId}",
                userId);

            return Unauthorized();
        }

        var dashboard = await _dashboardService.GetDashboardAsync(
            userId,
            user.UserName ?? user.Email ?? string.Empty);

        return Ok(dashboard);
    }
}
