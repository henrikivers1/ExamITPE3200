using GalacticSlicer.DAL;
using GalacticSlicer.Models;
using GalacticSlicer.Services;
using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GalacticSlicer.ViewComponents;

// A view component is like a small controller for one part of the layout,
// so _Layout.cshtml no longer needs the DbContext.
public class NavUserViewComponent : ViewComponent
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IPlayerProgressService _progressService;
    private readonly ICharacterRepository _characterRepository;
    private readonly ILogger<NavUserViewComponent> _logger;

    public NavUserViewComponent(UserManager<IdentityUser> userManager,
        IPlayerProgressService progressService, ICharacterRepository characterRepository,
        ILogger<NavUserViewComponent> logger)
    {
        _userManager = userManager;
        _progressService = progressService;
        _characterRepository = characterRepository;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Visitors have no Credits or badge, so nothing is shown
        var userId = _userManager.GetUserId(UserClaimsPrincipal);
        if (userId == null)
        {
            return Content(string.Empty);
        }

        PlayerProgress progress;
        Character? activeCharacter;
        try
        {
            progress = await _progressService.GetOrCreateProgressAsync(userId);

            activeCharacter = progress.ActiveCharacterId == null
                ? null
                : await _characterRepository.GetCharacterById(progress.ActiveCharacterId.Value);
        }
        catch (Exception ex)
        {
            // Runs inside the layout, so an error here would break every page: log it and hide the badge and Credits
            _logger.LogError(ex, "[NavUserViewComponent] Could not load the navbar data for the player");
            return Content(string.Empty);
        }

        var model = new NavUserViewModel
        {
            Username = UserClaimsPrincipal.Identity?.Name ?? string.Empty,
            Credits = progress.Credits,
            ActiveCharacter = activeCharacter
        };

        // Renders Views/Shared/Components/NavUser/Default.cshtml
        return View(model);
    }
}