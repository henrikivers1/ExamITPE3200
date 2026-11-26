namespace GalacticSlicer.Services;

// Saving a passed mission and giving Credits and XP
public interface IChallengeCompletionService
{
    Task<bool> CompletePassedMissionAsync(string userId, int planetId, IReadOnlyDictionary<int, string> submittedAnswers);
    Task<HashSet<int>> GetCompletedPlanetIdsAsync(string userId);
}