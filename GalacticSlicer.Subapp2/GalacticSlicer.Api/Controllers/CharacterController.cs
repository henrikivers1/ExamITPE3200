
using System.ComponentModel.DataAnnotations;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Route("api/characters")]
public class CharactersController : ControllerBase
{
    private readonly ICharacterService _service;
    private readonly ILogger<CharactersController> _logger;

    public CharactersController(
        ICharacterService service,
        ILogger<CharactersController> logger)
    {
        _service = service;
        _logger = logger;
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
            return NotFound(new
            {
                message = "Character not found."
            });
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
        try
        {
            var character = await _service.CreateAsync(dto);

            _logger.LogInformation(
                "Character {CharacterId} created",
                character.CharacterId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = character.CharacterId },
                character);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(
                "Character rejected ({Error}): {Message}",
                "ValidationException",
                ex.Message);

            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (CharacterConflictException ex)
        {
            _logger.LogWarning(
                "Character rejected ({Error}): {Message}",
                "CharacterConflictException",
                ex.Message);

            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // PUT /api/characters/1
    // Admin only - update a character
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CharacterUpsertDto dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);

            _logger.LogInformation(
                "Update of character {CharacterId}: {Updated}",
                id,
                updated);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Character not found."
                });
            }

            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(
                "Character rejected ({Error}): {Message}",
                "ValidationException",
                ex.Message);

            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (CharacterConflictException ex)
        {
            _logger.LogWarning(
                "Character rejected ({Error}): {Message}",
                "CharacterConflictException",
                ex.Message);

            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // DELETE /api/characters/1
    // Admin only - delete a character
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);

            _logger.LogInformation(
                "Delete of character {CharacterId}: {Result}",
                id,
                result);

            return result switch
            {
                CharacterDeleteResult.Deleted => NoContent(),
                CharacterDeleteResult.NotFound => NotFound(new
                {
                    message = "Character not found."
                }),
                CharacterDeleteResult.HasOwners => Conflict(new
                {
                    message =
                        "Character cannot be deleted because " +
                        "it is owned by players."
                }),
                CharacterDeleteResult.StandardCharacter => Conflict(new
                {
                    message = "Standard characters cannot be deleted."
                }),
                _ => StatusCode(500)
            };
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException
                { SqliteErrorCode: 19 })
        {
            _logger.LogWarning(
                "Character rejected ({Error}): {Message}",
                "DbUpdateException",
                ex.Message);

            return Conflict(new
            {
                message =
                    "Character cannot be deleted because " +
                    "it is owned by players."
            });
        }
    }
}
