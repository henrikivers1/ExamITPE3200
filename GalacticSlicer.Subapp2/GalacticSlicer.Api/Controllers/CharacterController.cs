
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Route("api/characters")]
public class CharactersController : ControllerBase
{
    private readonly ICharacterService _service;

    public CharactersController(ICharacterService service)
    {
        _service = service;
    }

    // GET /api/characters
    // Public - everyone can view characters
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CharacterDto>>> GetAll()
    {
        var characters = await _service.GetAllAsync();

        return Ok(characters);
    }

    // GET /api/characters/1
    // Public - everyone can view a specific character
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CharacterDto>> GetById(int id)
    {
        var character = await _service.GetByIdAsync(id);

        if (character == null)
        {
            return NotFound();
        }

        return Ok(character);
    }

    // POST /api/characters
    // Admin only - create a character
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CharacterDto>> Create(
        [FromBody] CharacterUpsertDto dto)
    {
        var character = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = character.CharacterId },
            character);
    }

    // PUT /api/characters/1
    // Admin only - update a character
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CharacterUpsertDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);

        return updated ? NoContent() : NotFound();
    }

    // DELETE /api/characters/1
    // Admin only - delete a character
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        return result switch
        {
            CharacterDeleteResult.Deleted => NoContent(),
            CharacterDeleteResult.NotFound => NotFound(),
            CharacterDeleteResult.HasOwners =>
                Conflict("Character is owned by players."),
            _ => StatusCode(500)
        };
    }
}
