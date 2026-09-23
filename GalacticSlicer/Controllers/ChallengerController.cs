using ExamITPE3200.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExamITPE3200.Controllers
{
    public class ChallengeController : Controller
    {
        private readonly GalacticSlicerDbContext _context;

        public ChallengeController(GalacticSlicerDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var challenges = _context.Challenges.ToList();

            return View(challenges);
        }

        public IActionResult Details(int id)
        {
            var challenge = _context.Challenges.FirstOrDefault(c => c.ChallengeId == id);

            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Challenge challenge)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Challenges.Add(challenge);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred while creating the challenge: {ex.Message}");
                    return View(challenge);
                }

                return RedirectToAction(nameof(Index));
            }

            return View(challenge);
        }
        public IActionResult Edit(int id)
        {
            var challenge = _context.Challenges.Find(id);

            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Challenge challenge)
        {
            if (id != challenge.ChallengeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Challenges.Update(challenge);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred while updating the challenge: {ex.Message}");
                    return View(challenge);
                }

                return RedirectToAction(nameof(Index));
            }

            return View(challenge);
        }

        public IActionResult Delete(int id)
        {
            var challenge = _context.Challenges.Find(id);

            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var challenge = _context.Challenges.Find(id);

            if (challenge == null)
            {
                return NotFound();
            }

            try
            {
                _context.Challenges.Remove(challenge);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while deleting the challenge: {ex.Message}");
                return View("Delete", challenge);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
