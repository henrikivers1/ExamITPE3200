using GalacticSlicer.DAL;
using GalacticSlicer.Models;
using GalacticSlicer.Services;
using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GalacticSlicer.Controllers
{
    public class ChallengeController : Controller
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IPlanetRepository _planetRepository;
        private readonly ILogger<ChallengeController> _logger;
        private readonly QuestionService _questionService;
        private readonly UserManager<IdentityUser> _userManager;

        public ChallengeController(IChallengeRepository challengeRepository, IPlanetRepository planetRepository, ILogger<ChallengeController> logger, QuestionService questionService, UserManager<IdentityUser> userManager)
        {
            _challengeRepository = challengeRepository;
            _planetRepository = planetRepository;
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

        // Fills the planet dropdown used by the Create and Edit forms
        private async Task LoadPlanetDropdown(int? selectedPlanetId = null)
        {
            var planets = await _planetRepository.GetAll() ?? new List<Planet>();
            ViewBag.Planets = new SelectList(planets, "PlanetId", "Name", selectedPlanetId);
        }

        // Domain rule: the correct answer must be one of the four options, and the options must differ.
        // Missions compare the clicked option's text with CorrectAnswer, so anything else is unanswerable.
        private void ValidateAnswerOptions(Challenge challenge)
        {
            var options = new[] { challenge.OptionA, challenge.OptionB, challenge.OptionC, challenge.OptionD };

            if (!options.Contains(challenge.CorrectAnswer, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(challenge.CorrectAnswer),
                    "The correct answer must match one of the four options exactly.");
            }

            if (options.Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Length)
            {
                ModelState.AddModelError(string.Empty, "The four options must all be different.");
            }
        }

        // GET: /Challenge/Topics - one card per subject in questions.json, in planet order
        public async Task<IActionResult> Topics()
        {
            var subjects = await _questionService.GetSubjectsAsync();

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
            var pool = string.IsNullOrWhiteSpace(subject)
                ? new List<Challenge>()
                : await _questionService.GetSubjectPoolAsync(subject);

            if (pool.Count == 0)
            {
                return RedirectToAction(nameof(Topics));
            }

            var model = new PracticeViewModel
            {
                Subject = subject,
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

        // GET: /Challenge - anyone can browse and search
        public async Task<IActionResult> Index(string? searchString, string? planetName, int? minReward)
        {
            var challenges = await _challengeRepository.Search(searchString, planetName, minReward);
            if (challenges == null)
            {
                _logger.LogError("[ChallengeController] Challenge list not found while executing Search()");
                return NotFound();
            }

            // Only the logged-in user's own challenges (visitors see none, questions.json is never listed)
            var ownChallenges = challenges.Where(IsOwnChallenge).ToList();

            // Send the current filter values back so the form keeps them after submitting
            ViewData["SearchString"] = searchString;
            ViewData["PlanetName"] = planetName;
            ViewData["MinReward"] = minReward;

            return View(ownChallenges);
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
            return View(challenge);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            await LoadPlanetDropdown();
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Challenge challenge)
        {
            // Set on the server, so a user can't post their way into the planet missions or another user's list
            challenge.CreatedByUserId = _userManager.GetUserId(User);
            challenge.SourceId = null;
            challenge.Subject = null;
            challenge.Explanation = null;

            ValidateAnswerOptions(challenge);

            if (ModelState.IsValid)
            {
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
            _logger.LogWarning("[ChallengeController] Challenge creation failed for {Title}", challenge.Title);

            // The form is shown again, so the dropdown needs its planets again
            await LoadPlanetDropdown(challenge.PlanetId);
            return View(challenge);
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

            await LoadPlanetDropdown(challenge.PlanetId);
            return View(challenge);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Challenge challenge)
        {
            if (id != challenge.ChallengeId)
            {
                _logger.LogWarning("[ChallengeController] Edit id mismatch: route {RouteId}, form {FormId}", id, challenge.ChallengeId);
                return BadRequest();
            }

            // Load the saved challenge and check the owner there, not in the posted form
            var existing = await _challengeRepository.GetChallengeById(id);
            if (existing == null || !IsOwnChallenge(existing))
            {
                return NotFound();
            }

            ValidateAnswerOptions(challenge);

            if (ModelState.IsValid)
            {
                try
                {
                    // Copy only the fields the form edits, so owner and source can't be changed
                    existing.Title = challenge.Title;
                    existing.Question = challenge.Question;
                    existing.CorrectAnswer = challenge.CorrectAnswer;
                    existing.CreditReward = challenge.CreditReward;
                    existing.PlanetId = challenge.PlanetId;
                    existing.OptionA = challenge.OptionA;
                    existing.OptionB = challenge.OptionB;
                    existing.OptionC = challenge.OptionC;
                    existing.OptionD = challenge.OptionD;

                    await _challengeRepository.Update(existing);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    // E.g. the challenge was deleted by someone else while this form was open
                    ModelState.AddModelError(string.Empty, "Could not update the challenge. Please try again.");
                }
            }
            _logger.LogWarning("[ChallengeController] Challenge update failed for ChallengeId {ChallengeId:0000}", challenge.ChallengeId);

            await LoadPlanetDropdown(challenge.PlanetId);
            return View(challenge);
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
            return View(challenge);
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
}
