using GalacticSlicer.DAL;
using GalacticSlicer.Models;
using Microsoft.AspNetCore.Mvc;

namespace GalacticSlicer.Controllers;

public class PlanetController : Controller
{
    private readonly IPlanetRepository _planetRepository;
    private readonly ILogger<PlanetController> _logger;

    public PlanetController(IPlanetRepository planetRepository, ILogger<PlanetController> logger)
    {
        _planetRepository = planetRepository;
        _logger = logger;
    }

    // GET: /Planet - the Galaxy page
    public async Task<IActionResult> Index()
    {
        var planets = await _planetRepository.GetAll();
        if (planets == null)
        {
            _logger.LogError("[PlanetController] Planet list not found while executing GetAll()");
            return NotFound("Planet list not found");
        }

        // Turn database entities into exactly what the Galaxy page needs
        var viewModels = planets.Select(p => new PlanetViewModel
        {
            PlanetId = p.PlanetId,
            Name = p.Name,
            CssClass = p.CssClass,
            StepLabel = $"Chapter {p.Order}",
            ChallengeCount = p.Challenges?.Count ?? 0,       // TODO 10b: real count once challenges belong to planets
            IsUnlocked = p.Order == 1 // TODO Task 14: real unlocking from Jonas's service
        }).ToList();

        return View(viewModels);
    }

    // GET: /Planet/Mission/1 - the mission (exam) for one planet
    public async Task<IActionResult> Mission(int id)
    {
        var planet = await _planetRepository.GetPlanetById(id);
        if (planet == null)
        {
            _logger.LogError("[PlanetController] Planet not found for PlanetId {PlanetId:0000}", id);
            return NotFound("Planet not found");
        }

        // Locked planets can't be played: same rule as the Galaxy page
        if (planet.Order != 1) // TODO Task 14: real unlocking from Jonas's service
        {
            return RedirectToAction(nameof(Index));
        }

        // Share of correct answers needed to beat the mission
        ViewData["PassPercent"] = 80;

        return View(planet);
    }
}