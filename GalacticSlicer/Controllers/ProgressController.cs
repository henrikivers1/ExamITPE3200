
using GalacticSlicer.DAL;
using GalacticSlicer.Models;
using GalacticSlicer.Services;
using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Controllers;

[Authorize]
public class ProgressController : Controller
{
    private readonly IChallengeRepository _challengeRepository;
    private readonly IPlanetRepository _planetRepository;
    private readonly ILogger<ProgressController> _logger;
    private readonly PlayerProgressService _progressService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly GalacticSlicerDbContext _context;

    public ProgressController(
        IChallengeRepository challengeRepository,
        IPlanetRepository planetRepository,
        ILogger<ProgressController> logger,
        PlayerProgressService progressService,
        UserManager<IdentityUser> userManager,
        GalacticSlicerDbContext context)
    {
        _challengeRepository = challengeRepository;
        _planetRepository = planetRepository;
        _logger = logger;
        _progressService = progressService;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
        {
            return Challenge();
        }

        // Get or create progress for the logged-in player
        var progress = await _progressService
            .GetOrCreateProgressAsync(userId);

        // Get challenges and planets using existing repositories
        var challenges = await _challengeRepository.GetAll();
        var planets = await _planetRepository.GetAll();

        if (challenges == null || planets == null)
        {
            _logger.LogError(
                "[ProgressController] Could not load dashboard data");

            return NotFound("Progress data not found");
        }

        // Count the logged-in player's completed challenges
        var completedCount = await _context.CompletedChallenges
            .CountAsync(c => c.UserId == userId);

        // Temporary unlocking rule until Task 6
        var planetsUnlocked = planets.Count(p => p.Order == 1);

        var model = new PlayerDashboardViewModel
        {
            Username = User.Identity?.Name ?? "Unknown slicer",
            Credits = progress.Credits,
            ChallengesCompleted = completedCount,
            ChallengesTotal = challenges.Count(),
            PlanetsUnlocked = planetsUnlocked,
            PlanetsTotal = planets.Count(),
            Rank = progress.Rank,
            ExperiencePoints = progress.ExperiencePoints,
            XpForCurrentRank = progress.ExperiencePoints switch
            {
                >= 12000 => 12000,
                >= 8000 => 8000,
                >= 5000 => 5000,
                >= 3000 => 3000,
                >= 1500 => 1500,
                >= 500 => 500,
                _ => 0
            },
            XpForNextRank = progress.ExperiencePoints switch
            {
                >= 12000 => 12000,
                >= 8000 => 12000,
                >= 5000 => 8000,
                >= 3000 => 5000,
                >= 1500 => 3000,
                >= 500 => 1500,
                _ => 500
            },
        };

        return View(model);
    }
}
