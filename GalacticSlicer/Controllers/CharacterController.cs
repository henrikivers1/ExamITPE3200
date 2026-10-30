using GalacticSlicer.DAL;
using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GalacticSlicer.Services;
using Microsoft.AspNetCore.Identity;

namespace GalacticSlicer.Controllers;

public class CharacterController : Controller
{
    private readonly ICharacterRepository _characterRepository;
    private readonly ILogger<CharacterController> _logger;
    private readonly CharacterService _characterService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly PlayerProgressService _progressService;

    public CharacterController(ICharacterRepository characterRepository, ILogger<CharacterController> logger, CharacterService characterService, UserManager<IdentityUser> userManager, PlayerProgressService progressService)
    {
        _characterRepository = characterRepository;
        _logger = logger;
        _userManager = userManager;
        _characterService = characterService;
        _progressService = progressService;
    }

    // GET: /Character/Shop - anyone can browse the shop

    public async Task<IActionResult> Shop()
    {
        var characters = await _characterRepository.GetAll();

        if (characters == null)
        {
            _logger.LogError("Character list not found");
            return NotFound("Character list not found");
        }

        int playerCredits = 0;

        var ownedCharacterIds = new List<int>();
        int? activeCharacterId = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);

            if (userId != null)
            {

                var progress = await _progressService
                    .GetOrCreateProgressAsync(userId);

                playerCredits = progress.Credits;

                ownedCharacterIds = await _characterService
                 .GetOwnedCharacterIdsAsync(userId);

                activeCharacterId = progress.ActiveCharacterId;
            }
        }

        var viewModel = new CharacterShopViewModel
        {
            Characters = characters,
            PlayerCredits = playerCredits,
            OwnedCharacterIds = ownedCharacterIds,
            ActiveCharacterId = activeCharacterId
        };

        return View(viewModel);
    }


    // POST: /Character/Buy - only logged-in players

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Buy(int characterId)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Unauthorized();

        var character = await _characterRepository.GetCharacterById(characterId);

        if (character == null)
        {
            TempData["ErrorMessage"] = "That character does not exist.";
            return RedirectToAction(nameof(Shop));
        }

        bool purchased = await _characterService
            .BuyCharacterAsync(userId, characterId);

        if (!purchased)
        {
            TempData["ErrorMessage"] =
                "Purchase failed. Not enough Credits or already owned.";
        }
        else
        {
            TempData["SuccessMessage"] =
                $"You purchased {character.Name}!";
        }

        return RedirectToAction(nameof(Shop));
    }


    // POST: /Character/Equip - only logged-in players
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Equip(int characterId)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Unauthorized();

        bool equipped = await _characterService
            .EquipCharacterAsync(userId, characterId);

        if (!equipped)
        {
            TempData["ErrorMessage"] =
                "You must own this character before equipping it.";
        }
        else
        {
            TempData["SuccessMessage"] =
                "Character equipped successfully!";
        }

        return RedirectToAction(nameof(Shop));
    }


    // POST: /Character/Unequip - removes the equipped character, so no badge is shown
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unequip()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Unauthorized();

        bool unequipped = await _characterService
            .UnequipCharacterAsync(userId);

        if (!unequipped)
        {
            TempData["ErrorMessage"] =
                "Could not unequip your character.";
        }
        else
        {
            TempData["SuccessMessage"] =
                "Character unequipped.";
        }

        return RedirectToAction(nameof(Shop));
    }
}