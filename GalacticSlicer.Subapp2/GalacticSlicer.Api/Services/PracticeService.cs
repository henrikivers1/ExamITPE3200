using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Api.Services;

// Data access for PracticeController, so the controller never touches the DbContext.
// Practice uses the official questions only and gives no rewards.
public class PracticeService
{
    public const int QuestionsPerRound = 10;

    private readonly GalacticSlicerDbContext _context;

    public PracticeService(GalacticSlicerDbContext context)
    {
        _context = context;
    }

    // Up to 10 random official questions from one subject, without the answers
    public async Task<List<PracticeQuestionDto>> GetRoundAsync(string subject)
    {
        var questions = await _context.Challenges
            .AsNoTracking()
            .Where(c =>
                c.SourceId != null &&
                c.CreatedByUserId == null &&
                c.PlanetId != null &&
                c.Subject == subject)
            .Select(c => new PracticeQuestionDto
            {
                ChallengeId = c.ChallengeId,
                Question = c.Question,
                Subject = c.Subject,
                OptionA = c.OptionA,
                OptionB = c.OptionB,
                OptionC = c.OptionC,
                OptionD = c.OptionD
            })
            .ToListAsync();

        // Randomise the question order in memory.
        // SQLite does not need any special random SQL support.
        Random.Shared.Shuffle(
            System.Runtime.InteropServices.CollectionsMarshal
                .AsSpan(questions));

        return questions
            .Take(QuestionsPerRound)
            .ToList();
    }

    // Checks one answer against the correct answer stored in SQLite.
    // null when the question doesn't exist or isn't an official one.
    public async Task<PracticeAnswerResultDto?> CheckAnswerAsync(
        int challengeId,
        string answer)
    {
        var challenge = await _context.Challenges
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.ChallengeId == challengeId &&
                c.SourceId != null &&
                c.CreatedByUserId == null &&
                c.PlanetId != null);

        if (challenge == null)
        {
            return null;
        }

        return new PracticeAnswerResultDto
        {
            IsCorrect = string.Equals(
                answer.Trim(),
                challenge.CorrectAnswer.Trim(),
                StringComparison.OrdinalIgnoreCase),
            CorrectAnswer = challenge.CorrectAnswer,
            Explanation = challenge.Explanation
        };
    }
}
