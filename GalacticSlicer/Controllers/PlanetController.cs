using ExamITPE3200.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExamITPE3200.Controllers;

public class PlanetController : Controller
{
    // Hardcoded placeholder route for now - only Tatooine is unlocked.
    // Will later come from signed users progress.
    public IActionResult Index()
    {
        var planets = new List<PlanetViewModel>
        {
            new() { Name = "Tatooine", CssClass = "tatooine", StepLabel = "Chapter 1", ChallengeCount = 4, IsUnlocked = true },
            new() { Name = "Naboo", CssClass = "naboo", StepLabel = "Chapter 2", ChallengeCount = 3, IsUnlocked = false },
            new() { Name = "Coruscant", CssClass = "coruscant", StepLabel = "Chapter 3", ChallengeCount = 5, IsUnlocked = false },
            new() { Name = "Kamino", CssClass = "kamino", StepLabel = "Chapter 4", ChallengeCount = 3, IsUnlocked = false },
            new() { Name = "Hoth", CssClass = "hoth", StepLabel = "Chapter 5", ChallengeCount = 4, IsUnlocked = false },
            new() { Name = "Endor", CssClass = "endor", StepLabel = "Chapter 6", ChallengeCount = 6, IsUnlocked = false },
        };

        return View(planets);
    }
}
