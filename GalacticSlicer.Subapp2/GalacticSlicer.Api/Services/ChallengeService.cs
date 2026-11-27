
using System.ComponentModel.DataAnnotations;
using GalacticSlicer.Api.DAL;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Models;

namespace GalacticSlicer.Api.Services;

public class ChallengeService : IChallengeService
{
    private readonly IChallengeRepository _repository;

    public ChallengeService(IChallengeRepository repository)
    {
        _repository = repository;
    }

    // ============================================
    // GET ALL
    // ============================================

    public async Task<IEnumerable<ChallengeDto>> GetAllAsync(
        string userId)
    {
        var challenges = await _repository.GetAll();

        return (challenges ?? Enumerable.Empty<Challenge>())
            .Where(c => CanView(c, userId))
            .Select(c => ToDto(c, userId))
            .ToList();
    }

    // ============================================
    // GET BY ID
    // ============================================

    public async Task<ChallengeDto?> GetByIdAsync(
        int id,
        string userId)
    {
        var challenge = await _repository.GetChallengeById(id);

        if (challenge == null || !CanView(challenge, userId))
        {
            return null;
        }

        return ToDto(challenge, userId);
    }

    // ============================================
    // CREATE
    // ============================================

    public async Task<ChallengeDto> CreateAsync(
        ChallengeUpsertDto dto,
        string userId)
    {
        Validate(dto);

        var challenge = new Challenge();

        ApplyChanges(challenge, dto);

        // User-created questions cannot be used as
        // official mission questions or give rewards.
        challenge.CreatedByUserId = userId;
        challenge.CreditReward = 0;
        challenge.PlanetId = null;
        challenge.SourceId = null;

        await _repository.Create(challenge);

        return ToDto(challenge, userId);
    }

    // ============================================
    // UPDATE
    // ============================================

    public async Task<bool> UpdateAsync(
        int id,
        ChallengeUpsertDto dto,
        string userId)
    {
        var challenge = await _repository.GetChallengeById(id);

        if (challenge == null || !CanEdit(challenge, userId))
        {
            return false;
        }

        Validate(dto);
        ApplyChanges(challenge, dto);

        return await _repository.Update(challenge);
    }

    // ============================================
    // DELETE
    // ============================================

    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        var challenge = await _repository.GetChallengeById(id);

        if (challenge == null || !CanEdit(challenge, userId))
        {
            return false;
        }

        return await _repository.Delete(id);
    }

    // ============================================
    // SEARCH AND FILTER
    // ============================================

    public async Task<IEnumerable<ChallengeDto>> SearchAsync(
        string? searchString,
        string? subject,
        string userId)
    {
        var results = await _repository.Search(
            searchString,
            subject,
            userId);

        return (results ?? Enumerable.Empty<Challenge>())
            .Where(c => CanView(c, userId))
            .Select(c => ToDto(c, userId))
            .ToList();
    }

    // ============================================
    // OWNERSHIP RULES
    // ============================================

    private static bool CanView(
        Challenge challenge,
        string userId)
    {
        return challenge.SourceId != null ||
               challenge.CreatedByUserId == userId;
    }

    private static bool CanEdit(
        Challenge challenge,
        string userId)
    {
        return challenge.SourceId == null &&
               challenge.CreatedByUserId == userId;
    }

    // ============================================
    // VALIDATION
    // ============================================

    private static void Validate(ChallengeUpsertDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Question) ||
            string.IsNullOrWhiteSpace(dto.OptionA) ||
            string.IsNullOrWhiteSpace(dto.OptionB) ||
            string.IsNullOrWhiteSpace(dto.OptionC) ||
            string.IsNullOrWhiteSpace(dto.OptionD) ||
            string.IsNullOrWhiteSpace(dto.CorrectAnswer))
        {
            throw new ValidationException(
                "Question, options and correct answer are required.");
        }

        var options = new[]
        {
            dto.OptionA,
            dto.OptionB,
            dto.OptionC,
            dto.OptionD
        };

        if (!options.Contains(dto.CorrectAnswer))
        {
            throw new ValidationException(
                "CorrectAnswer must match one of the four options.");
        }
    }

    // ============================================
    // APPLY CHANGES
    // ============================================

    private static void ApplyChanges(
        Challenge challenge,
        ChallengeUpsertDto dto)
    {
        challenge.Question = dto.Question.Trim();

        // The server generates the title.
        challenge.Title = challenge.Question.Length > 100
            ? challenge.Question[..100]
            : challenge.Question;

        challenge.OptionA = dto.OptionA;
        challenge.OptionB = dto.OptionB;
        challenge.OptionC = dto.OptionC;
        challenge.OptionD = dto.OptionD;

        challenge.CorrectAnswer = dto.CorrectAnswer;
        challenge.Subject = dto.Subject;
        challenge.Explanation = dto.Explanation;
    }

    // ============================================
    // MAP ENTITY TO DTO
    // ============================================

    private static ChallengeDto ToDto(
        Challenge challenge,
        string userId)
    {
        bool isOwner = CanEdit(challenge, userId);

        return new ChallengeDto
        {
            ChallengeId = challenge.ChallengeId,
            Title = challenge.Title,
            Question = challenge.Question,

            OptionA = challenge.OptionA,
            OptionB = challenge.OptionB,
            OptionC = challenge.OptionC,
            OptionD = challenge.OptionD,

            Subject = challenge.Subject,
            PlanetId = challenge.PlanetId,
            CreditReward = challenge.CreditReward,

            IsOwnedByCurrentUser = isOwner,

            // Only reveal the answer and explanation
            // to the creator of a user-created challenge.
            CorrectAnswer = isOwner
                ? challenge.CorrectAnswer
                : null,

            Explanation = isOwner
                ? challenge.Explanation
                : null
        };
    }
}
