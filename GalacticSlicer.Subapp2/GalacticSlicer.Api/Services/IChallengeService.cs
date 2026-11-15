
using GalacticSlicer.Api.DTOs;

namespace GalacticSlicer.Api.Services;

public interface IChallengeService
{
    Task<IEnumerable<ChallengeDto>> GetAllAsync(string userId);
    Task<ChallengeDto?> GetByIdAsync(int id, string userId);

    Task<ChallengeDto> CreateAsync(
        ChallengeUpsertDto dto, string userId);

    Task<bool> UpdateAsync(
        int id, ChallengeUpsertDto dto, string userId);

    Task<bool> DeleteAsync(int id, string userId);

    Task<IEnumerable<ChallengeDto>> SearchAsync(
        string? searchString, string? subject, string userId);
}
