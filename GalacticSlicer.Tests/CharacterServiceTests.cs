
using GalacticSlicer.Models;
using GalacticSlicer.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GalacticSlicer.Tests;

public class CharacterServiceTests
{
    // Creates a separate SQLite database for each test
    private static async Task<(GalacticSlicerDbContext Context,
        SqliteConnection Connection)> CreateDatabaseAsync()
    {
        // Keep the in-memory database alive while the connection is open
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        // Configure EF Core to use the temporary database
        var options = new DbContextOptionsBuilder<GalacticSlicerDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new GalacticSlicerDbContext(options);

        // Create tables from the DbContext model
        await context.Database.EnsureCreatedAsync();

        return (context, connection);
    }

    // Positive: A player with enough Credits can purchase a character
    [Fact]
    public async Task BuyCharacter_WithEnoughCredits_Succeeds()
    {
        // Arrange
        var (context, connection) = await CreateDatabaseAsync();

        await using (connection)
        await using (context)
        {
            var user = new Microsoft.AspNetCore.Identity.IdentityUser
            {
                Id = "test-user",
                UserName = "TestPlayer"
            };

            context.Users.Add(user);

            context.PlayerProgresses.Add(new PlayerProgress
            {
                UserId = user.Id,
                Credits = 500
            });

            context.Characters.Add(new Character
            {
                CharacterId = 1,
                Name = "Astromech Hacker",
                Price = 200
            });

            await context.SaveChangesAsync();

            var service = new CharacterService(context);

            // Act
            bool result = await service.BuyCharacterAsync(user.Id, 1);

            // Assert
            Assert.True(result);
            Assert.Equal(300,
                (await context.PlayerProgresses
                    .FirstAsync(p => p.UserId == user.Id)).Credits);
        }
    }

    // Positive: A purchased character is stored in the player's collection
    [Fact]
    public async Task BuyCharacter_AfterPurchase_StoresOwnership()
    {
        // Arrange
        var (context, connection) = await CreateDatabaseAsync();

        await using (connection)
        await using (context)
        {
            var user = new Microsoft.AspNetCore.Identity.IdentityUser
            {
                Id = "test-user",
                UserName = "TestPlayer"
            };

            context.Users.Add(user);

            context.PlayerProgresses.Add(new PlayerProgress
            {
                UserId = user.Id,
                Credits = 500
            });

            context.Characters.Add(new Character
            {
                CharacterId = 1,
                Name = "Astromech Hacker",
                Price = 200
            });

            await context.SaveChangesAsync();

            var service = new CharacterService(context);

            // Act
            await service.BuyCharacterAsync(user.Id, 1);

            // Assert
            bool ownsCharacter = await context.UserCharacters.AnyAsync(
                uc => uc.UserId == user.Id && uc.CharacterId == 1);

            Assert.True(ownsCharacter);
        }
    }

    // Negative: A player cannot purchase a character without enough Credits
    [Fact]
    public async Task BuyCharacter_WithoutEnoughCredits_Fails()
    {
        // Arrange
        var (context, connection) = await CreateDatabaseAsync();

        await using (connection)
        await using (context)
        {
            var user = new Microsoft.AspNetCore.Identity.IdentityUser
            {
                Id = "test-user",
                UserName = "TestPlayer"
            };

            context.Users.Add(user);

            context.PlayerProgresses.Add(new PlayerProgress
            {
                UserId = user.Id,
                Credits = 100
            });

            context.Characters.Add(new Character
            {
                CharacterId = 1,
                Name = "Astromech Hacker",
                Price = 200
            });

            await context.SaveChangesAsync();

            var service = new CharacterService(context);

            // Act
            bool result = await service.BuyCharacterAsync(user.Id, 1);

            // Assert
            Assert.False(result);
            Assert.Equal(100,
                (await context.PlayerProgresses
                    .FirstAsync(p => p.UserId == user.Id)).Credits);

            Assert.Empty(await context.UserCharacters.ToListAsync());
        }
    }

    // Negative: The same character cannot be purchased twice
    [Fact]
    public async Task BuyCharacter_AlreadyOwned_Fails()
    {
        // Arrange
        var (context, connection) = await CreateDatabaseAsync();

        await using (connection)
        await using (context)
        {
            var user = new Microsoft.AspNetCore.Identity.IdentityUser
            {
                Id = "test-user",
                UserName = "TestPlayer"
            };

            context.Users.Add(user);

            context.PlayerProgresses.Add(new PlayerProgress
            {
                UserId = user.Id,
                Credits = 500
            });

            context.Characters.Add(new Character
            {
                CharacterId = 1,
                Name = "Astromech Hacker",
                Price = 200
            });

            await context.SaveChangesAsync();

            var service = new CharacterService(context);

            // Act
            bool firstPurchase = await service.BuyCharacterAsync(user.Id, 1);
            bool secondPurchase = await service.BuyCharacterAsync(user.Id, 1);

            // Assert
            Assert.True(firstPurchase);
            Assert.False(secondPurchase);

            Assert.Equal(300,
                (await context.PlayerProgresses
                    .FirstAsync(p => p.UserId == user.Id)).Credits);

            Assert.Single(await context.UserCharacters.ToListAsync());
        }
    }
}
