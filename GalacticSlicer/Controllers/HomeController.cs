using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GalacticSlicer.Models;

namespace GalacticSlicer.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
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

        _logger.LogInformation(
            "Contact message received. Title: {Title}, Description: {Description}",
            model.Title,
            model.Description);

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