using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GalacticSlicer.Api.Controllers;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Models;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GalacticSlicer.Api.Tests;

public class ProgressionTests
{
    [Fact]
    public async Task NewPlayer_OnlyTatooineIsUnlocked()
    {
        await using var database = await TestDatabase.CreateAsync();

        var result = await database.CreateProgressController()
            .GetMyProgress();

        var progress = ProgressFrom(result);

        Assert.True(progress.Planets.Single(p => p.Order == 1).IsUnlocked);
        Assert.All(
            progress.Planets.Where(p => p.Order > 1),
            planet => Assert.False(planet.IsUnlocked));
    }

    [Fact]
    public async Task CompletedTatooine_UnlocksNaboo()
    {
        await using var database = await TestDatabase.CreateAsync(
            experiencePoints: ChallengeCompletionService.XpPerMission);

        await database.CompleteMissionAsync(planetOrder: 1);

        var result = await database.CreateProgressController()
            .GetMyProgress();

        var progress = ProgressFrom(result);
        Assert.True(progress.Planets.Single(p => p.Order == 2).IsUnlocked);
    }

    [Fact]
    public async Task HighXpWithoutCompletedNaboo_DoesNotUnlockCoruscant()
    {
        await using var database = await TestDatabase.CreateAsync(
            experiencePoints: 100_000);

        await database.CompleteMissionAsync(planetOrder: 1);

        var result = await database.CreateProgressController()
            .GetMyProgress();

        var progress = ProgressFrom(result);
        Assert.False(progress.Planets.Single(p => p.Order == 3).IsUnlocked);
    }

    [Fact]
    public async Task CompletedNaboo_UnlocksCoruscant()
    {
        await using var database = await TestDatabase.CreateAsync(
            experiencePoints: 2 * ChallengeCompletionService.XpPerMission);

        await database.CompleteMissionAsync(planetOrder: 1);
        await database.CompleteMissionAsync(planetOrder: 2);

        var result = await database.CreateProgressController()
            .GetMyProgress();

        var progress = ProgressFrom(result);
        Assert.True(progress.Planets.Single(p => p.Order == 3).IsUnlocked);
    }

    [Fact]
    public async Task OutOfOrderLegacyCompletion_DoesNotBypassMissingPlanet()
    {
        await using var database = await TestDatabase.CreateAsync(
            experiencePoints: 100_000);

        await database.CompleteMissionAsync(planetOrder: 1);
        await database.CompleteMissionAsync(planetOrder: 3);

        var planets = await database.Context.Planets
            .OrderBy(p => p.Order)
            .ToListAsync();
        var kamino = planets.Single(p => p.Order == 4);

        var completedIds = await database.Context.CompletedMissions
            .Select(m => m.PlanetId)
            .ToHashSetAsync();

        bool unlocked = new PlanetProgressService().IsPlanetUnlocked(
            kamino,
            await database.ProgressAsync(),
            planets,
            completedIds);

        Assert.False(unlocked);
    }

    [Fact]
    public async Task LockedMission_CannotBeFetchedSubmittedOrRewarded()
    {
        const int startingXp = 100_000;
        const int startingCredits = 25;

        await using var database = await TestDatabase.CreateAsync(
            startingXp,
            startingCredits);

        await database.CompleteMissionAsync(planetOrder: 1);

        var coruscant = await database.PlanetByOrderAsync(3);
        var answers = await database.AnswersForPlanetAsync(
            coruscant.PlanetId,
            correctAnswers: 10);

        var controller = database.CreateMissionsController();

        var getResult = await controller.GetMission(coruscant.PlanetId);
        var getForbidden = Assert.IsType<ObjectResult>(getResult);
        Assert.Equal(StatusCodes.Status403Forbidden, getForbidden.StatusCode);

        var submitResult = await controller.SubmitMission(
            coruscant.PlanetId,
            new MissionSubmitDto { Answers = answers });
        var submitForbidden = Assert.IsType<ObjectResult>(submitResult);
        Assert.Equal(
            StatusCodes.Status403Forbidden,
            submitForbidden.StatusCode);

        bool rewarded = await database.CompletionService
            .CompletePassedMissionAsync(
                TestDatabase.UserId,
                coruscant.PlanetId,
                answers);

        Assert.False(rewarded);

        database.Context.ChangeTracker.Clear();
        var progress = await database.ProgressAsync();
        Assert.Equal(startingXp, progress.ExperiencePoints);
        Assert.Equal(startingCredits, progress.Credits);
    }

