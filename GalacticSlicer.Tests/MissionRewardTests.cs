
using GalacticSlicer.Models;
using GalacticSlicer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GalacticSlicer.Tests;

public class MissionRewardTests
{
    // Create a temporary SQLite database with 10 official questions
    private static async Task<(ServiceProvider Provider, SqliteConnection Connection, int PlanetId)>
        CreateTestDatabaseAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<GalacticSlicerDbContext>(options =>
            options.UseSqlite(connection));

        services.AddScoped<IPlayerProgressService, PlayerProgressService>();
        services.AddScoped<IRankService, RankService>();
        services.AddScoped<ChallengeCompletionService>();

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<GalacticSlicerDbContext>();

        await context.Database.EnsureCreatedAsync();

        // Create a test user
        context.Users.Add(new IdentityUser
        {
            Id = "test-user",
            UserName = "TestPlayer"
        });

        // Create player progress
        context.PlayerProgresses.Add(new PlayerProgress
        {
            UserId = "test-user",
            Credits = 0,
            ExperiencePoints = 0
        });

        // Create a planet for the mission
        var planet = new Planet
        {
            Name = "Tatooine",
            Order = 1
        };

        context.Planets.Add(planet);
        await context.SaveChangesAsync();

        // Create 10 official mission questions
        for (int i = 1; i <= 10; i++)
        {
            context.Challenges.Add(new Challenge
            {
                Title = $"Question {i}",
                Question = $"Test question {i}?",
                CorrectAnswer = "A",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CreditReward = 100,
                PlanetId = planet.PlanetId,
                SourceId = $"test-mission-{i}"
            });
        }

        await context.SaveChangesAsync();

