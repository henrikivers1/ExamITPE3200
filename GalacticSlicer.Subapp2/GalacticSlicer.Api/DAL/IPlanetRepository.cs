using GalacticSlicer.Api.Models;

namespace GalacticSlicer.Api.DAL;

public interface IPlanetRepository
{
    Task<IEnumerable<Planet>?> GetAll();
    Task<Planet?> GetPlanetById(int id);
    Task<bool> Create(Planet planet);
    Task<bool> Update(Planet planet);
    Task<bool> Delete(int id);
}