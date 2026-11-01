
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

        services.AddScoped<PlayerProgressService>();
        services.AddScoped<RankService>();
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
    public async Task EightCorrectAnswers_AwardsOnlyCorrectQuestions()
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
            Assert.Equal(800, progress.Credits);
            Assert.Equal(1200, progress.ExperiencePoints);
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
            Assert.Equal(1000, progress.Credits);
            Assert.Equal(1500, progress.ExperiencePoints);
            Assert.Equal(10, completedQuestions);
            Assert.Equal(1, completedMissions);
        }
    }
}
