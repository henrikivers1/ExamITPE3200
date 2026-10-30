
using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.DAL;

public class ChallengeRepository : IChallengeRepository
{
    private readonly GalacticSlicerDbContext _db;
    private readonly ILogger<ChallengeRepository> _logger;

    public ChallengeRepository(
        GalacticSlicerDbContext db,
        ILogger<ChallengeRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<Challenge>?> GetAll()
    {
        try
        {
            return await _db.Challenges
                .Include(c => c.Planet)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log the complete exception, including its stack trace
            _logger.LogError(
                ex,
                "Failed to retrieve all challenges");

            throw;
        }
    }

    public async Task<Challenge?> GetChallengeById(int id)
    {
        try
        {
            return await _db.Challenges
                .Include(c => c.Planet)
                .FirstOrDefaultAsync(c => c.ChallengeId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve Challenge {ChallengeId}",
                id);

            throw;
        }
    }

    public async Task<bool> Create(Challenge challenge)
    {
        try
        {
            _db.Challenges.Add(challenge);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Challenge {ChallengeId} created successfully",
                challenge.ChallengeId);

            return true;
        }
        catch (Exception ex)
        {
            // Avoid logging the Challenge object or its correct answer
            _logger.LogError(
                ex,
                "Failed to create Challenge {ChallengeId}",
                challenge.ChallengeId);

            throw;
        }
    }

    public async Task<bool> Update(Challenge challenge)
    {
        try
        {
            _db.Challenges.Update(challenge);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Challenge {ChallengeId} updated successfully",
                challenge.ChallengeId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update Challenge {ChallengeId}",
                challenge.ChallengeId);

            throw;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var challenge = await _db.Challenges.FindAsync(id);

            if (challenge == null)
            {
                // A missing challenge is an expected outcome, not a database error
                _logger.LogWarning(
                    "Challenge {ChallengeId} not found during deletion",
                    id);

                return false;
            }

            // Remove completion records linked to this challenge
            var completions = await _db.CompletedChallenges
                .Where(cc => cc.ChallengeId == id)
                .ToListAsync();

            _db.CompletedChallenges.RemoveRange(completions);

            // Delete the challenge
            _db.Challenges.Remove(challenge);

            // Save both changes together
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Challenge {ChallengeId} deleted successfully",
                id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete Challenge {ChallengeId}",
                id);

            throw;
        }
    }

    public async Task<IEnumerable<Challenge>?> Search(
        string? searchString,
        string? subject,
        string createdByUserId)
    {
        try
        {
            // Start with the user's own challenges (questions.json has no owner), but don't run the query yet
            IQueryable<Challenge> query = _db.Challenges
                .Where(c => c.SourceId == null && c.CreatedByUserId == createdByUserId);

            // Add one filter per field the user actually filled in
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(c =>
                    c.Title.Contains(searchString) ||
                    c.Question.Contains(searchString));
            }

            if (!string.IsNullOrWhiteSpace(subject))
            {
                query = query.Where(c => c.Subject == subject);
            }

            // Only now is the SQL sent to the database
            return await query
                .OrderBy(c => c.Title)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Do not log raw search input unnecessarily
            _logger.LogError(
                ex,
                "Failed to search challenges");

            throw;
        }
    }
}
