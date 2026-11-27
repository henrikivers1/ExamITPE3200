using GalacticSlicer.DAL;
using GalacticSlicer.Models;
using GalacticSlicer.Services;
using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GalacticSlicer.Controllers;

public class ChallengeController : Controller
{
    private readonly IChallengeRepository _challengeRepository;
    private readonly ILogger<ChallengeController> _logger;
    private readonly IQuestionService _questionService;
    private readonly UserManager<IdentityUser> _userManager;

    public ChallengeController(IChallengeRepository challengeRepository, ILogger<ChallengeController> logger, IQuestionService questionService, UserManager<IdentityUser> userManager)
    {
        _challengeRepository = challengeRepository;
        _logger = logger;
        _questionService = questionService;
        _userManager = userManager;
    }

    // True if the logged-in user made this challenge. Questions from questions.json have no
    // owner, so nobody can open, edit or delete them from the Challenges page.
    private bool IsOwnChallenge(Challenge challenge)
    {
        var userId = _userManager.GetUserId(User);
        return userId != null && challenge.SourceId == null && challenge.CreatedByUserId == userId;
    }

    // Fills the subject dropdown used by the Create and Edit forms (the subjects from questions.json)
    private async Task LoadSubjectDropdown(string? selectedSubject = null)
    {
        var subjects = (await _questionService.GetSubjectsAsync()).Select(s => s.Subject);
        ViewBag.Subjects = new SelectList(subjects, selectedSubject);
    }

    // Domain rules the form's DataAnnotations can't check: the subject must be one of the real ones,
    // and the four options must differ (the correct answer is compared by text, so two equal options
    // would make the question unanswerable)
    private async Task ValidateAnswerOptions(ChallengeFormViewModel model)
    {
        var subjects = await _questionService.GetSubjectsAsync();
        if (!string.IsNullOrEmpty(model.Subject) && !subjects.Any(s => s.Subject == model.Subject))
        {
            ModelState.AddModelError(nameof(model.Subject), "Please choose a subject.");
        }

        var options = new[] { model.OptionA, model.OptionB, model.OptionC, model.OptionD };
        if (options.Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Length)
        {
            ModelState.AddModelError(string.Empty, "The four options must all be different.");
        }
    }

    // The form picks the correct answer as a letter (A-D); the database stores the option's text
    private static string CorrectAnswerText(ChallengeFormViewModel model) => model.CorrectOption switch
    {
        "A" => model.OptionA,
        "B" => model.OptionB,
        "C" => model.OptionC,
        "D" => model.OptionD,
        _ => string.Empty
    };

    // A saved challenge as shown on the list, Details and Delete pages
    private static ChallengeViewModel ToViewModel(Challenge challenge) => new ChallengeViewModel
    {
        ChallengeId = challenge.ChallengeId,
        Title = challenge.Title,
        Subject = challenge.Subject,
        Question = challenge.Question,
        OptionA = challenge.OptionA,
        OptionB = challenge.OptionB,
        OptionC = challenge.OptionC,
        OptionD = challenge.OptionD,
        CorrectAnswer = challenge.CorrectAnswer
    };

    // A saved challenge in the Edit form, with the correct answer turned back into its letter
    private static ChallengeFormViewModel ToFormViewModel(Challenge challenge) => new ChallengeFormViewModel
    {
        ChallengeId = challenge.ChallengeId,
        Subject = challenge.Subject,
        Question = challenge.Question,
        OptionA = challenge.OptionA,
        OptionB = challenge.OptionB,
        OptionC = challenge.OptionC,
        OptionD = challenge.OptionD,
        CorrectOption = challenge.CorrectAnswer == challenge.OptionA ? "A"
            : challenge.CorrectAnswer == challenge.OptionB ? "B"
            : challenge.CorrectAnswer == challenge.OptionC ? "C"
            : challenge.CorrectAnswer == challenge.OptionD ? "D"
            : null
    };

    // GET: /Challenge/Topics - one card per subject in questions.json, in planet order.
    // The question count includes the logged-in player's own questions in that subject.
    public async Task<IActionResult> Topics()
    {
        var subjects = await _questionService.GetSubjectsAsync(_userManager.GetUserId(User));

        var topics = subjects.Select(s => new ChallengeTopicViewModel
        {
            Name = s.Subject,
            IconKey = s.Subject switch
            {
                "Linux Command Line" => "linux",
                "Network Security" => "network",
                "Software Security" => "web",
                "Cryptography Basics" => "crypto",
                _ => "access"
            },
            // Difficulty follows the planet the subject belongs to (chapter 1-2, 3-4, 5+)
            Difficulty = s.Order switch
            {
                <= 2 => "Beginner",
                <= 4 => "Intermediate",
                _ => "Advanced"
            },
            ChallengeCount = s.Count
        }).ToList();

        return View(topics);
    }

    // GET: /Challenge/Practice?subject=Network%20Security - 10 random questions from one subject.
    // Practice gives no rewards, so the correct answers are sent to the page for instant feedback.
    public async Task<IActionResult> Practice(string subject)
    {
        var userId = _userManager.GetUserId(User);
        var pool = string.IsNullOrWhiteSpace(subject)
            ? new List<Challenge>()
            : await _questionService.GetSubjectPoolAsync(subject, userId);

        if (pool.Count == 0)
        {
            return RedirectToAction(nameof(Topics));
        }

        var model = new PracticeViewModel
        {
            Subject = subject,
            // 10 random questions; the player's own questions are picked like any other
            Questions = QuestionService.PickRandom(pool).Select(c =>
            {
                var options = QuestionService.ShuffledOptions(c);
                return new PracticeQuestionViewModel
                {
                    Question = c.Question,
                    Options = options,
                    CorrectIndex = options.IndexOf(c.CorrectAnswer),
                    Explanation = c.Explanation
                };
            }).ToList()
        };

        return View(model);
    }

