using GalacticSlicer.DAL;
using GalacticSlicer.Models;
using Microsoft.AspNetCore.Mvc;
using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using GalacticSlicer.Services;
using Microsoft.AspNetCore.Identity;

namespace GalacticSlicer.Controllers;

public class PlanetController : Controller
{
    private readonly IPlanetRepository _planetRepository;
    private readonly ILogger<PlanetController> _logger;
    private readonly ChallengeCompletionService _completionService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly PlanetProgressService _planetProgressService;
    private readonly PlayerProgressService _progressService;

    public PlanetController(IPlanetRepository planetRepository, ILogger<PlanetController>
    logger, ChallengeCompletionService completionService, UserManager<IdentityUser>
    userManager, PlanetProgressService planetProgressService, PlayerProgressService progressService)
    {
        _planetRepository = planetRepository;
        _logger = logger;
        _completionService = completionService;
        _userManager = userManager;
        _planetProgressService = planetProgressService;
        _progressService = progressService;

    }



    // GET: /Planet - Display the Galaxy map with unlocked and locked planets
    public async Task<IActionResult> Index()
    {
        // Get all planets from the database
        var planets = await _planetRepository.GetAll();

        // Handle errors if the planets could not be retrieved
        if (planets == null)
        {
            _logger.LogError(
                "[PlanetController] Planet list not found while executing GetAll()");

            return NotFound("Planet list not found");
        }

        // Get the currently logged-in user's ID
        var userId = _userManager.GetUserId(User);

        // Visitors who are not logged in do not have player progress
        PlayerProgress? progress = null;

        if (userId != null)
        {
            // Load the player's saved Credits, XP and rank from SQLite
            progress = await _progressService
                .GetOrCreateProgressAsync(userId);
        }

        // Convert database planets into ViewModels for the Galaxy page
        var viewModels = planets.Select(p => new PlanetViewModel
        {
            PlanetId = p.PlanetId,
            Name = p.Name,
            CssClass = p.CssClass,
            StepLabel = $"Chapter {p.Order}",

            // Count the challenges belonging to each planet
            ChallengeCount = p.Challenges?.Count ?? 0,

            // The first planet is always unlocked.
            // Other planets unlock when the player has enough XP.
            IsUnlocked = p.Order == 1 ||
                (progress != null &&
                 _planetProgressService.IsPlanetUnlocked(p, progress))

        }).ToList();

        // Send the planet data and unlock status to the Razor View
        return View(viewModels);

        // TODO 10b: real count once challenges belong to planets
        // TODO Task 14: real unlocking from Jonas's service
    }


    // GET: /Planet/Mission/1 - the mission (exam) for one planet
    public async Task<IActionResult> Mission(int id)
    {
        var planet = await _planetRepository.GetPlanetById(id);

        if (planet == null)
        {
            _logger.LogError(
                "[PlanetController] Planet not found for PlanetId {PlanetId}",
                id);

            return NotFound("Planet not found");
        }

        // The first planet is always available
        if (planet.Order != 1)
        {
            // Other planets require the player to be logged in
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            // Retrieve the player's saved XP and progression
            var progress = await _progressService
                .GetOrCreateProgressAsync(userId);

            // Prevent access if the player has not earned enough XP
            if (!_planetProgressService.IsPlanetUnlocked(planet, progress))
            {
                _logger.LogWarning(
                    "[PlanetController] User {UserId} attempted to access locked Planet {PlanetId}",
                    userId, id);

                return RedirectToAction(nameof(Index));
            }
        }

        var model = new MissionViewModel
        {
            Planet = planet,
            PassPercent = 80,

            // Total Credits available for completing the mission
            TotalCreditReward = planet.Challenges?
             .Sum(c => c.CreditReward) ?? 0,

            Questions = planet.Challenges?
                 .Select(c => new MissionQuestionViewModel
                 {
                     ChallengeId = c.ChallengeId,
                     Question = c.Question,
                     Options = new List<string>
                     {
                    c.OptionA,
                    c.OptionB,
                    c.OptionC,
                    c.OptionD
                     }
                 })
                 .ToList() ?? new List<MissionQuestionViewModel>()
        };

        return View(model);
    }


    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitMission(
        [FromBody] MissionSubmissionViewModel submission)
    {
        if (submission == null || !ModelState.IsValid)
            return BadRequest("Invalid submission.");

        var planet = await _planetRepository
            .GetPlanetById(submission.PlanetId);

        if (planet == null)
            return NotFound("Planet not found.");

        // Get the authenticated player's ID
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Unauthorized();

        // Retrieve the player's saved progression
        var progress = await _progressService
            .GetOrCreateProgressAsync(userId);

        // Prevent submissions to planets the player has not unlocked
        if (!_planetProgressService.IsPlanetUnlocked(planet, progress))
        {
            _logger.LogWarning(
                "[PlanetController] User {UserId} attempted to submit locked Planet {PlanetId}",
                userId, planet.PlanetId);

            return Forbid();
        }

        var challenges = planet.Challenges?.ToList();

        // A mission needs actual questions in the database
        if (challenges == null || challenges.Count == 0)
            return BadRequest("This mission has no challenges.");

        var answers = submission.Answers;

        if (answers == null || answers.Count != challenges.Count)
            return BadRequest("All questions must be answered.");

        // Reject duplicate IDs or questions from other planets
        var expectedIds = challenges.Select(c => c.ChallengeId).ToHashSet();
        var submittedIds = answers.Select(a => a.ChallengeId).ToList();

        if (submittedIds.Count != submittedIds.Distinct().Count() ||
            submittedIds.Any(id => !expectedIds.Contains(id)))
        {
            return BadRequest("Invalid challenge IDs.");
        }

        // Server-side answer validation
        int correctCount = challenges.Count(challenge =>
        {
            var answer = answers.First(a =>
                a.ChallengeId == challenge.ChallengeId);

            return string.Equals(
                answer.SelectedAnswer,
                challenge.CorrectAnswer,
                StringComparison.OrdinalIgnoreCase);
        });

        double percent = correctCount * 100.0 / challenges.Count;
        bool passed = percent >= 80;

        // Award Credits and XP only when the mission is passed
        if (passed)
        {
            foreach (var challenge in challenges)
            {
                await _completionService.CompleteChallengeAsync(
                    userId,
                    challenge.ChallengeId);
            }
        }

        return Json(new
        {
            passed,
            correctCount,
            totalQuestions = challenges.Count,
            percent = Math.Round(percent, 1)
        });
    }

}