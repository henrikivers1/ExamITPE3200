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
            return View();
        }

        public IActionResult Details(int id)
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            return View();
        }

        public IActionResult Delete(int id)
        {
            return View();
        }
    }
}