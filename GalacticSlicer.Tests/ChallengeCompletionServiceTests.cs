
using GalacticSlicer.Models;
using GalacticSlicer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GalacticSlicer.Tests;

public class ChallengeCompletionServiceTests
{
    // Create an isolated SQLite database for each test
    private static async Task<(GalacticSlicerDbContext Context,
        SqliteConnection Connection)> CreateDatabaseAsync()
    {
        // Keep the database alive while the connection is open
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        // Configure EF Core to use the temporary SQLite database
        var options = new DbContextOptionsBuilder<GalacticSlicerDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new GalacticSlicerDbContext(options);

        // Create the database tables using the existing EF Core model
        await context.Database.EnsureCreatedAsync();

        return (context, connection);
    }

    // Add a test player and a challenge with a defined Credit reward
    private static async Task SeedDataAsync(GalacticSlicerDbContext context)
    {
        // Create a test user to satisfy the Identity foreign key
        context.Users.Add(new IdentityUser
        {
            Id = "test-user",
            UserName = "TestPlayer"
        });

        // Create a planet to satisfy the Challenge foreign key
        context.Planets.Add(new Planet
        {
            PlanetId = 1,
            Name = "Tatooine",
            Order = 1
        });

        // Create a challenge worth 200 Credits
        context.Challenges.Add(new Challenge
        {
            ChallengeId = 1,
            Title = "Password Security",
            Question = "What makes a password stronger?",
            CorrectAnswer = "A longer unique password",
            OptionA = "A longer unique password",
            OptionB = "Using your birthday",
            OptionC = "Using password123",
            OptionD = "Reusing passwords",
            CreditReward = 200,
            PlanetId = 1
        });

        await context.SaveChangesAsync();
    }

    // Positive: Completing a challenge should award the configured Credits
    [Fact]
    public async Task CompleteChallenge_ValidChallenge_AwardsCredits()
    {
        // Arrange
        var (context, connection) = await CreateDatabaseAsync();

        await using (connection)
        await using (context)
        {
            await SeedDataAsync(context);

            // Use NullLogger to avoid producing log output during testing
            var service = new ChallengeCompletionService(
                context,
                new PlayerProgressService(
                    context,
                    NullLogger<PlayerProgressService>.Instance),
                new RankService(),
                NullLogger<ChallengeCompletionService>.Instance);

            // Act
            bool result = await service.CompleteChallengeAsync("test-user", 1);

            // Assert
            Assert.True(result);

            var progress = await context.PlayerProgresses
                .SingleAsync(p => p.UserId == "test-user");

            Assert.Equal(200, progress.Credits);

            // Confirm that completion was registered
            Assert.True(await context.CompletedChallenges
                .AnyAsync(c => c.UserId == "test-user" && c.ChallengeId == 1));
        }
    }

    // Positive: Completing a challenge should award XP and update the rank
    [Fact]
    public async Task CompleteChallenge_ValidChallenge_UpdatesXpAndRank()
    {
        // Arrange
        var (context, connection) = await CreateDatabaseAsync();

        await using (connection)
        await using (context)
        {
            await SeedDataAsync(context);

            // Start close to the rank threshold
            context.PlayerProgresses.Add(new PlayerProgress
            {
                UserId = "test-user",
                Credits = 0,
                ExperiencePoints = 400,
                Rank = "Outsider"
            });

            await context.SaveChangesAsync();

            // Use NullLogger to avoid producing log output during testing
            var service = new ChallengeCompletionService(
                context,
                new PlayerProgressService(
                    context,
                    NullLogger<PlayerProgressService>.Instance),
                new RankService(),
                NullLogger<ChallengeCompletionService>.Instance);

            // Act
            bool result = await service.CompleteChallengeAsync("test-user", 1);

            // Assert
            Assert.True(result);

            var progress = await context.PlayerProgresses
                .SingleAsync(p => p.UserId == "test-user");

            Assert.Equal(550, progress.ExperiencePoints);
            Assert.Equal("Slicer", progress.Rank);
        }
    }

    // Negative: Completing the same challenge twice must not give extra rewards
    [Fact]
    public async Task CompleteChallenge_AlreadyCompleted_DoesNotRewardTwice()
    {
        // Arrange
        var (context, connection) = await CreateDatabaseAsync();

        await using (connection)
        await using (context)
        {
            await SeedDataAsync(context);

            // Use NullLogger to avoid producing log output during testing
            var service = new ChallengeCompletionService(
                context,
                new PlayerProgressService(
                    context,
                    NullLogger<PlayerProgressService>.Instance),
                new RankService(),
                NullLogger<ChallengeCompletionService>.Instance);

            // Act
            bool firstResult = await service.CompleteChallengeAsync("test-user", 1);
            bool secondResult = await service.CompleteChallengeAsync("test-user", 1);

            // Assert
            Assert.True(firstResult);
            Assert.False(secondResult);

            var progress = await context.PlayerProgresses
                .SingleAsync(p => p.UserId == "test-user");

            Assert.Equal(200, progress.Credits);
            Assert.Equal(150, progress.ExperiencePoints);

            // Only one completion record should exist
            Assert.Single(await context.CompletedChallenges.ToListAsync());
        }
    }

    // Negative: A challenge that does not exist must not grant rewards
    [Fact]
    public async Task CompleteChallenge_InvalidChallengeId_ReturnsFalse()
    {
        // Arrange
        var (context, connection) = await CreateDatabaseAsync();

        await using (connection)
        await using (context)
        {
            await SeedDataAsync(context);

            // Use NullLogger to avoid producing log output during testing
            var service = new ChallengeCompletionService(
                context,
                new PlayerProgressService(
                    context,
                    NullLogger<PlayerProgressService>.Instance),
                new RankService(),
                NullLogger<ChallengeCompletionService>.Instance);

            // Act
            bool result = await service.CompleteChallengeAsync("test-user", 999);

            // Assert
            Assert.False(result);

            // No completion should be recorded
            Assert.Empty(await context.CompletedChallenges.ToListAsync());

            // An invalid challenge should not create or reward player progress
            Assert.Empty(await context.PlayerProgresses.ToListAsync());
        }
    }
}
