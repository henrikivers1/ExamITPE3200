
using GalacticSlicer.Api.DAL;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Models;

namespace GalacticSlicer.Api.Services;

public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _repository;

    public CharacterService(ICharacterRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CharacterDto>> GetAllAsync()
    {
        var characters = await _repository.GetAll();

        return (characters ?? Enumerable.Empty<Character>())
            .Select(ToDto)
            .ToList();
    }

    public async Task<CharacterDto?> GetByIdAsync(int id)
    {
        var character = await _repository.GetCharacterById(id);

        return character == null ? null : ToDto(character);
    }

    public async Task<CharacterDto> CreateAsync(
        CharacterUpsertDto dto)
    {
        var character = new Character
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl
        };

        await _repository.Create(character);

        return ToDto(character);
    }

    public async Task<bool> UpdateAsync(
        int id, CharacterUpsertDto dto)
    {
        var character = await _repository.GetCharacterById(id);

        if (character == null)
        {
            return false;
        }

        character.Name = dto.Name;
        character.Description = dto.Description;
        character.Price = dto.Price;
        character.ImageUrl = dto.ImageUrl;

        return await _repository.Update(character);
    }

    public async Task<CharacterDeleteResult> DeleteAsync(int id)
    {
        var character = await _repository.GetCharacterById(id);

        if (character == null)
        {
            return CharacterDeleteResult.NotFound;
        }

        var owners = await _repository.CountOwners(id);

        if (owners > 0)
        {
            return CharacterDeleteResult.HasOwners;
        }

        var deleted = await _repository.Delete(id);

        return deleted
            ? CharacterDeleteResult.Deleted
            : CharacterDeleteResult.NotFound;
    }

    private static CharacterDto ToDto(Character c)
    {
        return new CharacterDto
        {
            CharacterId = c.CharacterId,
            Name = c.Name,
            Description = c.Description,
            Price = c.Price,
            ImageUrl = c.ImageUrl
        };
    }
}
