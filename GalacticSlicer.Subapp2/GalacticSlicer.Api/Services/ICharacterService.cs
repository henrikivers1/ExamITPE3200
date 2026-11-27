
using GalacticSlicer.Api.DTOs;

namespace GalacticSlicer.Api.Services;

public interface ICharacterService
{
    Task<IEnumerable<CharacterDto>> GetAllAsync();
    Task<CharacterDto?> GetByIdAsync(int id);

    Task<CharacterDto> CreateAsync(CharacterUpsertDto dto);
    Task<bool> UpdateAsync(int id, CharacterUpsertDto dto);
    Task<CharacterDeleteResult> DeleteAsync(int id);
}


public enum CharacterDeleteResult
{
    Deleted,
    NotFound,
    HasOwners,
    StandardCharacter
}
