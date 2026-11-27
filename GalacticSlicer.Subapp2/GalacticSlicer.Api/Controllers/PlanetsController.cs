
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Route("api/planets")]
public class PlanetsController : ControllerBase
{
    private readonly IPlanetService _planetService;

    public PlanetsController(IPlanetService planetService)
    {
        _planetService = planetService;
    }

    // ============================================
    // GET ALL - PUBLIC
    // ============================================

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlanetDto>>> GetPlanets()
    {
        var planets = await _planetService.GetAllAsync();

        return Ok(planets);
    }

    // ============================================
    // GET BY ID - PUBLIC
    // ============================================

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlanetDto>> GetPlanet(int id)
    {
        var planet = await _planetService.GetByIdAsync(id);

        if (planet == null)
        {
            return NotFound();
        }

        return Ok(planet);
    }

    // ============================================
    // CREATE - ADMIN
    // ============================================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PlanetDto>> CreatePlanet(
        [FromBody] PlanetUpsertDto dto)
    {
        try
        {
            var planet = await _planetService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetPlanet),
                new { id = planet.PlanetId },
                planet);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // ============================================
    // UPDATE - ADMIN
    // ============================================

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePlanet(
        int id,
        [FromBody] PlanetUpsertDto dto)
    {
        try
        {
            var updated = await _planetService.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Planet not found."
                });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // ============================================
    // DELETE - ADMIN
    // ============================================

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePlanet(int id)
    {
        try
        {
            var deleted = await _planetService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Planet not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException
                { SqliteErrorCode: 19 })
        {
            return Conflict(new
            {
                message =
                    "Planet cannot be deleted because " +
                    "related data exists."
            });
        }
    }
}
