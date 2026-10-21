using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.DAL;

public class CharacterRepository : ICharacterRepository
{
    private readonly GalacticSlicerDbContext _db;
    private readonly ILogger<CharacterRepository> _logger;

    public CharacterRepository(GalacticSlicerDbContext db, ILogger<CharacterRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<Character>?> GetAll()
    {
        try
        {
            return await _db.Characters.OrderBy(c => c.Price).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("[CharacterRepository] characters ToListAsync() failed when GetAll(), error message: {e}", e.Message);
            return null;
        }
    }

    public async Task<Character?> GetCharacterById(int id)
    {
        try
        {
            return await _db.Characters.FindAsync(id);
        }
        catch (Exception e)
        {
            _logger.LogError("[CharacterRepository] character FindAsync(id) failed for CharacterId {CharacterId:0000}, error message: {e}", id, e.Message);
            return null;
        }
    }

    public async Task<bool> Create(Character character)
    {
        try
        {
            _db.Characters.Add(character);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[CharacterRepository] character creation failed for character {@character}, error message: {e}", character, e.Message);
            return false;
        }
    }

    public async Task<bool> Update(Character character)
    {
        try
        {
            _db.Characters.Update(character);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[CharacterRepository] character update failed for CharacterId {CharacterId:0000}, error message: {e}", character.CharacterId, e.Message);
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var character = await _db.Characters.FindAsync(id);
            if (character == null)
            {
                _logger.LogError("[CharacterRepository] character not found for CharacterId {CharacterId:0000}", id);
                return false;
            }

            _db.Characters.Remove(character);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[CharacterRepository] character deletion failed for CharacterId {CharacterId:0000}, error message: {e}", id, e.Message);
            return false;
        }
    }
}