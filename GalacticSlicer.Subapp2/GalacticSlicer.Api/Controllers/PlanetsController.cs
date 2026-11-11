using GalacticSlicer.Api.DAL;
using Microsoft.AspNetCore.Mvc;
using GalacticSlicer.Api.DTOs;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Route("api/planets")]
public class PlanetsController : ControllerBase
{
    private readonly IPlanetRepository _planetRepository;

    public PlanetsController(IPlanetRepository planetRepository)
    {
        _planetRepository = planetRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlanets()
    {
        var planets = await _planetRepository.GetAll();

        if (planets == null)
        {
            return Ok(new List<PlanetDto>());
        }

        var planetDtos = planets.Select(p => new PlanetDto
        {
            PlanetId = p.PlanetId,
            Name = p.Name,
            Description = p.Description,
            Order = p.Order,
            CssClass = p.CssClass,
            Subject = p.Subject
        }).ToList();

        return Ok(planetDtos);
    }
}