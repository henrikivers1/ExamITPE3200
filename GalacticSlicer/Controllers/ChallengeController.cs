using GalacticSlicer.DAL;
using GalacticSlicer.Models;
using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GalacticSlicer.Controllers
{
    public class ChallengeController : Controller
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IPlanetRepository _planetRepository;
        private readonly ILogger<ChallengeController> _logger;

        public ChallengeController(IChallengeRepository challengeRepository, IPlanetRepository planetRepository, ILogger<ChallengeController> logger)
        {
            _challengeRepository = challengeRepository;
            _planetRepository = planetRepository;
            _logger = logger;
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

        public IActionResult Topics()
        {
            var topics = new List<ChallengeTopicViewModel>
            {
                new() { Name = "Cryptography Basics", IconKey = "crypto", Difficulty = "Beginner", ChallengeCount = 4 },
                new() { Name = "Network Security", IconKey = "network", Difficulty = "Beginner", ChallengeCount = 5 },
                new() { Name = "Web Application Security", IconKey = "web", Difficulty = "Intermediate", ChallengeCount = 6 },
                new() { Name = "Social Engineering & Phishing", IconKey = "phishing", Difficulty = "Intermediate", ChallengeCount = 3 },
                new() { Name = "Malware Analysis", IconKey = "malware", Difficulty = "Advanced", ChallengeCount = 4 },
                new() { Name = "Access Control & Authentication", IconKey = "access", Difficulty = "Advanced", ChallengeCount = 3 },
            };

            return View(topics);
        }

        // Placeholder - one topic with multiple choice questions
        public IActionResult Practice()
        {
            return View();
        }

        // GET: /Challenge - anyone can browse and search
        public async Task<IActionResult> Index(string? searchString, string? planetName, int? minReward)
        {
            var challenges = await _challengeRepository.Search(searchString, planetName, minReward);
            if (challenges == null)
            {
                _logger.LogError("[ChallengeController] Challenge list not found while executing Search()");
                return NotFound("Challenge list not found");
            }

            // Send the current filter values back so the form keeps them after submitting
            ViewData["SearchString"] = searchString;
            ViewData["PlanetName"] = planetName;
            ViewData["MinReward"] = minReward;

            return View(challenges);
        }

        // GET: /Challenge/Details/5 - anyone can view
        public async Task<IActionResult> Details(int id)
        {
            var challenge = await _challengeRepository.GetChallengeById(id);
            if (challenge == null)
            {
                _logger.LogError("[ChallengeController] Challenge not found for ChallengeId {ChallengeId:0000}", id);
                return NotFound("Challenge not found");
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
            ValidateAnswerOptions(challenge);

            if (ModelState.IsValid)
            {
                bool ok = await _challengeRepository.Create(challenge);
                if (ok)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, "Could not save the challenge. Please try again.");
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
            if (challenge == null)
            {
                _logger.LogError("[ChallengeController] Challenge not found when editing ChallengeId {ChallengeId:0000}", id);
                return NotFound("Challenge not found");
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
                return BadRequest("Challenge id mismatch");
            }

            ValidateAnswerOptions(challenge);

            if (ModelState.IsValid)
            {
                bool ok = await _challengeRepository.Update(challenge);
                if (ok)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, "Could not update the challenge. Please try again.");
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
            if (challenge == null)
            {
                _logger.LogError("[ChallengeController] Challenge not found when deleting ChallengeId {ChallengeId:0000}", id);
                return NotFound("Challenge not found");
            }
            return View(challenge);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool ok = await _challengeRepository.Delete(id);
            if (!ok)
            {
                _logger.LogError("[ChallengeController] Challenge deletion failed for ChallengeId {ChallengeId:0000}", id);
                return BadRequest("Challenge deletion failed");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
