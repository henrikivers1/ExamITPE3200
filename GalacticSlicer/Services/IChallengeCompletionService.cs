using GalacticSlicer.Models;

namespace GalacticSlicer.Services;

// Saving a passed mission and giving Credits and XP, and the history of every mission attempt
public interface IChallengeCompletionService
{
    Task<bool> CompletePassedMissionAsync(string userId, int planetId, IReadOnlyDictionary<int, string> submittedAnswers);
    Task<HashSet<int>> GetCompletedPlanetIdsAsync(string userId);
    Task<bool> SaveAttemptAsync(string userId, int planetId, int correctCount, int totalQuestions, bool passed);
    Task<List<MissionAttempt>> GetRecentAttemptsAsync(string userId, int count);
}