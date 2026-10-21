using GalacticSlicer.DAL;
using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GalacticSlicer.Controllers;

[Authorize] // the dashboard is personal, so only for logged-in players
public class ProgressController : Controller
{
    private readonly IChallengeRepository _challengeRepository;
    private readonly IPlanetRepository _planetRepository;
    private readonly ILogger<ProgressController> _logger;

    public ProgressController(IChallengeRepository challengeRepository, IPlanetRepository planetRepository, ILogger<ProgressController> logger)
    {
        _challengeRepository = challengeRepository;
        _planetRepository = planetRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var challenges = await _challengeRepository.GetAll();
        var planets = await _planetRepository.GetAll();
        if (challenges == null || planets == null)
        {
            _logger.LogError("[ProgressController] Could not load challenges or planets for the dashboard");
            return NotFound("Progress data not found");
        }

        // TODO Task 14: real unlocked planets from Jonas's unlocking service
        var planetsUnlocked = planets.Count(p => p.Order == 1);

        var model = new PlayerDashboardViewModel
        {
            Username = User.Identity?.Name ?? "Unknown slicer",
            Credits = 0,             // TODO Task 11/12: real Credits from the player
            ChallengesCompleted = 0, // TODO Task 12: real count from completed challenges
            ChallengesTotal = challenges.Count(),
            PlanetsUnlocked = planetsUnlocked,
            PlanetsTotal = planets.Count(),
            Rank = GetRank(planetsUnlocked),
        };

        return View(model);
    }

    private static string GetRank(int planetsUnlocked)
    {
        return planetsUnlocked switch
        {
            0 => "Initiate",
            1 or 2 => "Slicer",
            3 or 4 => "Rogue Slicer",
            5 => "Shadow Operative",
            _ => "Holonet Ghost",
        };
    }
}