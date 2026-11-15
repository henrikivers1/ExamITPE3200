using GalacticSlicer.Api.Models;

namespace GalacticSlicer.Api.DAL;

public interface IChallengeRepository
{
    Task<IEnumerable<Challenge>?> GetAll();
    Task<Challenge?> GetChallengeById(int id);
    Task<bool> Create(Challenge challenge);
    Task<bool> Update(Challenge challenge);
    Task<bool> Delete(int id);
    Task<IEnumerable<Challenge>?> Search(string? searchString, string? subject, string createdByUserId);
}
