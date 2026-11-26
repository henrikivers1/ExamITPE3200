
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/characters")]
public class CharacterGameplayController : ControllerBase
{
    private readonly CharacterGameplayService _gameplay;

    public CharacterGameplayController(
        CharacterGameplayService gameplay)
    {
        _gameplay = gameplay;
    }

    private string? UserId =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    // GET /api/characters/owned
    [HttpGet("owned")]
    public async Task<IActionResult> GetOwned()
    {
        if (UserId == null) return Unauthorized();

        var ids = await _gameplay.GetOwnedCharacterIdsAsync(UserId);
        return Ok(ids);
    }

    // POST /api/characters/1/buy
    [HttpPost("{id:int}/buy")]
    public async Task<IActionResult> Buy(int id)
    {
        if (UserId == null) return Unauthorized();

        var success = await _gameplay.BuyCharacterAsync(UserId, id);

        return success
            ? Ok(new { message = "Character purchased" })
            : Conflict(new { message = "Purchase rejected" });
    }

    // POST /api/characters/1/equip
    [HttpPost("{id:int}/equip")]
    public async Task<IActionResult> Equip(int id)
    {
        if (UserId == null) return Unauthorized();

        var success = await _gameplay.EquipCharacterAsync(UserId, id);

        return success
            ? Ok(new { message = "Character equipped" })
            : Conflict(new { message = "Equip rejected" });
    }

    // POST /api/characters/unequip
    [HttpPost("unequip")]
    public async Task<IActionResult> Unequip()
    {
        if (UserId == null) return Unauthorized();

        var success = await _gameplay.UnequipCharacterAsync(UserId);

        return success
            ? Ok(new { message = "Character unequipped" })
            : Conflict(new { message = "Unequip rejected" });
    }
}
