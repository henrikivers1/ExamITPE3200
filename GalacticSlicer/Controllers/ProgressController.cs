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
    private readonly PlanetProgressService _planetProgressService;
    private readonly ChallengeCompletionService _completionService;
    private readonly RankService _rankService;

    public ProgressController(
        IChallengeRepository challengeRepository,
        IPlanetRepository planetRepository,
        ILogger<ProgressController> logger,
        PlayerProgressService progressService,
        UserManager<IdentityUser> userManager,
        GalacticSlicerDbContext context,
        PlanetProgressService planetProgressService,
        ChallengeCompletionService completionService,
        RankService rankService)
    {
        _challengeRepository = challengeRepository;
        _planetRepository = planetRepository;
        _logger = logger;
        _progressService = progressService;
        _userManager = userManager;
        _context = context;
        _planetProgressService = planetProgressService;
        _completionService = completionService;
        _rankService = rankService;
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

            return NotFound();
        }

        // Count the logged-in player's completed challenges
        var completedCount = await _context.CompletedChallenges
            .CountAsync(c => c.UserId == userId);

        // Locked / unlocked / completed status for every planet (same rules as the Galaxy page)
        var completedPlanetIds = await _completionService.GetCompletedPlanetIdsAsync(userId);
        var statuses = _planetProgressService.GetPlanetStatuses(planets, progress, completedPlanetIds);
        var nextLocked = statuses.FirstOrDefault(p => !p.IsUnlocked);

        // The equipped character, for the badge next to the username
        var activeCharacter = progress.ActiveCharacterId == null
            ? null
            : await _context.Characters.FindAsync(progress.ActiveCharacterId);

        var xpRange = _rankService.GetXpRange(progress.ExperiencePoints);

        var model = new PlayerDashboardViewModel
        {
            Username = User.Identity?.Name ?? "Unknown slicer",
            ActiveCharacter = activeCharacter,
            Credits = progress.Credits,
            ChallengesCompleted = completedCount,
            // Only questions.json questions can be completed in missions, so only those count
            // (user-made challenges don't change anyone's total)
            ChallengesTotal = challenges.Count(c => c.SourceId != null),
            PlanetsUnlocked = statuses.Count(p => p.IsUnlocked),
            PlanetsTotal = statuses.Count,
            Rank = progress.Rank,
            ExperiencePoints = progress.ExperiencePoints,
            XpForCurrentRank = xpRange.Current,
            XpForNextRank = xpRange.Next,
            Planets = statuses,
            NextPlanet = statuses.FirstOrDefault(p => p.IsUnlocked && !p.IsCompleted),
            LastUnlockedPlanet = statuses.LastOrDefault(p => p.IsUnlocked),
            NextLockedPlanet = nextLocked,
            NextLockedPlanetXp = nextLocked == null ? 0 : _planetProgressService.GetRequiredXp(
                planets.First(p => p.PlanetId == nextLocked.PlanetId).Order)
        };

        return View(model);
    }
}
