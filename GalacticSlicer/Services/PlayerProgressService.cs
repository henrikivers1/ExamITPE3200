
using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Services;

public class PlayerProgressService
{
    private readonly GalacticSlicerDbContext _context;
    private readonly ILogger<PlayerProgressService> _logger;

    public PlayerProgressService(
        GalacticSlicerDbContext context,
        ILogger<PlayerProgressService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PlayerProgress> GetOrCreateProgressAsync(
        string userId)
    {
        try
        {
            // Same as SELECT * FROM PlayerProgresses WHERE UserId = 'bruker-id' LIMIT 1;
            var progress = await _context.PlayerProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // If the player already has progression return the existing data
            if (progress != null)
            {
                return progress;
            }

            // If not, create a new PlayerProgress with the starting point
            progress = new PlayerProgress
            {
                UserId = userId,
                Credits = 0,
                ExperiencePoints = 0,
                Rank = "Outsider",
                ActiveCharacter = null
            };

            // Add the new player progress (Insert)
            _context.PlayerProgresses.Add(progress);
            await _context.SaveChangesAsync();

            // Log when a new progress record is created
            _logger.LogInformation(
                "New player progress created successfully");

            return progress;
        }
        catch (Exception ex)
        {
            // Log unexpected database errors with exception details
            _logger.LogError(
                ex,
                "Failed to retrieve or create player progress");

            // Discard unsaved EF Core changes after a failure
            _context.ChangeTracker.Clear();

            // Propagate the failure instead of returning invalid progress
            throw;
        }
    }
}
