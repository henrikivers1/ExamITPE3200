
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Services;
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
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CharacterDto>>> GetAll()
    {
        var characters = await _service.GetAllAsync();
        return Ok(characters);
    }

    // GET /api/characters/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CharacterDto>> GetById(int id)
    {
        var character = await _service.GetByIdAsync(id);

        if (character == null)
            return NotFound();

        return Ok(character);
    }

    // POST /api/characters
    [HttpPost]
    public async Task<ActionResult<CharacterDto>> Create(
        CharacterUpsertDto dto)
    {
        var character = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = character.CharacterId },
            character);
    }

    // PUT /api/characters/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, CharacterUpsertDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);

        return updated ? NoContent() : NotFound();
    }

    // DELETE /api/characters/1
    [HttpDelete("{id:int}")]
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
