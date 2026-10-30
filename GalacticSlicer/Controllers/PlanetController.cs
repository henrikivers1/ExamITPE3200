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
    private readonly QuestionService _questionService;

    public PlanetController(IPlanetRepository planetRepository, ILogger<PlanetController>
    logger, ChallengeCompletionService completionService, UserManager<IdentityUser>
    userManager, PlanetProgressService planetProgressService, PlayerProgressService progressService,
    QuestionService questionService)
    {
        _questionService = questionService;
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

            return NotFound();
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

        // Number of questions.json questions per subject, for the "X challenges" label
        var subjects = await _questionService.GetSubjectsAsync();
        var totalQuestions = subjects.Sum(s => s.Count);

        // Convert database planets into ViewModels for the Galaxy page
        var viewModels = planets.Select(p => new PlanetViewModel
        {
            PlanetId = p.PlanetId,
            Name = p.Name,
            CssClass = p.CssClass,
            StepLabel = $"Chapter {p.Order}",

            // Questions the planet's mission can pick from (every subject for the mixed planet)
            ChallengeCount = p.Subject == null
                ? totalQuestions
                : subjects.FirstOrDefault(s => s.Subject == p.Subject).Count,

            // The first planet is always unlocked.
            // Other planets unlock when the player has enough XP.
            IsUnlocked = p.Order == 1 ||
                (progress != null &&
                 _planetProgressService.IsPlanetUnlocked(p, progress))

        }).ToList();

        // Send the planet data and unlock status to the Razor View
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

            return NotFound();
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

        // 10 random questions from the planet's subject (questions.json only, never user-made ones)
        var questions = QuestionService.PickRandom(
            await _questionService.GetPlanetPoolAsync(planet));

        // Remember which questions were handed out, so SubmitMission only accepts answers to these
        TempData[MissionKey(planet.PlanetId)] =
            string.Join(",", questions.Select(q => q.ChallengeId));

        var model = new MissionViewModel
        {
            Planet = planet,
            PassPercent = 80,

            // Total Credits available for completing the mission
            TotalCreditReward = questions.Sum(c => c.CreditReward),

            // Options are shuffled so the correct answer isn't always in the same place
            Questions = questions
                 .Select(c => new MissionQuestionViewModel
                 {
                     ChallengeId = c.ChallengeId,
                     Question = c.Question,
                     Options = QuestionService.ShuffledOptions(c)
                 })
                 .ToList()
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
        {
            _logger.LogWarning("[PlanetController] Invalid mission submission");
            return BadRequest("Invalid submission.");
        }

        var planet = await _planetRepository
            .GetPlanetById(submission.PlanetId);

        if (planet == null)
        {
            _logger.LogWarning("[PlanetController] Mission submitted for unknown Planet {PlanetId}", submission.PlanetId);
            return NotFound("Planet not found.");
        }

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

        // The questions Mission() handed out for this planet (stored in TempData, removed once read)
        if (TempData[MissionKey(planet.PlanetId)] is not string servedList)
            return BadRequest("This mission has expired. Reload the page to get new questions.");

        var servedIds = servedList.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        // Load them from the planet's question pool, in the order they were asked
        var pool = await _questionService.GetPlanetPoolAsync(planet);
        var challenges = servedIds
            .Select(id => pool.FirstOrDefault(c => c.ChallengeId == id))
            .OfType<Challenge>()
            .ToList();

        // A mission needs actual questions in the database
        if (challenges.Count == 0)
        {
            _logger.LogWarning("[PlanetController] Mission {PlanetId} has no challenges", planet.PlanetId);
            return BadRequest("This mission has no challenges.");
        }

        var answers = submission.Answers;

        if (answers == null || answers.Count != challenges.Count)
        {
            _logger.LogWarning("[PlanetController] Wrong number of answers for Planet {PlanetId}", planet.PlanetId);
            return BadRequest("All questions must be answered.");
        }

        // Reject duplicate IDs or questions that weren't handed out
        var expectedIds = challenges.Select(c => c.ChallengeId).ToHashSet();
        var submittedIds = answers.Select(a => a.ChallengeId).ToList();

        if (submittedIds.Count != submittedIds.Distinct().Count() ||
            submittedIds.Any(id => !expectedIds.Contains(id)))
        {
            _logger.LogWarning("[PlanetController] Invalid challenge ids in submission for Planet {PlanetId}", planet.PlanetId);
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

        _logger.LogInformation(
            "[PlanetController] User {UserId} submitted Planet {PlanetId}: {Correct}/{Total}, passed {Passed}",
            userId, planet.PlanetId, correctCount, challenges.Count, passed);

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

        // Review for the result screen: the correct answer and explanation for every question
        var review = challenges.Select(challenge =>
        {
            var selected = answers.First(a => a.ChallengeId == challenge.ChallengeId).SelectedAnswer;

            return new
            {
                question = challenge.Question,
                selectedAnswer = selected,
                correctAnswer = challenge.CorrectAnswer,
                isCorrect = string.Equals(selected, challenge.CorrectAnswer,
                    StringComparison.OrdinalIgnoreCase),
                explanation = challenge.Explanation
            };
        });

        return Json(new
        {
            passed,
            correctCount,
            totalQuestions = challenges.Count,
            percent = Math.Round(percent, 1),
            review
        });
    }

    // TempData key for the questions handed out for one planet's mission
    private static string MissionKey(int planetId) => $"MissionQuestions{planetId}";

}