using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.DAL;

public class ChallengeRepository : IChallengeRepository
{
    private readonly GalacticSlicerDbContext _db;
    private readonly ILogger<ChallengeRepository> _logger;

    public ChallengeRepository(GalacticSlicerDbContext db, ILogger<ChallengeRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<Challenge>?> GetAll()
    {
        try
        {
            return await _db.Challenges.Include(c => c.Planet).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("[ChallengeRepository] challenges ToListAsync() failed when GetAll(), error message: {e}", e.Message);
            return null;
        }
    }

    public async Task<Challenge?> GetChallengeById(int id)
    {
        try
        {
            return await _db.Challenges.Include(c => c.Planet).FirstOrDefaultAsync(c => c.ChallengeId == id);
        }
        catch (Exception e)
        {
            _logger.LogError("[ChallengeRepository] challenge FindAsync(id) failed for ChallengeId {ChallengeId:0000}, error message: {e}", id, e.Message);
            return null;
        }
    }

    public async Task<bool> Create(Challenge challenge)
    {
        try
        {
            _db.Challenges.Add(challenge);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[ChallengeRepository] challenge creation failed for challenge {@challenge}, error message: {e}", challenge, e.Message);
            return false;
        }
    }

    public async Task<bool> Update(Challenge challenge)
    {
        try
        {
            _db.Challenges.Update(challenge);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[ChallengeRepository] challenge update failed for ChallengeId {ChallengeId:0000}, error message: {e}", challenge.ChallengeId, e.Message);
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var challenge = await _db.Challenges.FindAsync(id);
            if (challenge == null)
            {
                _logger.LogError("[ChallengeRepository] challenge not found for ChallengeId {ChallengeId:0000}", id);
                return false;
            }

            _db.Challenges.Remove(challenge);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[ChallengeRepository] challenge deletion failed for ChallengeId {ChallengeId:0000}, error message: {e}", id, e.Message);
            return false;
        }
    }
    public async Task<IEnumerable<Challenge>?> Search(string? searchString, string? planetName, int? minReward)
    {
        try
        {
            // Start with all challenges, but don't run the query yet
            IQueryable<Challenge> query = _db.Challenges.Include(c => c.Planet);

            // Add one filter per parameter the user actually filled in

            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(c => c.Title.Contains(searchString) || c.Question.Contains(searchString));

            if (!string.IsNullOrWhiteSpace(planetName))
                query = query.Where(c => c.Planet != null && c.Planet.Name == planetName);

            if (minReward.HasValue)
                query = query.Where(c => c.CreditReward >= minReward.Value);

            // Only now is the SQL sent to the database
            return await query.OrderBy(c => c.Title).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("[ChallengeRepository] Search failed for search {Search}, planet {Planet}, minReward {MinReward}, error message: {e}",
                searchString, planetName, minReward, e.Message);
            return null;
        }
    }
}