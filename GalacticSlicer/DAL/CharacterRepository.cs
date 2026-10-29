
using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.DAL;

public class CharacterRepository : ICharacterRepository
{
    private readonly GalacticSlicerDbContext _db;
    private readonly ILogger<CharacterRepository> _logger;

    public CharacterRepository(
        GalacticSlicerDbContext db,
        ILogger<CharacterRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<Character>?> GetAll()
    {
        try
        {
            return await _db.Characters
                .OrderBy(c => c.Price)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log the full exception, including its stack trace
            _logger.LogError(
                ex,
                "Failed to retrieve all characters");

            throw;
        }
    }

    public async Task<Character?> GetCharacterById(int id)
    {
        try
        {
            return await _db.Characters.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve Character {CharacterId}",
                id);

            throw;
        }
    }

    public async Task<bool> Create(Character character)
    {
        try
        {
            _db.Characters.Add(character);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Character {CharacterId} created successfully",
                character.CharacterId);

            return true;
        }
        catch (Exception ex)
        {
            // Log the character ID instead of the entire object
            _logger.LogError(
                ex,
                "Failed to create Character {CharacterId}",
                character.CharacterId);

            throw;
        }
    }

    public async Task<bool> Update(Character character)
    {
        try
        {
            _db.Characters.Update(character);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Character {CharacterId} updated successfully",
                character.CharacterId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update Character {CharacterId}",
                character.CharacterId);

            throw;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var character = await _db.Characters.FindAsync(id);

            if (character == null)
            {
                // A missing character is not an unexpected database error
                _logger.LogWarning(
                    "Character {CharacterId} not found during deletion",
                    id);

                return false;
            }

            // Delete the character
            _db.Characters.Remove(character);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Character {CharacterId} deleted successfully",
                id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete Character {CharacterId}",
                id);

            throw;
        }
    }
}
