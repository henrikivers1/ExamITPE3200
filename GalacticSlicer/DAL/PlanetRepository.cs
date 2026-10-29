
using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.DAL;

public class PlanetRepository : IPlanetRepository
{
    private readonly GalacticSlicerDbContext _db;
    private readonly ILogger<PlanetRepository> _logger;

    public PlanetRepository(
        GalacticSlicerDbContext db,
        ILogger<PlanetRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<Planet>?> GetAll()
    {
        try
        {
            // Sorted by chapter so the Galaxy page shows them in story order
            return await _db.Planets
                .Include(p => p.Challenges)
                .OrderBy(p => p.Order)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log the full exception, including its stack trace
            _logger.LogError(
                ex,
                "Failed to retrieve all planets");

            throw;
        }
    }

    public async Task<Planet?> GetPlanetById(int id)
    {
        try
        {
            return await _db.Planets
                .Include(p => p.Challenges)
                .FirstOrDefaultAsync(p => p.PlanetId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve Planet {PlanetId}",
                id);

            throw;
        }
    }

    public async Task<bool> Create(Planet planet)
    {
        try
        {
            _db.Planets.Add(planet);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Planet {PlanetId} created successfully",
                planet.PlanetId);

            return true;
        }
        catch (Exception ex)
        {
            // Log the planet ID instead of the entire object
            _logger.LogError(
                ex,
                "Failed to create Planet {PlanetId}",
                planet.PlanetId);

            throw;
        }
    }

    public async Task<bool> Update(Planet planet)
    {
        try
        {
            _db.Planets.Update(planet);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Planet {PlanetId} updated successfully",
                planet.PlanetId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update Planet {PlanetId}",
                planet.PlanetId);

            throw;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var planet = await _db.Planets.FindAsync(id);

            if (planet == null)
            {
                // A missing planet is an expected outcome, not a database error
                _logger.LogWarning(
                    "Planet {PlanetId} not found during deletion",
                    id);

                return false;
            }

            // Delete the planet
            _db.Planets.Remove(planet);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Planet {PlanetId} deleted successfully",
                id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete Planet {PlanetId}",
                id);

            throw;
        }
    }
}
