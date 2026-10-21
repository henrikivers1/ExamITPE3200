using GalacticSlicer.DAL;
using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GalacticSlicer.Controllers;

public class CharacterController : Controller
{
    private readonly ICharacterRepository _characterRepository;
    private readonly ILogger<CharacterController> _logger;

    public CharacterController(ICharacterRepository characterRepository, ILogger<CharacterController> logger)
    {
        _characterRepository = characterRepository;
        _logger = logger;
    }

    // GET: /Character/Shop - anyone can browse the shop
    public async Task<IActionResult> Shop()
    {
        var characters = await _characterRepository.GetAll();
        if (characters == null)
        {
            _logger.LogError("[CharacterController] Character list not found while executing GetAll()");
            return NotFound("Character list not found");
        }

        var viewModel = new CharacterShopViewModel
        {
            Characters = characters,
            PlayerCredits = 0 // TODO Task 11: real credits from the logged-in player
        };
        return View(viewModel);
    }

    // POST: /Character/Buy - only logged-in players
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Buy(int characterId)
    {
        // Never trust ids from a form: check that the character exists
        var character = await _characterRepository.GetCharacterById(characterId);
        if (character == null)
        {
            _logger.LogWarning("[CharacterController] Buy attempted with invalid CharacterId {CharacterId:0000}", characterId);
            TempData["ErrorMessage"] = "That character does not exist.";
            return RedirectToAction(nameof(Shop));
        }

        // TODO Task 11: call Jonas's CharacterService to check Credits and save the purchase
        _logger.LogInformation("[CharacterController] Buy requested for {CharacterName} by {Username}", character.Name, User.Identity?.Name);
        TempData["SuccessMessage"] = $"Buying {character.Name} is coming soon!";
        return RedirectToAction(nameof(Shop));
    }

    // POST: /Character/Equip - only logged-in players
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Equip(int characterId)
    {
        var character = await _characterRepository.GetCharacterById(characterId);
        if (character == null)
        {
            _logger.LogWarning("[CharacterController] Equip attempted with invalid CharacterId {CharacterId:0000}", characterId);
            TempData["ErrorMessage"] = "That character does not exist.";
            return RedirectToAction(nameof(Shop));
        }

        // TODO Task 11: check that the player owns it, then save it as equipped
        TempData["SuccessMessage"] = $"Equipping {character.Name} is coming soon!";
        return RedirectToAction(nameof(Shop));
    }
}