    [Fact]
    public async Task PassedMission_AwardsFixedXpAndChapterCredits()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tatooine = await database.PlanetByOrderAsync(1);
        var answers = await database.AnswersForPlanetAsync(
            tatooine.PlanetId,
            correctAnswers: 8);

        var result = await database.CreateMissionsController()
            .SubmitMission(
                tatooine.PlanetId,
                new MissionSubmitDto { Answers = answers });

        Assert.True(PassedFrom(result));

        database.Context.ChangeTracker.Clear();
        var progress = await database.ProgressAsync();
        Assert.Equal(
            ChallengeCompletionService.XpPerMission,
            progress.ExperiencePoints);
        Assert.Equal(
            ChallengeCompletionService.CreditsForMission(1),
            progress.Credits);
        Assert.Equal("Elite Slicer", progress.Rank);
        Assert.Equal(
            8,
            await database.Context.CompletedChallenges.CountAsync());
        Assert.Single(await database.Context.CompletedMissions.ToListAsync());
    }

    [Fact]
    public async Task FailedMission_AwardsNothing()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tatooine = await database.PlanetByOrderAsync(1);
        var answers = await database.AnswersForPlanetAsync(
            tatooine.PlanetId,
            correctAnswers: 7);

        var result = await database.CreateMissionsController()
            .SubmitMission(
                tatooine.PlanetId,
                new MissionSubmitDto { Answers = answers });

        Assert.False(PassedFrom(result));

        database.Context.ChangeTracker.Clear();
        var progress = await database.ProgressAsync();
        Assert.Equal(0, progress.ExperiencePoints);
        Assert.Equal(0, progress.Credits);
        Assert.Empty(await database.Context.CompletedChallenges.ToListAsync());
        Assert.Empty(await database.Context.CompletedMissions.ToListAsync());
    }

    [Fact]
    public async Task CompletedMission_CannotAwardRewardsTwice()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tatooine = await database.PlanetByOrderAsync(1);
        var answers = await database.AnswersForPlanetAsync(
            tatooine.PlanetId,
            correctAnswers: 10);
        var controller = database.CreateMissionsController();

        var firstResult = await controller.SubmitMission(
            tatooine.PlanetId,
            new MissionSubmitDto { Answers = answers });
        var secondResult = await controller.SubmitMission(
            tatooine.PlanetId,
            new MissionSubmitDto { Answers = answers });

        Assert.True(PassedFrom(firstResult));
        Assert.IsType<ConflictObjectResult>(secondResult);

        database.Context.ChangeTracker.Clear();
        var progress = await database.ProgressAsync();
        Assert.Equal(
            ChallengeCompletionService.XpPerMission,
            progress.ExperiencePoints);
        Assert.Equal(
            ChallengeCompletionService.CreditsForMission(1),
            progress.Credits);
        Assert.Single(await database.Context.CompletedMissions.ToListAsync());
    }

    [Fact]
    public async Task ReadingProgress_PreservesExistingPlayerData()
    {
        const int existingXp = 12_345;
        const int existingCredits = 6_789;

        await using var database = await TestDatabase.CreateAsync(
            existingXp,
            existingCredits);

        await database.CompleteMissionAsync(planetOrder: 1);

        var result = await database.CreateProgressController()
            .GetMyProgress();
        var response = ProgressFrom(result);

        database.Context.ChangeTracker.Clear();
        var stored = await database.ProgressAsync();

        Assert.Equal(existingXp, response.ExperiencePoints);
        Assert.Equal(existingCredits, response.Credits);
        Assert.Equal(existingXp, stored.ExperiencePoints);
        Assert.Equal(existingCredits, stored.Credits);
        Assert.True(await database.Context.CompletedMissions.AnyAsync(
            m => m.UserId == TestDatabase.UserId));
    }

    [Fact]
    public async Task GetMission_ReturnsTenQuestions()
    {
        await using var database = await TestDatabase.CreateAsync();
        var tatooine = await database.PlanetByOrderAsync(1);

        var result = await database.CreateMissionsController()
            .GetMission(tatooine.PlanetId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var questions = Assert.IsAssignableFrom<IEnumerable<ChallengeDto>>(
            ok.Value);
        Assert.Equal(
            ChallengeCompletionService.QuestionsPerMission,
            questions.Count());
    }

    [Fact]
    public async Task Endor_NewPlayer_IsLockedForGetAndSubmit()
    {
        await using var database = await TestDatabase.CreateAsync();
        var endor = await database.PlanetByOrderAsync(6);
        var controller = database.CreateMissionsController();

        var getResult = await controller.GetMission(endor.PlanetId);
        var getForbidden = Assert.IsType<ObjectResult>(getResult);
        Assert.Equal(StatusCodes.Status403Forbidden, getForbidden.StatusCode);

        var submitResult = await controller.SubmitMission(
            endor.PlanetId,
            new MissionSubmitDto());
        var submitForbidden = Assert.IsType<ObjectResult>(submitResult);
        Assert.Equal(
            StatusCodes.Status403Forbidden,
            submitForbidden.StatusCode);
    }

    [Fact]
    public async Task Endor_MissingOnePreviousMission_IsLocked()
    {
        await using var database = await TestDatabase.CreateAsync(
            experiencePoints: 10_000);
        await database.CompleteMissionsAsync(1, 2, 3, 4);
        var endor = await database.PlanetByOrderAsync(6);

        var result = await database.CreateMissionsController()
            .GetMission(endor.PlanetId);

        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task Endor_InsufficientXp_IsLocked()
    {
        await using var database = await TestDatabase.CreateAsync(
            experiencePoints: 5_999);
        await database.CompleteMissionsAsync(1, 2, 3, 4, 5);
        var endor = await database.PlanetByOrderAsync(6);

        var result = await database.CreateMissionsController()
            .GetMission(endor.PlanetId);

        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task Endor_AllRequirementsMet_IsUnlocked()
    {
        await using var database = await TestDatabase.CreateAsync(
            experiencePoints: 6_000);
        await database.CompleteMissionsAsync(1, 2, 3, 4, 5);

        var result = await database.CreateProgressController()
            .GetMyProgress();
        var progress = ProgressFrom(result);

        var endor = progress.Planets.Single(p => p.Order == 6);
        Assert.True(endor.IsUnlocked);
        Assert.False(endor.IsCompleted);
    }

    [Fact]
    public async Task Endor_Get_ReturnsBalancedFifteenWithoutAnswers()
    {
        await using var database = await TestDatabase.CreateAsync(
            experiencePoints: 6_000);
        await database.CompleteMissionsAsync(1, 2, 3, 4, 5);
        var endor = await database.PlanetByOrderAsync(6);

        var result = await database.CreateMissionsController()
            .GetMission(endor.PlanetId);
        var questions = MissionQuestionsFrom(result);

        Assert.Equal(
            ChallengeCompletionService.QuestionsPerFinalMission,
            questions.Count);
        Assert.Equal(
            ChallengeCompletionService.FinalMissionSubjectCount,
            questions.Select(q => q.Subject).Distinct().Count());
        Assert.All(
            questions.GroupBy(q => q.Subject),
            group => Assert.Equal(
                ChallengeCompletionService.QuestionsPerFinalSubject,
                group.Count()));
        Assert.All(questions, question =>
        {
            Assert.Null(question.CorrectAnswer);
            Assert.Null(question.Explanation);
        });
    }

    [Fact]
    public async Task Endor_ElevenCorrect_AwardsNothing()
    {
        const int startingXp = 6_000;
        const int startingCredits = 100;

        await using var database = await TestDatabase.CreateAsync(
            startingXp,
            startingCredits);
        await database.CompleteMissionsAsync(1, 2, 3, 4, 5);
        var endor = await database.PlanetByOrderAsync(6);
        var controller = database.CreateMissionsController();
        var questions = MissionQuestionsFrom(
            await controller.GetMission(endor.PlanetId));

        var result = await controller.SubmitMission(
            endor.PlanetId,
            new MissionSubmitDto
            {
                Answers = AnswersFor(questions, correctAnswers: 11)
            });

        Assert.False(PassedFrom(result));

        database.Context.ChangeTracker.Clear();
        var progress = await database.ProgressAsync();
        Assert.Equal(startingXp, progress.ExperiencePoints);
        Assert.Equal(startingCredits, progress.Credits);
        Assert.False(await database.Context.CompletedMissions.AnyAsync(
            m => m.PlanetId == endor.PlanetId));
    }

    [Fact]
    public async Task Endor_TwelveCorrect_AwardsFinalRewardsAndCompletes()
    {
        const int startingXp = 6_000;
        const int startingCredits = 100;

        await using var database = await TestDatabase.CreateAsync(
            startingXp,
            startingCredits);
        await database.CompleteMissionsAsync(1, 2, 3, 4, 5);
        var endor = await database.PlanetByOrderAsync(6);
        var controller = database.CreateMissionsController();
        var questions = MissionQuestionsFrom(
            await controller.GetMission(endor.PlanetId));

        var result = await controller.SubmitMission(
            endor.PlanetId,
            new MissionSubmitDto
            {
                Answers = AnswersFor(questions, correctAnswers: 12)
            });

        Assert.True(PassedFrom(result));

        database.Context.ChangeTracker.Clear();
        var progress = await database.ProgressAsync();
        Assert.Equal(
            startingXp + ChallengeCompletionService.XpPerMission,
            progress.ExperiencePoints);
        Assert.Equal(startingCredits + 3_000, progress.Credits);
        Assert.Equal("System Architect", progress.Rank);
        Assert.True(await database.Context.CompletedMissions.AnyAsync(
            m => m.PlanetId == endor.PlanetId));

        var progressResult = await database.CreateProgressController()
            .GetMyProgress();
        var progressResponse = ProgressFrom(progressResult);
        Assert.True(progressResponse.Planets
            .Single(p => p.Order == 6)
            .IsCompleted);
    }

    [Fact]
    public async Task Endor_RepeatedSubmission_DoesNotAwardTwice()
    {
        await using var database = await TestDatabase.CreateAsync(
            experiencePoints: 6_000);
        await database.CompleteMissionsAsync(1, 2, 3, 4, 5);
        var endor = await database.PlanetByOrderAsync(6);
        var controller = database.CreateMissionsController();
        var questions = MissionQuestionsFrom(
            await controller.GetMission(endor.PlanetId));
        var submission = new MissionSubmitDto
        {
            Answers = AnswersFor(questions, correctAnswers: 15)
        };

        var firstResult = await controller.SubmitMission(
            endor.PlanetId,
            submission);
        var secondResult = await controller.SubmitMission(
            endor.PlanetId,
            submission);

        Assert.True(PassedFrom(firstResult));
        Assert.IsType<ConflictObjectResult>(secondResult);

        database.Context.ChangeTracker.Clear();
        var progress = await database.ProgressAsync();
        Assert.Equal(8_000, progress.ExperiencePoints);
        Assert.Equal(3_000, progress.Credits);
        Assert.Equal(
            1,
            await database.Context.CompletedMissions.CountAsync(
                m => m.PlanetId == endor.PlanetId));
    }

    private static PlayerProgressDto ProgressFrom(IActionResult result)
    {
        var ok = Assert.IsType<OkObjectResult>(result);
        return Assert.IsType<PlayerProgressDto>(ok.Value);
    }

    private static bool PassedFrom(IActionResult result)
    {
        var ok = Assert.IsType<OkObjectResult>(result);
        var property = ok.Value?
            .GetType()
            .GetProperty("passed");

        return Assert.IsType<bool>(property?.GetValue(ok.Value));
    }

    private static List<ChallengeDto> MissionQuestionsFrom(
        IActionResult result)
    {
        var ok = Assert.IsType<OkObjectResult>(result);
        return Assert.IsAssignableFrom<IEnumerable<ChallengeDto>>(
                ok.Value)
            .ToList();
    }

    private static Dictionary<int, string> AnswersFor(
        IReadOnlyList<ChallengeDto> questions,
        int correctAnswers)
    {
        return questions
            .Select((question, index) => new
            {
                question.ChallengeId,
                Answer = index < correctAnswers ? "A" : "B"
            })
            .ToDictionary(
                item => item.ChallengeId,
                item => item.Answer);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        public const string UserId = "progression-test-user";

        private readonly SqliteConnection _connection;
        private readonly ServiceProvider _provider;
        private readonly IServiceScope _scope;

        private TestDatabase(
            SqliteConnection connection,
            ServiceProvider provider,
            IServiceScope scope)
        {
            _connection = connection;
            _provider = provider;
            _scope = scope;
        }

        public GalacticSlicerDbContext Context =>
            _scope.ServiceProvider
                .GetRequiredService<GalacticSlicerDbContext>();

        public ChallengeCompletionService CompletionService =>
            _scope.ServiceProvider
                .GetRequiredService<ChallengeCompletionService>();

        public static async Task<TestDatabase> CreateAsync(
            int experiencePoints = 0,
            int credits = 0)
        {
            var connection = new SqliteConnection(
                "DataSource=:memory:");
            await connection.OpenAsync();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<GalacticSlicerDbContext>(options =>
                options.UseSqlite(connection));
            services
                .AddIdentityCore<IdentityUser>()
                .AddEntityFrameworkStores<GalacticSlicerDbContext>();
            services.AddScoped<PlayerProgressService>();
            services.AddScoped<PlanetProgressService>();
            services.AddScoped<RankService>();
            services.AddScoped<ChallengeCompletionService>();

            var provider = services.BuildServiceProvider();
            var scope = provider.CreateScope();
            var database = new TestDatabase(
                connection,
                provider,
                scope);

            await database.Context.Database.EnsureCreatedAsync();
            await database.SeedAsync(experiencePoints, credits);

            return database;
        }

        public MissionsController CreateMissionsController()
        {
            var controller = new MissionsController(
                Context,
                CompletionService,
                _scope.ServiceProvider
                    .GetRequiredService<PlayerProgressService>(),
                _scope.ServiceProvider
                    .GetRequiredService<PlanetProgressService>());

            SetAuthenticatedUser(controller);
            return controller;
        }

        public ProgressController CreateProgressController()
        {
            var controller = new ProgressController(
                _scope.ServiceProvider
                    .GetRequiredService<PlayerProgressService>(),
                Context,
                _scope.ServiceProvider
                    .GetRequiredService<UserManager<IdentityUser>>(),
                _scope.ServiceProvider
                    .GetRequiredService<RankService>(),
                _scope.ServiceProvider
                    .GetRequiredService<PlanetProgressService>(),
                CompletionService);

            SetAuthenticatedUser(controller);
            return controller;
        }

        public async Task<Planet> PlanetByOrderAsync(int order)
        {
            return await Context.Planets.SingleAsync(
                p => p.Order == order);
        }

        public async Task<PlayerProgress> ProgressAsync()
        {
            return await Context.PlayerProgresses.SingleAsync(
                p => p.UserId == UserId);
        }

        public async Task CompleteMissionAsync(int planetOrder)
        {
            var planet = await PlanetByOrderAsync(planetOrder);

            Context.CompletedMissions.Add(new CompletedMission
            {
                UserId = UserId,
                PlanetId = planet.PlanetId,
                CompletedAt = DateTime.UtcNow
            });

            await Context.SaveChangesAsync();
        }

        public async Task CompleteMissionsAsync(
            params int[] planetOrders)
        {
            foreach (int planetOrder in planetOrders)
            {
                await CompleteMissionAsync(planetOrder);
            }
        }

        public async Task<Dictionary<int, string>> AnswersForPlanetAsync(
            int planetId,
            int correctAnswers)
        {
            var ids = await Context.Challenges
                .Where(c => c.PlanetId == planetId)
                .OrderBy(c => c.ChallengeId)
                .Take(ChallengeCompletionService.QuestionsPerMission)
                .Select(c => c.ChallengeId)
                .ToListAsync();

            return ids
                .Select((id, index) => new
                {
                    Id = id,
                    Answer = index < correctAnswers ? "A" : "B"
                })
                .ToDictionary(item => item.Id, item => item.Answer);
        }

        public async ValueTask DisposeAsync()
        {
            _scope.Dispose();
            await _provider.DisposeAsync();
            await _connection.DisposeAsync();
        }

        private async Task SeedAsync(
            int experiencePoints,
            int credits)
        {
            Context.Users.Add(new IdentityUser
            {
                Id = UserId,
                UserName = "ProgressionPlayer",
                NormalizedUserName = "PROGRESSIONPLAYER"
            });

            Context.PlayerProgresses.Add(new PlayerProgress
            {
                UserId = UserId,
                Credits = credits,
                ExperiencePoints = experiencePoints,
                Rank = new RankService().GetRank(experiencePoints)
            });

            var planets = new[]
            {
                new Planet
                {
                    Name = "Tatooine",
                    Order = 1,
                    CssClass = "tatooine",
                    Subject = "Linux Command Line"
                },
                new Planet
                {
                    Name = "Naboo",
                    Order = 2,
                    CssClass = "naboo",
                    Subject = "Network Security"
                },
                new Planet
                {
                    Name = "Coruscant",
                    Order = 3,
                    CssClass = "coruscant",
                    Subject = "Software Security"
                },
                new Planet
                {
                    Name = "Kamino",
                    Order = 4,
                    CssClass = "kamino",
                    Subject = "Access Control & Authentication"
                },
                new Planet
                {
                    Name = "Hoth",
                    Order = 5,
                    CssClass = "hoth",
                    Subject = "Cryptography Basics"
                },
                new Planet
                {
                    Name = "Endor",
                    Order = 6,
                    CssClass = "endor",
                    Subject = null
                }
            };

            Context.Planets.AddRange(planets);
            await Context.SaveChangesAsync();

            foreach (var planet in planets.Where(p => p.Subject != null))
            {
                for (int number = 1; number <= 12; number++)
                {
                    Context.Challenges.Add(new Challenge
                    {
                        Title = $"Question {number}",
                        Question = $"{planet.Name} question {number}?",
                        OptionA = "A",
                        OptionB = "B",
                        OptionC = "C",
                        OptionD = "D",
                        CorrectAnswer = "A",
                        Subject = planet.Subject,
                        PlanetId = planet.PlanetId,
                        SourceId = $"{planet.Name}-{number}",
                        CreditReward = 100
                    });
                }
            }

            await Context.SaveChangesAsync();
        }

        private static void SetAuthenticatedUser(
            ControllerBase controller)
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        JwtRegisteredClaimNames.Sub,
                        UserId)
                },
                authenticationType: "Test");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };
        }
    }
}
