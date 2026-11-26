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

    public NavUserViewComponent(UserManager<IdentityUser> userManager,
        PlayerProgressService progressService, ICharacterRepository characterRepository)
    {
        _userManager = userManager;
        _progressService = progressService;
        _characterRepository = characterRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Visitors have no Credits or badge, so nothing is shown
        var userId = _userManager.GetUserId(UserClaimsPrincipal);
        if (userId == null)
        {
            return Content(string.Empty);
        }

        var progress = await _progressService.GetOrCreateProgressAsync(userId);

        Character? activeCharacter = progress.ActiveCharacterId == null
            ? null
            : await _characterRepository.GetCharacterById(progress.ActiveCharacterId.Value);

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