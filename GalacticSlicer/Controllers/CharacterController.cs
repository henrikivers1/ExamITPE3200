using GalacticSlicer.DAL;
using GalacticSlicer.ViewModels;
using GalacticSlicer.Models;
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
            _logger.LogError("[CharacterController] Character list not found");
            return NotFound();
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
        // ---------- Manage characters (CRUD) ----------

    // Domain rules for characters, checked on the server for both Create and Edit
    private async Task ValidateCharacterRules(Character character)
    {
        // Every character except the one being edited
        var others = (await _characterRepository.GetAll() ?? Enumerable.Empty<Character>())
            .Where(c => c.CharacterId != character.CharacterId)
            .ToList();

        if (others.Any(c => string.Equals(c.Name, character.Name, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(character.Name), "A character with this name already exists.");
        }

        // New players have 0 Credits, so the Shop must always have a free character
        if (character.Price > 0 && !others.Any(c => c.Price == 0))
        {
            ModelState.AddModelError(nameof(character.Price),
                "At least one character must stay free (price 0) for new players.");
        }
    }

    // Builds one row for the Manage and Delete pages, including the Delete rule:
    // nobody may own the character, and it can't be the last free one
    private async Task<CharacterManageViewModel> ToManageRow(Character character, List<Character> all)
    {
        int owners = await _characterRepository.CountOwners(character.CharacterId);
        bool isLastFree = character.Price == 0 &&
            !all.Any(c => c.CharacterId != character.CharacterId && c.Price == 0);

        return new CharacterManageViewModel
        {
            Character = character,
            OwnerCount = owners,
            CanDelete = owners == 0 && !isLastFree
        };
    }
        // GET: /Character/Manage - table of all characters with Edit and Delete buttons
    [Authorize]
    public async Task<IActionResult> Manage()
    {
        var characters = await _characterRepository.GetAll();
        if (characters == null)
        {
            _logger.LogError("[CharacterController] Character list not found while executing Manage()");
            return NotFound();
        }

        var all = characters.ToList();
        var rows = new List<CharacterManageViewModel>();
        foreach (var character in all)
        {
            rows.Add(await ToManageRow(character, all));
        }

        return View(rows);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Character character)
    {
        // Set on the server: always a new id, and no image URL from the form
        character.CharacterId = 0;
        character.ImageUrl = null;

        await ValidateCharacterRules(character);

        if (ModelState.IsValid)
        {
            try
            {
                await _characterRepository.Create(character);
                TempData["SuccessMessage"] = $"{character.Name} was added to the Shop.";
                return RedirectToAction(nameof(Manage));
            }
            catch (Exception)
            {
                // The repository already logged the full exception
                ModelState.AddModelError(string.Empty, "Could not save the character. Please try again.");
            }
        }
        _logger.LogWarning("[CharacterController] Character creation failed for {Name}", character.Name);
        return View(character);
    }
        [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var character = await _characterRepository.GetCharacterById(id);
        if (character == null)
        {
            _logger.LogError("[CharacterController] Character not found when editing CharacterId {CharacterId}", id);
            return NotFound();
        }

        return View(character);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Character character)
    {
        if (id != character.CharacterId)
        {
            _logger.LogWarning("[CharacterController] Edit id mismatch: route {RouteId}, form {FormId}", id, character.CharacterId);
            return BadRequest();
        }

        var existing = await _characterRepository.GetCharacterById(id);
        if (existing == null)
        {
            return NotFound();
        }

        await ValidateCharacterRules(character);

        if (ModelState.IsValid)
        {
            try
            {
                // Copy only the fields the form edits (ImageUrl stays as it is)
                existing.Name = character.Name;
                existing.Price = character.Price;
                existing.Description = character.Description;

                await _characterRepository.Update(existing);
                TempData["SuccessMessage"] = $"{existing.Name} was updated.";
                return RedirectToAction(nameof(Manage));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Could not update the character. Please try again.");
            }
        }
        _logger.LogWarning("[CharacterController] Character update failed for CharacterId {CharacterId}", id);
        return View(character);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var character = await _characterRepository.GetCharacterById(id);
        if (character == null)
        {
            _logger.LogError("[CharacterController] Character not found when deleting CharacterId {CharacterId}", id);
            return NotFound();
        }

        var all = (await _characterRepository.GetAll())?.ToList() ?? new List<Character>();
        return View(await ToManageRow(character, all));
    }

    [HttpPost, ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var character = await _characterRepository.GetCharacterById(id);
        if (character == null)
        {
            return NotFound();
        }

        // Checked again here: the page hides the button, but a POST can be sent without the page
        var all = (await _characterRepository.GetAll())?.ToList() ?? new List<Character>();
        var row = await ToManageRow(character, all);
        if (!row.CanDelete)
        {
            _logger.LogWarning("[CharacterController] Blocked deletion of Character {CharacterId}", id);
            TempData["ErrorMessage"] = $"{character.Name} can't be deleted: players own it, or it is the last free character.";
            return RedirectToAction(nameof(Manage));
        }

        try
        {
            if (!await _characterRepository.Delete(id))
            {
                return NotFound();
            }
        }
        catch (Exception)
        {
            _logger.LogWarning("[CharacterController] Character deletion failed for CharacterId {CharacterId}", id);
            TempData["ErrorMessage"] = "Could not delete the character. Please try again.";
            return RedirectToAction(nameof(Manage));
        }

        TempData["SuccessMessage"] = $"{character.Name} was removed from the Shop.";
        return RedirectToAction(nameof(Manage));
    }
}