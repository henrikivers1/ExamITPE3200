using ExamITPE3200.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExamITPE3200.Controllers;

public class ProgressController : Controller
{

    public IActionResult Index()
    {
        var planetsUnlocked = 1;
        var planetsTotal = 6;

        var model = new ProgressViewModel
        {
            Username = "Slicer_01",
            MemberSince = new DateTime(2026, 9, 12),
            Credits = 120,
            ChallengesCompleted = 4,
            ChallengesTotal = 25,
            PlanetsUnlocked = planetsUnlocked,
            PlanetsTotal = planetsTotal,
            Rank = GetRank(planetsUnlocked),
        };

        return View(model);
    }

    private static string GetRank(int planetsUnlocked)
    {
        return planetsUnlocked switch
        {
            0 => "Initiate",
            1 or 2 => "Slicer",
            3 or 4 => "Rogue Slicer",
            5 => "Shadow Operative",
            _ => "Holonet Ghost",
        };
    }
}
