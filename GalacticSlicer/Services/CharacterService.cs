
using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;


namespace GalacticSlicer.Services;

public class CharacterService
{
    private readonly GalacticSlicerDbContext _context;
    private readonly ILogger<CharacterService> _logger;

    public CharacterService(
        GalacticSlicerDbContext context,
        ILogger<CharacterService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> BuyCharacterAsync(
        string userId, int characterId)
    {
        try
        {
            // 1. Find the character
            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.CharacterId == characterId);

            if (character == null)
            {
                _logger.LogWarning(
                    "Purchase rejected: Character {CharacterId} does not exist",
                    characterId);

                return false;
            }

            // 2. Check if the player already owns the character
            bool alreadyOwned = await _context.UserCharacters
                .AnyAsync(uc => uc.UserId == userId &&
                                uc.CharacterId == characterId);

            if (alreadyOwned)
            {
                _logger.LogWarning(
                    "Purchase rejected: Character {CharacterId} is already owned",
                    characterId);

                return false;
            }

            // 3. Get the player's Credits
            var progress = await _context.PlayerProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (progress == null)
            {
                _logger.LogWarning(
                    "Purchase rejected: Player progress not found");

                return false;
            }

            if (progress.Credits < character.Price)
            {
                _logger.LogWarning(
                    "Purchase rejected: Insufficient Credits for Character {CharacterId}",
                    characterId);

                return false;
            }

            // 4. Deduct Credits
            progress.Credits -= character.Price;

            // 5. Add the character to the player's collection
            _context.UserCharacters.Add(new UserCharacter
            {
                UserId = userId,
                CharacterId = characterId,
                PurchasedAt = DateTime.UtcNow
            });

            // 6. Save the purchase and updated balance together
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Character {CharacterId} purchased successfully",
                characterId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while purchasing Character {CharacterId}",
                characterId);

            // Discard tracked changes from a failed purchase
            _context.ChangeTracker.Clear();

            return false;
        }
    }

    public async Task<bool> EquipCharacterAsync(
        string userId, int characterId)
    {
        try
        {
            // Check if the player owns the character
            bool ownsCharacter = await _context.UserCharacters
                .AnyAsync(uc => uc.UserId == userId &&
                                uc.CharacterId == characterId);

            if (!ownsCharacter)
            {
                _logger.LogWarning(
                    "Equip rejected: Character {CharacterId} is not owned",
                    characterId);

                return false;
            }

            // Find player progress
            var progress = await _context.PlayerProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (progress == null)
            {
                _logger.LogWarning(
                    "Equip rejected: Player progress not found");

                return false;
            }

            // Set active character
            progress.ActiveCharacterId = characterId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Character {CharacterId} equipped successfully",
                characterId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while equipping Character {CharacterId}",
                characterId);

            _context.ChangeTracker.Clear();

            return false;
        }
    }

    public async Task<bool> UnequipCharacterAsync(string userId)
    {
        try
        {
            // Find player progress
            var progress = await _context.PlayerProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (progress == null)
            {
                _logger.LogWarning(
                    "Unequip rejected: Player progress not found");

                return false;
            }

            // No active character means no badge is shown
            progress.ActiveCharacterId = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Character unequipped successfully");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while unequipping character");

            _context.ChangeTracker.Clear();

            return false;
        }
    }

    public async Task<List<int>> GetOwnedCharacterIdsAsync(string userId)
    {
        try
        {
            // Retrieve the IDs of all characters owned by the player
            return await _context.UserCharacters
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CharacterId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve owned characters");

            // Let the controller handle the database failure
            throw;
        }
    }
}
