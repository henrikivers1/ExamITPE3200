
using GalacticSlicer.Api.DAL;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Models;

namespace GalacticSlicer.Api.Services;

public class PlanetService : IPlanetService
{
    private readonly IPlanetRepository _planetRepository;

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

        return planets.Select(ToDto).ToList();
    }

    public async Task<PlanetDto?> GetByIdAsync(int id)
    {
        var planet = await _planetRepository.GetPlanetById(id);

        return planet == null ? null : ToDto(planet);
    }

    public async Task<PlanetDto> CreateAsync(PlanetUpsertDto dto)
    {
        var planet = new Planet
        {
            Name = dto.Name,
            Description = dto.Description,
            Order = dto.Order,
            CssClass = dto.CssClass,
            Subject = dto.Subject
        };

        await _planetRepository.Create(planet);

        return ToDto(planet);
    }

    public async Task<bool> UpdateAsync(int id, PlanetUpsertDto dto)
    {
        var planet = await _planetRepository.GetPlanetById(id);

        if (planet == null)
        {
            return false;
        }

        planet.Name = dto.Name;
        planet.Description = dto.Description;
        planet.Order = dto.Order;
        planet.CssClass = dto.CssClass;
        planet.Subject = dto.Subject;

        return await _planetRepository.Update(planet);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _planetRepository.Delete(id);
    }

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