        return (provider, connection, planet.PlanetId);
    }

    // Return answers with a specified number correct
    private static async Task<Dictionary<int, string>> GetAnswersAsync(
        GalacticSlicerDbContext context,
        int planetId,
        int correctAnswers)
    {
        var ids = await context.Challenges
            .Where(c => c.PlanetId == planetId)
            .OrderBy(c => c.ChallengeId)
            .Select(c => c.ChallengeId)
            .ToListAsync();

        return ids
            .Select((id, index) => new
            {
                Id = id,
                Answer = index < correctAnswers ? "A" : "B"
            })
            .ToDictionary(x => x.Id, x => x.Answer);
    }

    [Fact]
    public void PassingEachMissionOnce_GivesEnoughXpForTheNextPlanet()
    {
        // Arrange
        var planetProgress = new PlanetProgressService();

        // Act + Assert: after passing missions 1..k (XpPerMission each), planet k+1 is unlocked.
        // This is the chain that soft-locked players before Hoth with XP per correct answer.
        for (int passedMissions = 1; passedMissions <= 5; passedMissions++)
        {
            int xp = passedMissions * ChallengeCompletionService.XpPerMission;
            int nextPlanetOrder = passedMissions + 1;

            Assert.True(xp >= planetProgress.GetRequiredXp(nextPlanetOrder),
                $"{passedMissions} passed mission(s) = {xp} XP, but chapter {nextPlanetOrder} needs " +
                $"{planetProgress.GetRequiredXp(nextPlanetOrder)} XP");
        }
    }

    [Fact]
    public void PassingEachMissionOnce_GivesEnoughCreditsForTheNextBadge()
    {
        // Arrange: the paid badges seeded in DBInit, cheapest first
        int[] badgePrices = { 500, 1000, 1500, 2000 };
        int credits = 0;

        // Act + Assert: after each passed mission (chapter 1, 2, 3, ...) the player can buy the next badge
        for (int mission = 0; mission < badgePrices.Length; mission++)
        {
            credits += ChallengeCompletionService.CreditsForMission(mission + 1);

            Assert.True(credits >= badgePrices[mission],
                $"After mission {mission + 1} the player has {credits} Credits, " +
                $"but the next badge costs {badgePrices[mission]}");

            credits -= badgePrices[mission];
        }
    }

    [Fact]
    public async Task EightCorrectAnswers_PassesAndStoresOnlyCorrectQuestions()
    {
        // Arrange
        var (provider, connection, planetId) =
            await CreateTestDatabaseAsync();

        using (provider)
        using (connection)
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<GalacticSlicerDbContext>();

            var service = scope.ServiceProvider
                .GetRequiredService<ChallengeCompletionService>();

            var answers = await GetAnswersAsync(context, planetId, 8);

            // Act
            bool result = await service.CompletePassedMissionAsync(
                "test-user", planetId, answers);

            context.ChangeTracker.Clear();

            var progress = await context.PlayerProgresses
                .SingleAsync(p => p.UserId == "test-user");

            int completedQuestions = await context.CompletedChallenges
                .CountAsync(c => c.UserId == "test-user");

            bool missionCompleted = await context.CompletedMissions
                .AnyAsync(m => m.UserId == "test-user" &&
                               m.PlanetId == planetId);

            // Assert
            Assert.True(result);
            // The rewards depend on the planet, not the score, so 8/10 gives the same as 10/10
            Assert.Equal(ChallengeCompletionService.CreditsForMission(1), progress.Credits); // test planet is chapter 1
            Assert.Equal(ChallengeCompletionService.XpPerMission, progress.ExperiencePoints);
            Assert.Equal(8, completedQuestions);
            Assert.True(missionCompleted);
        }
    }

    [Fact]
    public async Task SevenCorrectAnswers_DoesNotAwardRewards()
    {
        // Arrange
        var (provider, connection, planetId) =
            await CreateTestDatabaseAsync();

        using (provider)
        using (connection)
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<GalacticSlicerDbContext>();

            var service = scope.ServiceProvider
                .GetRequiredService<ChallengeCompletionService>();

            var answers = await GetAnswersAsync(context, planetId, 7);

            // Act
            bool result = await service.CompletePassedMissionAsync(
                "test-user", planetId, answers);

            context.ChangeTracker.Clear();

            var progress = await context.PlayerProgresses
                .SingleAsync(p => p.UserId == "test-user");

            int completedQuestions = await context.CompletedChallenges
                .CountAsync(c => c.UserId == "test-user");

            bool missionCompleted = await context.CompletedMissions
                .AnyAsync(m => m.UserId == "test-user" &&
                               m.PlanetId == planetId);

            // Assert
            Assert.False(result);
            Assert.Equal(0, progress.Credits);
            Assert.Equal(0, progress.ExperiencePoints);
            Assert.Equal(0, completedQuestions);
            Assert.False(missionCompleted);
        }
    }

    [Fact]
    public async Task CompletingSameMissionTwice_DoesNotAwardTwice()
    {
        // Arrange
        var (provider, connection, planetId) =
            await CreateTestDatabaseAsync();

        using (provider)
        using (connection)
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<GalacticSlicerDbContext>();

            var service = scope.ServiceProvider
                .GetRequiredService<ChallengeCompletionService>();

            var answers = await GetAnswersAsync(context, planetId, 10);

            // Act - complete the same mission twice
            bool firstResult = await service.CompletePassedMissionAsync(
                "test-user", planetId, answers);

            bool secondResult = await service.CompletePassedMissionAsync(
                "test-user", planetId, answers);

            context.ChangeTracker.Clear();

            var progress = await context.PlayerProgresses
                .SingleAsync(p => p.UserId == "test-user");

            int completedQuestions = await context.CompletedChallenges
                .CountAsync(c => c.UserId == "test-user");

            int completedMissions = await context.CompletedMissions
                .CountAsync(m => m.UserId == "test-user" &&
                                 m.PlanetId == planetId);

            // Assert
            Assert.True(firstResult);
            Assert.True(secondResult);
            Assert.Equal(ChallengeCompletionService.CreditsForMission(1), progress.Credits); // test planet is chapter 1
            Assert.Equal(ChallengeCompletionService.XpPerMission, progress.ExperiencePoints);
            Assert.Equal(10, completedQuestions);
            Assert.Equal(1, completedMissions);
        }
    }
}
