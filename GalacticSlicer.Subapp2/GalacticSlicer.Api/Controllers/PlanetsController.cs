
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Services;
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

    // GET: /api/planets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlanetDto>>> GetPlanets()
    {
        var planets = await _planetService.GetAllAsync();

        return Ok(planets);
    }

    // GET: /api/planets/1
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

    // POST: /api/planets
    [HttpPost]
    public async Task<ActionResult<PlanetDto>> CreatePlanet(
        [FromBody] PlanetUpsertDto dto)
    {
        var planet = await _planetService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetPlanet),
            new { id = planet.PlanetId },
            planet);
    }

    // PUT: /api/planets/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePlanet(
        int id,
        [FromBody] PlanetUpsertDto dto)
    {
        var updated = await _planetService.UpdateAsync(id, dto);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: /api/planets/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePlanet(int id)
    {
        try
        {
            var deleted = await _planetService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException
                { SqliteErrorCode: 19 })
        {
            return Conflict(
                "Planet cannot be deleted because related data exists.");
        }
    }
}
