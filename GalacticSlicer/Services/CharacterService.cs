
using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Services;

public class CharacterService
{
    private readonly GalacticSlicerDbContext _context;

    public CharacterService(GalacticSlicerDbContext context)
    {
        _context = context;
    }

    public async Task<bool> BuyCharacterAsync(
        string userId, int characterId)
    {
        // 1. Find the character
        var character = await _context.Characters
            .FirstOrDefaultAsync(c => c.CharacterId == characterId);

        if (character == null)
            return false;

        // 2. Check if the player already owns the character
        bool alreadyOwned = await _context.UserCharacters
            .AnyAsync(uc => uc.UserId == userId &&
                            uc.CharacterId == characterId);

        if (alreadyOwned)
            return false;

        // 3. Get the player's Credits
        var progress = await _context.PlayerProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (progress == null || progress.Credits < character.Price)
            return false;

        // 4. Deduct Credits
        progress.Credits -= character.Price;

        // 5. Add the character to the player's collection
        _context.UserCharacters.Add(new UserCharacter
        {
            UserId = userId,
            CharacterId = characterId,
            PurchasedAt = DateTime.UtcNow
        });

        // 6. Save the purchase
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EquipCharacterAsync(string userId, int characterId)
    {
        // Check if the player owns the character
        bool ownsCharacter = await _context.UserCharacters
            .AnyAsync(uc => uc.UserId == userId &&
                            uc.CharacterId == characterId);

        if (!ownsCharacter)
            return false;

        // Find player progress
        var progress = await _context.PlayerProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (progress == null)
            return false;

        // Set active character
        progress.ActiveCharacterId = characterId;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<int>> GetOwnedCharacterIdsAsync(string userId)
    {
        return await _context.UserCharacters
            .Where(uc => uc.UserId == userId)
            .Select(uc => uc.CharacterId)
            .ToListAsync();
    }
}
