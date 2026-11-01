using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Services;

// Picks random questions for planet missions and practice.
// Missions only use questions.json (stored in the Challenges table with a SourceId).
// Practice also includes the logged-in player's own questions in that subject.
public class QuestionService
{
    // How many questions a mission or practice round asks
    public const int QuestionsPerRound = 10;

    private readonly GalacticSlicerDbContext _context;

    public QuestionService(GalacticSlicerDbContext context)
    {
        _context = context;
    }

    // All questions a planet's mission can ask: its subject, or every subject if it has none (Endor)
    public async Task<List<Challenge>> GetPlanetPoolAsync(Planet planet)
    {
        var query = _context.Challenges.Where(c => c.SourceId != null);

        if (planet.Subject != null)
        {
            query = query.Where(c => c.Subject == planet.Subject);
        }

        return await query.ToListAsync();
    }

    // All practice questions in one subject: questions.json plus the player's own questions
    // (userId is null for visitors, who only get questions.json)
    public async Task<List<Challenge>> GetSubjectPoolAsync(string subject, string? userId = null)
    {
        return await _context.Challenges
            .Where(c => (c.SourceId != null || (userId != null && c.CreatedByUserId == userId))
                        && c.Subject == subject)
            .ToListAsync();
    }

    // Each subject with its number of practice questions (questions.json plus the player's own),
    // in planet order (subjects without a planet last)
    public async Task<List<(string Subject, int Count, int Order)>> GetSubjectsAsync(string? userId = null)
    {
        var counts = await _context.Challenges
            .Where(c => (c.SourceId != null || (userId != null && c.CreatedByUserId == userId))
                        && c.Subject != null)
            .GroupBy(c => c.Subject!)
            .Select(g => new { Subject = g.Key, Count = g.Count() })
            .ToListAsync();

        var planetOrder = await _context.Planets
            .Where(p => p.Subject != null)
            .ToDictionaryAsync(p => p.Subject!, p => p.Order);

        return counts
            .Select(s => (s.Subject, s.Count, planetOrder.GetValueOrDefault(s.Subject, int.MaxValue)))
            .OrderBy(s => s.Item3)
            .ToList();
    }

    // A random selection of questions, different every time
    public static List<Challenge> PickRandom(List<Challenge> pool, int count = QuestionsPerRound)
    {
        return pool.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
    }

    // Challenges have no typed-in title, so use the start of the question (cut at a word, max ~90 characters)
    public static string MakeTitle(string question)
    {
        if (question.Length <= 90)
        {
            return question;
        }
        var cut = question.LastIndexOf(' ', 90);
        return question[..(cut > 0 ? cut : 90)] + "…";
    }

    // The four options in random order, so the correct answer isn't always in the same place
    public static List<string> ShuffledOptions(Challenge challenge)
    {
        return new[] { challenge.OptionA, challenge.OptionB, challenge.OptionC, challenge.OptionD }
            .OrderBy(_ => Random.Shared.Next())
            .ToList();
    }
}
