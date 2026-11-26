using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GalacticSlicer.DAL;
using GalacticSlicer.Models;
using GalacticSlicer.Services;
using GalacticSlicer.ViewModels;

namespace GalacticSlicer.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IPlayerProgressService _progressService;
    private readonly IPlanetProgressService _planetProgressService;
    private readonly IChallengeCompletionService _completionService;
    private readonly IPlanetRepository _planetRepository;

    public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager,
        PlayerProgressService progressService, PlanetProgressService planetProgressService,
        ChallengeCompletionService completionService, IPlanetRepository planetRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _progressService = progressService;
        _planetProgressService = planetProgressService;
        _completionService = completionService;
        _planetRepository = planetRepository;
    }

    // GET: / - the intro for everyone; players also get a greeting and a button to their next mission
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        // Visitors only see the intro, so no database work is needed
        if (userId == null)
        {
            return View(new HomeViewModel { IsLoggedIn = false });
        }

        var progress = await _progressService.GetOrCreateProgressAsync(userId);
        var completedPlanetIds = await _completionService.GetCompletedPlanetIdsAsync(userId);
        var planets = await _planetRepository.GetAll() ?? new List<Planet>();
        var statuses = _planetProgressService.GetPlanetStatuses(planets, progress, completedPlanetIds);

        var model = new HomeViewModel
        {
            IsLoggedIn = true,
            Username = User.Identity?.Name ?? "Slicer",
            NextPlanet = statuses.FirstOrDefault(p => p.IsUnlocked && !p.IsCompleted),
            LastUnlockedPlanet = statuses.LastOrDefault(p => p.IsUnlocked)
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Contact()
    {
        return View(new ContactMessageViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(ContactMessageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

                // Only the title is logged: the message text can contain personal data
        _logger.LogInformation("[HomeController] Contact message received: {Title}", model.Title);

        TempData["ContactSuccess"] = true;
        return RedirectToAction(nameof(Contact));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // GET: /Home/HttpStatus/404 - friendly page for status codes (see UseStatusCodePagesWithReExecute in Program.cs)
    public IActionResult HttpStatus(int id)
    {
        // The URL the player actually asked for, before ASP.NET re-ran this action
        var originalPath = HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath;

        _logger.LogWarning("[HomeController] Status code {StatusCode} for path {Path}", id, originalPath);

        return View(id);
    }
}