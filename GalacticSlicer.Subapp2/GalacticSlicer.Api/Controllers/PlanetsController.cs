
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
    private readonly ILogger<PlanetsController> _logger;

    public PlanetsController(
        IPlanetService planetService,
        ILogger<PlanetsController> logger)
    {
        _planetService = planetService;
        _logger = logger;
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

            _logger.LogInformation(
                "Planet {PlanetId} created",
                planet.PlanetId);

            return CreatedAtAction(
                nameof(GetPlanet),
                new { id = planet.PlanetId },
                planet);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                "Planet rejected ({Error}): {Message}",
                "ArgumentException",
                ex.Message);

            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Planet rejected ({Error}): {Message}",
                "InvalidOperationException",
                ex.Message);

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

            _logger.LogInformation(
                "Update of planet {PlanetId}: {Updated}",
                id,
                updated);

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
            _logger.LogWarning(
                "Planet rejected ({Error}): {Message}",
                "ArgumentException",
                ex.Message);

            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Planet rejected ({Error}): {Message}",
                "InvalidOperationException",
                ex.Message);

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

            _logger.LogInformation(
                "Delete of planet {PlanetId}: {Deleted}",
                id,
                deleted);

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
            _logger.LogWarning(
                "Planet rejected ({Error}): {Message}",
                "InvalidOperationException",
                ex.Message);

            return Conflict(new
            {
                message = ex.Message
            });
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException
                { SqliteErrorCode: 19 })
        {
            _logger.LogWarning(
                "Planet rejected ({Error}): {Message}",
                "DbUpdateException",
                ex.Message);

            return Conflict(new
            {
                message =
                    "Planet cannot be deleted because " +
                    "related data exists."
            });
        }
    }
}
