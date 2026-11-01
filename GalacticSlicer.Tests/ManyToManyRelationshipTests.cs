
using GalacticSlicer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GalacticSlicer.Tests;

public class ManyToManyRelationshipTests
{
    [Fact]
    public async Task PlayersCanOwnMultipleCharacters_AndCharactersCanHaveMultipleOwners()
    {
        // Create a temporary SQLite database
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<GalacticSlicerDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new GalacticSlicerDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Create two users
        context.Users.AddRange(
            new IdentityUser { Id = "user1", UserName = "player1" },
            new IdentityUser { Id = "user2", UserName = "player2" }
        );

        // Create two player progress records
        var player1 = new PlayerProgress { UserId = "user1" };
        var player2 = new PlayerProgress { UserId = "user2" };

        context.PlayerProgresses.AddRange(player1, player2);

        // Create two characters
        var kael = new Character { Name = "Kael Voss", Price = 100 };
        var r2d2 = new Character { Name = "R2-D2", Price = 200 };

        context.Characters.AddRange(kael, r2d2);
        await context.SaveChangesAsync();

        // Player 1 owns both characters
        // Player 2 also owns Kael Voss
        context.UserCharacters.AddRange(
            new UserCharacter
            {
                UserId = "user1",
                PlayerProgressId = player1.Id,
                CharacterId = kael.CharacterId
            },
            new UserCharacter
            {
                UserId = "user1",
                PlayerProgressId = player1.Id,
                CharacterId = r2d2.CharacterId
            },
            new UserCharacter
            {
                UserId = "user2",
                PlayerProgressId = player2.Id,
                CharacterId = kael.CharacterId
            }
        );

        await context.SaveChangesAsync();

        // Reload the relationships from the database
        context.ChangeTracker.Clear();

        var loadedPlayer1 = await context.PlayerProgresses
            .Include(p => p.OwnedCharacters)
            .SingleAsync(p => p.UserId == "user1");

        var loadedKael = await context.Characters
            .Include(c => c.Owners)
            .SingleAsync(c => c.Name == "Kael Voss");

        // One player can own multiple characters
        Assert.Equal(2, loadedPlayer1.OwnedCharacters.Count);

        // One character can be owned by multiple players
        Assert.Equal(2, loadedKael.Owners.Count);
    }
}
