using Microsoft.AspNetCore.Mvc;
using GalacticSlicer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanetsController : ControllerBase
{

    private readonly GalacticSlicerDbContext _context;

    public PlanetsController(GalacticSlicerDbContext context)
    {
        _context = context;
    }


    [HttpGet]
    public async Task<IActionResult> GetPlanets()
    {
        var planets = await _context.Planets
            .AsNoTracking()
            .ToListAsync();

        return Ok(planets);
    }



}