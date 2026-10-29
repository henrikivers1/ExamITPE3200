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

    public PlanetController(IPlanetRepository planetRepository, ILogger<PlanetController> logger, ChallengeCompletionService completionService, UserManager<IdentityUser> userManager)
    {
        _planetRepository = planetRepository;
        _logger = logger;
        _completionService = completionService;
        _userManager = userManager;

    }

    // GET: /Planet - the Galaxy page
    public async Task<IActionResult> Index()
    {
        var planets = await _planetRepository.GetAll();
        if (planets == null)
        {
            _logger.LogError("[PlanetController] Planet list not found while executing GetAll()");
            return NotFound("Planet list not found");
        }

        // Turn database entities into exactly what the Galaxy page needs
        var viewModels = planets.Select(p => new PlanetViewModel
        {
            PlanetId = p.PlanetId,
            Name = p.Name,
            CssClass = p.CssClass,
            StepLabel = $"Chapter {p.Order}",
            ChallengeCount = p.Challenges?.Count ?? 0,       // TODO 10b: real count once challenges belong to planets
            IsUnlocked = p.Order == 1 // TODO Task 14: real unlocking from Jonas's service
        }).ToList();

        return View(viewModels);
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

        // Temporary unlocking rule
        if (planet.Order != 1)
        {
            return RedirectToAction(nameof(Index));
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

        // Temporary unlocking rule
        if (planet.Order != 1)
            return Forbid();

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

        if (passed)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Unauthorized();

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