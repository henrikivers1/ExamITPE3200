using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Services;

public class PlayerProgressService
{
    private readonly GalacticSlicerDbContext _context;

    public PlayerProgressService(GalacticSlicerDbContext context)
    {
        _context = context;
    }
    
    public async Task<PlayerProgress> GetOrCreateProgressAsync(
        string userId)
    {
        // Same as SELECT * FROM PlayerProgresses WHERE UserId = 'bruker-id' LIMIT 1;
        var progress = await _context.PlayerProgresses
        .FirstOrDefaultAsync(p => p.UserId == userId);

        // If the player already has progression return the existing data
         if (progress != null)
        {
            return progress;
        }

        // If not, create a new Playerprogress with the starting point
        progress = new PlayerProgress
        {
            UserId = userId,
            Credits = 0,
            Rank = "Outsider",
            ActiveCharacter = null
        };

       // Add the new player progress (Insert)
        _context.PlayerProgresses.Add (progress);
        await _context.SaveChangesAsync();

        return progress;
    }
   
}