using GalacticSlicer.Models;

namespace GalacticSlicer.Services;

// Loading questions for missions and practice
public interface IQuestionService
{
    Task<List<Challenge>> GetPlanetPoolAsync(Planet planet);
    Task<List<Challenge>> GetSubjectPoolAsync(string subject, string? userId = null);
    Task<List<(string Subject, int Count, int Order)>> GetSubjectsAsync(string? userId = null);
}