    // GET: /Challenge - the logged-in user's own challenges, with search
    public async Task<IActionResult> Index(string? searchString, string? subject)
    {
        // Send the current filter values back so the form keeps them after submitting
        ViewData["SearchString"] = searchString;
        ViewData["Subject"] = subject;
        await LoadSubjectDropdown(subject);

        // Visitors have no challenges of their own
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return View(new List<ChallengeViewModel>());
        }

        // Only the logged-in user's own challenges (questions.json is never listed)
        var challenges = await _challengeRepository.Search(searchString, subject, userId);
        if (challenges == null)
        {
            _logger.LogError("[ChallengeController] Challenge list not found while executing Search()");
            return NotFound();
        }

        return View(challenges.Select(ToViewModel).ToList());
    }

    // GET: /Challenge/Details/5 - only the user who made the challenge
    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var challenge = await _challengeRepository.GetChallengeById(id);
        if (challenge == null || !IsOwnChallenge(challenge))
        {
            _logger.LogError("[ChallengeController] Challenge not found for ChallengeId {ChallengeId:0000}", id);
            return NotFound();
        }
        return View(ToViewModel(challenge));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Create()
    {
        await LoadSubjectDropdown();
        return View(new ChallengeFormViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChallengeFormViewModel model)
    {
        await ValidateAnswerOptions(model);

        if (ModelState.IsValid)
        {
            // Owner, title and correct answer are set on the server. Source, planet and reward stay empty,
            // so a user can't post their way into the planet missions or another user's list.
            var challenge = new Challenge
            {
                CreatedByUserId = _userManager.GetUserId(User),
                Title = QuestionService.MakeTitle(model.Question),
                Question = model.Question,
                Subject = model.Subject,
                OptionA = model.OptionA,
                OptionB = model.OptionB,
                OptionC = model.OptionC,
                OptionD = model.OptionD,
                CorrectAnswer = CorrectAnswerText(model)
            };

            try
            {
                await _challengeRepository.Create(challenge);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                // The repository already logged the full exception
                ModelState.AddModelError(string.Empty, "Could not save the challenge. Please try again.");
            }
        }
        _logger.LogWarning("[ChallengeController] Challenge creation failed for {Question}", model.Question);

        // The form is shown again, so the dropdown needs its subjects again
        await LoadSubjectDropdown(model.Subject);
        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var challenge = await _challengeRepository.GetChallengeById(id);
        if (challenge == null || !IsOwnChallenge(challenge))
        {
            _logger.LogError("[ChallengeController] Challenge not found when editing ChallengeId {ChallengeId:0000}", id);
            return NotFound();
        }

        await LoadSubjectDropdown(challenge.Subject);
        return View(ToFormViewModel(challenge));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ChallengeFormViewModel model)
    {
        if (id != model.ChallengeId)
        {
            _logger.LogWarning("[ChallengeController] Edit id mismatch: route {RouteId}, form {FormId}", id, model.ChallengeId);
            return BadRequest();
        }

        // Load the saved challenge and check the owner there, not in the posted form
        var existing = await _challengeRepository.GetChallengeById(id);
        if (existing == null || !IsOwnChallenge(existing))
        {
            return NotFound();
        }

        await ValidateAnswerOptions(model);

        if (ModelState.IsValid)
        {
            try
            {
                // Copy only the fields the form edits, so owner, source, planet and reward can't be changed
                existing.Title = QuestionService.MakeTitle(model.Question);
                existing.Question = model.Question;
                existing.CorrectAnswer = CorrectAnswerText(model);
                existing.Subject = model.Subject;
                existing.OptionA = model.OptionA;
                existing.OptionB = model.OptionB;
                existing.OptionC = model.OptionC;
                existing.OptionD = model.OptionD;

                await _challengeRepository.Update(existing);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                // E.g. the challenge was deleted by someone else while this form was open
                ModelState.AddModelError(string.Empty, "Could not update the challenge. Please try again.");
            }
        }
        _logger.LogWarning("[ChallengeController] Challenge update failed for ChallengeId {ChallengeId:0000}", model.ChallengeId);

        await LoadSubjectDropdown(model.Subject);
        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var challenge = await _challengeRepository.GetChallengeById(id);
        if (challenge == null || !IsOwnChallenge(challenge))
        {
            _logger.LogError("[ChallengeController] Challenge not found when deleting ChallengeId {ChallengeId:0000}", id);
            return NotFound();
        }
        return View(ToViewModel(challenge));
    }

    [HttpPost, ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Only the owner can delete, and never a questions.json question
        var challenge = await _challengeRepository.GetChallengeById(id);
        if (challenge == null || !IsOwnChallenge(challenge))
        {
            return NotFound();
        }

        try
        {
            bool deleted = await _challengeRepository.Delete(id);
            if (!deleted)
            {
                _logger.LogWarning("[ChallengeController] Challenge {ChallengeId:0000} not found for deletion", id);
                return NotFound();
            }
        }
        catch (Exception)
        {
            // The repository already logged the full exception; show the confirm page again with a message
            _logger.LogWarning("[ChallengeController] Challenge deletion failed for ChallengeId {ChallengeId:0000}", id);
            TempData["ErrorMessage"] = "Could not delete the challenge. Please try again.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }
}