using GalacticSlicer.Models;

namespace GalacticSlicer.Services;

// Credits, XP and rank for one player
public interface IPlayerProgressService
{
    Task<PlayerProgress> GetOrCreateProgressAsync(string userId);
}