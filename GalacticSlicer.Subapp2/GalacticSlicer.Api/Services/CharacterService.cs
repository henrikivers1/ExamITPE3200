
using System.ComponentModel.DataAnnotations;
using GalacticSlicer.Api.DAL;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Models;

namespace GalacticSlicer.Api.Services;

public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _repository;

    private static readonly HashSet<string> StandardCharacterNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Rookie Slicer",
            "Astromech Hacker",
            "Smuggler",
            "Bounty Hunter",
            "Shadow Operative"
        };

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
        var name = NormalizeName(dto.Name);

        await EnsureUniqueNameAsync(name);

        var character = new Character
        {
            Name = name,
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

        var name = NormalizeName(dto.Name);

        if (IsStandardCharacter(character))
        {
            if (!string.Equals(
                character.Name.Trim(),
                name,
                StringComparison.Ordinal))
            {
                throw new CharacterConflictException(
                    "The names of standard characters cannot be changed.");
            }
        }
        else
        {
            await EnsureUniqueNameAsync(name, id);
            character.Name = name;
        }

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

        if (IsStandardCharacter(character))
        {
            return CharacterDeleteResult.StandardCharacter;
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

    private async Task EnsureUniqueNameAsync(
        string name,
        int? excludedCharacterId = null)
    {
        if (await _repository.NameExists(
            name,
            excludedCharacterId))
        {
            throw new CharacterConflictException(
                $"A character named '{name}' already exists.");
        }
    }

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(
                "Character name is required.");
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length < 2 ||
            normalizedName.Length > 50)
        {
            throw new ValidationException(
                "Character name must be between 2 and 50 characters.");
        }

        return normalizedName;
    }

    private static bool IsStandardCharacter(Character character)
    {
        return StandardCharacterNames.Contains(
            character.Name.Trim());
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
