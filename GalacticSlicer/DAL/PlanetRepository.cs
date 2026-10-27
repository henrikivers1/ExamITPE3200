using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.DAL;

public class PlanetRepository : IPlanetRepository
{
    private readonly GalacticSlicerDbContext _db;
    private readonly ILogger<PlanetRepository> _logger;

    public PlanetRepository(GalacticSlicerDbContext db, ILogger<PlanetRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<Planet>?> GetAll()
    {
        try
        {
            // Sorted by chapter so the Galaxy page shows them in story order
            return await _db.Planets.Include(p => p.Challenges).OrderBy(p => p.Order).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("[PlanetRepository] planets ToListAsync() failed when GetAll(), error message: {e}", e.Message);
            return null;
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
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "[PlanetRepository] Failed to load planet {PlanetId}",
                id);

            return null;
        }
    }

    public async Task<bool> Create(Planet planet)
    {
        try
        {
            _db.Planets.Add(planet);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[PlanetRepository] planet creation failed for planet {@planet}, error message: {e}", planet, e.Message);
            return false;
        }
    }

    public async Task<bool> Update(Planet planet)
    {
        try
        {
            _db.Planets.Update(planet);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[PlanetRepository] planet update failed for PlanetId {PlanetId:0000}, error message: {e}", planet.PlanetId, e.Message);
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var planet = await _db.Planets.FindAsync(id);
            if (planet == null)
            {
                _logger.LogError("[PlanetRepository] planet not found for PlanetId {PlanetId:0000}", id);
                return false;
            }

            _db.Planets.Remove(planet);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[PlanetRepository] planet deletion failed for PlanetId {PlanetId:0000}, error message: {e}", id, e.Message);
            return false;
        }
    }
}