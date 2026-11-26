
using GalacticSlicer.Api.DTOs;

namespace GalacticSlicer.Api.Services;

public interface IPlanetService
{
    Task<IEnumerable<PlanetDto>> GetAllAsync();
    Task<PlanetDto?> GetByIdAsync(int id);

    Task<PlanetDto> CreateAsync(PlanetUpsertDto dto);
    Task<bool> UpdateAsync(int id, PlanetUpsertDto dto);
    Task<bool> DeleteAsync(int id);
}
