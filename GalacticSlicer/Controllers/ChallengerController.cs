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
                _context.Challenges.Add(challenge);
                _context.SaveChanges();

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
                _context.Challenges.Update(challenge);
                _context.SaveChanges();

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

            _context.Challenges.Remove(challenge);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
