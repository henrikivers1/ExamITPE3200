
using GalacticSlicer.Api.DAL;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Models;

namespace GalacticSlicer.Api.Services;

public class PlanetService : IPlanetService
{
    private readonly IPlanetRepository _planetRepository;

    private static readonly HashSet<string> ProtectedPlanets =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Tatooine",
            "Naboo",
            "Coruscant",
            "Kamino",
            "Hoth",
            "Endor"
        };

    public PlanetService(IPlanetRepository planetRepository)
    {
        _planetRepository = planetRepository;
    }

    public async Task<IEnumerable<PlanetDto>> GetAllAsync()
    {
        var planets = await _planetRepository.GetAll();

        if (planets == null)
        {
            return Enumerable.Empty<PlanetDto>();
        }

        return planets
            .OrderBy(p => p.Order)
            .Select(ToDto)
            .ToList();
    }

    public async Task<PlanetDto?> GetByIdAsync(int id)
    {
        var planet = await _planetRepository.GetPlanetById(id);

        return planet == null ? null : ToDto(planet);
    }

    // ============================================
    // CREATE
    // ============================================

    public async Task<PlanetDto> CreateAsync(
        PlanetUpsertDto dto)
    {
        ValidateInput(dto);

        var name = dto.Name.Trim();

        await ValidateUniqueAsync(
            name,
            dto.Order);

        var planet = new Planet
        {
            Name = name,
            Description = CleanOptional(dto.Description),
            Order = dto.Order,
            CssClass = dto.CssClass.Trim(),
            Subject = CleanOptional(dto.Subject)
        };

        await _planetRepository.Create(planet);

        return ToDto(planet);
    }

    // ============================================
    // UPDATE
    // ============================================

    public async Task<bool> UpdateAsync(
        int id,
        PlanetUpsertDto dto)
    {
        var planet = await _planetRepository.GetPlanetById(id);

        if (planet == null)
        {
            return false;
        }

        ValidateInput(dto);

        var name = dto.Name.Trim();
        var cssClass = dto.CssClass.Trim();
        var subject = CleanOptional(dto.Subject);
        var description = CleanOptional(dto.Description);

        if (IsProtected(planet))
        {
            // Official planets are connected to missions,
            // progression and seeded questions.
            // Only their descriptions can be edited.
            if (name != planet.Name ||
                dto.Order != planet.Order ||
                cssClass != planet.CssClass ||
                subject != planet.Subject)
            {
                throw new InvalidOperationException(
                    "Official planets only allow description changes. " +
                    "Name, order, CSS class and subject are protected.");
            }

            planet.Description = description;

            return await _planetRepository.Update(planet);
        }

        await ValidateUniqueAsync(
            name,
            dto.Order,
            id);

        // Do not allow custom planets to reuse
        // the identity of an official planet.
        if (ProtectedPlanets.Contains(name))
        {
            throw new InvalidOperationException(
                "This planet name is reserved for an official planet.");
        }

        planet.Name = name;
        planet.Description = description;
        planet.Order = dto.Order;
        planet.CssClass = cssClass;
        planet.Subject = subject;

        return await _planetRepository.Update(planet);
    }

    // ============================================
    // DELETE
    // ============================================

    public async Task<bool> DeleteAsync(int id)
    {
        var planet = await _planetRepository.GetPlanetById(id);

        if (planet == null)
        {
            return false;
        }

        if (IsProtected(planet))
        {
            throw new InvalidOperationException(
                "Official planets cannot be deleted because " +
                "they are required by the game.");
        }

        return await _planetRepository.Delete(id);
    }

    // ============================================
    // VALIDATION
    // ============================================

    private static void ValidateInput(PlanetUpsertDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException(
                "Planet name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.CssClass))
        {
            throw new ArgumentException(
                "CSS class is required.");
        }

        if (dto.Name.Trim().Length > 50)
        {
            throw new ArgumentException(
                "Planet name cannot exceed 50 characters.");
        }

        if (dto.CssClass.Trim().Length > 30)
        {
            throw new ArgumentException(
                "CSS class cannot exceed 30 characters.");
        }

        if (dto.Order < 1 || dto.Order > 100)
        {
            throw new ArgumentException(
                "Chapter order must be between 1 and 100.");
        }

        if (dto.Description?.Length > 300)
        {
            throw new ArgumentException(
                "Description cannot exceed 300 characters.");
        }

        if (dto.Subject?.Length > 100)
        {
            throw new ArgumentException(
                "Subject cannot exceed 100 characters.");
        }
    }

    private async Task ValidateUniqueAsync(
        string name,
        int order,
        int? excludedPlanetId = null)
    {
        var planets = await _planetRepository.GetAll();

        if (planets == null)
        {
            return;
        }

        foreach (var planet in planets)
        {
            if (excludedPlanetId.HasValue &&
                planet.PlanetId == excludedPlanetId.Value)
            {
                continue;
            }

            if (string.Equals(
                planet.Name?.Trim(),
                name,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"A planet named '{name}' already exists.");
            }

            if (planet.Order == order)
            {
                throw new InvalidOperationException(
                    $"Chapter order {order} is already used " +
                    $"by {planet.Name}.");
            }
        }
    }

    private static bool IsProtected(Planet planet)
    {
        return ProtectedPlanets.Contains(planet.Name);
    }

    private static string? CleanOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    // ============================================
    // MAPPING
    // ============================================

    private static PlanetDto ToDto(Planet planet)
    {
        return new PlanetDto
        {
            PlanetId = planet.PlanetId,
            Name = planet.Name,
            Description = planet.Description,
            Order = planet.Order,
            CssClass = planet.CssClass,
            Subject = planet.Subject
        };
    }
}
