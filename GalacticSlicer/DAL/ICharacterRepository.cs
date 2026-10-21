using GalacticSlicer.Models;

namespace GalacticSlicer.DAL;

public interface ICharacterRepository
{
    Task<IEnumerable<Character>?> GetAll();
    Task<Character?> GetCharacterById(int id);
    Task<bool> Create(Character character);
    Task<bool> Update(Character character);
    Task<bool> Delete(int id);
}