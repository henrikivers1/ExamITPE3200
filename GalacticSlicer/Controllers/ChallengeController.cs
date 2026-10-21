using GalacticSlicer.DAL;
using GalacticSlicer.Models;
using Microsoft.AspNetCore.Mvc;

namespace GalacticSlicer.Controllers
{
    public class ChallengeController : Controller
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly ILogger<ChallengeController> _logger;

        public ChallengeController(IChallengeRepository challengeRepository, ILogger<ChallengeController> logger)
        {
            _challengeRepository = challengeRepository;
            _logger = logger;
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

        public async Task<IActionResult> Index()
        {
            var challenges = await _challengeRepository.GetAll();
            if (challenges == null)
            {
                _logger.LogError("[ChallengeController] Challenge list not found while executing GetAll()");
                return NotFound("Challenge list not found");
            }
            return View(challenges);
        }

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
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Challenge challenge)
        {
            if (ModelState.IsValid)
            {
                bool ok = await _challengeRepository.Create(challenge);
                if (ok)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, "Could not save the challenge. Please try again.");
            }
            _logger.LogWarning("[ChallengeController] Challenge creation failed {@challenge}", challenge);
            return View(challenge);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var challenge = await _challengeRepository.GetChallengeById(id);
            if (challenge == null)
            {
                _logger.LogError("[ChallengeController] Challenge not found when editing ChallengeId {ChallengeId:0000}", id);
                return NotFound("Challenge not found");
            }
            return View(challenge);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Challenge challenge)
        {
            if (id != challenge.ChallengeId)
            {
                _logger.LogWarning("[ChallengeController] Edit id mismatch: route {RouteId}, form {FormId}", id, challenge.ChallengeId);
                return BadRequest("Challenge id mismatch");
            }

            if (ModelState.IsValid)
            {
                bool ok = await _challengeRepository.Update(challenge);
                if (ok)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, "Could not update the challenge. Please try again.");
            }
            _logger.LogWarning("[ChallengeController] Challenge update failed {@challenge}", challenge);
            return View(challenge);
        }

        [HttpGet]
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