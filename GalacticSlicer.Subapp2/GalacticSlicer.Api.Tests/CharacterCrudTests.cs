using GalacticSlicer.Api.Controllers;
using GalacticSlicer.Api.DAL;
using GalacticSlicer.Api.DTOs;
using GalacticSlicer.Api.Models;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GalacticSlicer.Api.Tests;

public class CharacterCrudTests
{
    [Fact]
    public async Task Create_ValidCharacter_TrimsNameAndReturnsCreated()
    {
        var repository = Substitute.For<ICharacterRepository>();
        repository.NameExists("Custom Pilot", null).Returns(false);
        repository.Create(Arg.Any<Character>()).Returns(true);
        var controller = CreateController(repository);

        var result = await controller.Create(new CharacterUpsertDto
        {
            Name = "  Custom Pilot  ",
            Price = 250
        });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var character = Assert.IsType<CharacterDto>(created.Value);
        Assert.Equal("Custom Pilot", character.Name);
        await repository.Received(1).Create(
            Arg.Is<Character>(c => c.Name == "Custom Pilot"));
    }

    [Fact]
    public async Task Create_DuplicateNameIgnoringCase_ReturnsConflict()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using (connection)
        {
            var options =
                new DbContextOptionsBuilder<GalacticSlicerDbContext>()
                    .UseSqlite(connection)
                    .Options;

            await using var context =
                new GalacticSlicerDbContext(options);

            await context.Database.EnsureCreatedAsync();
            context.Characters.Add(new Character
            {
                Name = "Smuggler",
                Price = 300
            });
            await context.SaveChangesAsync();

            var repository = new CharacterRepository(
                context,
                NullLogger<CharacterRepository>.Instance);

            var controller = CreateController(repository);

            var result = await controller.Create(
                new CharacterUpsertDto
                {
                    Name = "  smuggler  ",
                    Price = 500
                });

            var conflict =
                Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Contains("already exists", MessageFrom(conflict));
            Assert.Single(await context.Characters.ToListAsync());
        }
    }

    [Fact]
    public async Task Update_CustomCharacter_UpdatesAndReturnsNoContent()
    {
        var repository = Substitute.For<ICharacterRepository>();
        var existing = new Character
        {
            CharacterId = 10,
            Name = "Custom Pilot",
            Price = 100
        };

        repository.GetCharacterById(10).Returns(existing);
        repository.NameExists("Renamed Pilot", 10).Returns(false);
        repository.Update(existing).Returns(true);
        var controller = CreateController(repository);

        var result = await controller.Update(10, new CharacterUpsertDto
        {
            Name = " Renamed Pilot ",
            Price = 350,
            Description = "Updated"
        });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Renamed Pilot", existing.Name);
        Assert.Equal(350, existing.Price);
        Assert.Equal("Updated", existing.Description);
    }

    [Fact]
    public async Task Update_StandardCharacterName_ReturnsConflict()
    {
        var repository = Substitute.For<ICharacterRepository>();
        var existing = new Character
        {
            CharacterId = 1,
            Name = "Rookie Slicer",
            Price = 0
        };

        repository.GetCharacterById(1).Returns(existing);
        var controller = CreateController(repository);

        var result = await controller.Update(1, new CharacterUpsertDto
        {
            Name = "Renamed Rookie",
            Price = 0
        });

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Contains("cannot be changed", MessageFrom(conflict));
        await repository.DidNotReceive().Update(Arg.Any<Character>());
    }

    [Fact]
    public async Task Update_StandardCharacterDetails_ReturnsNoContent()
    {
        var repository = Substitute.For<ICharacterRepository>();
        var existing = new Character
        {
            CharacterId = 1,
            Name = "Rookie Slicer",
            Price = 0
        };

        repository.GetCharacterById(1).Returns(existing);
        repository.Update(existing).Returns(true);
        var controller = CreateController(repository);

        var result = await controller.Update(1, new CharacterUpsertDto
        {
            Name = "  Rookie Slicer  ",
            Price = 75,
            Description = "Updated description",
            ImageUrl = "https://example.com/rookie.png"
        });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Rookie Slicer", existing.Name);
        Assert.Equal(75, existing.Price);
        Assert.Equal("Updated description", existing.Description);
        Assert.Equal(
            "https://example.com/rookie.png",
            existing.ImageUrl);
        await repository.Received(1).Update(existing);
    }

    [Fact]
    public async Task Delete_StandardCharacter_ReturnsConflict()
    {
        var repository = Substitute.For<ICharacterRepository>();
        repository.GetCharacterById(2).Returns(new Character
        {
            CharacterId = 2,
            Name = "Astromech Hacker"
        });
        var controller = CreateController(repository);

        var result = await controller.Delete(2);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Contains("cannot be deleted", MessageFrom(conflict));
        await repository.DidNotReceive().Delete(Arg.Any<int>());
    }

    [Fact]
    public async Task Delete_OwnedCustomCharacter_ReturnsConflict()
    {
        var repository = Substitute.For<ICharacterRepository>();
        repository.GetCharacterById(10).Returns(new Character
        {
            CharacterId = 10,
            Name = "Custom Pilot"
        });
        repository.CountOwners(10).Returns(1);
        var controller = CreateController(repository);

        var result = await controller.Delete(10);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Contains("owned by players", MessageFrom(conflict));
        await repository.DidNotReceive().Delete(Arg.Any<int>());
    }

    [Fact]
    public async Task Delete_UnownedCustomCharacter_ReturnsNoContent()
    {
        var repository = Substitute.For<ICharacterRepository>();
        repository.GetCharacterById(10).Returns(new Character
        {
            CharacterId = 10,
            Name = "Custom Pilot"
        });
        repository.CountOwners(10).Returns(0);
        repository.Delete(10).Returns(true);
        var controller = CreateController(repository);

        var result = await controller.Delete(10);

        Assert.IsType<NoContentResult>(result);
        await repository.Received(1).Delete(10);
    }

    [Fact]
    public async Task Create_NameTooShortAfterTrimming_ReturnsBadRequest()
    {
        var repository = Substitute.For<ICharacterRepository>();
        var controller = CreateController(repository);

        var result = await controller.Create(new CharacterUpsertDto
        {
            Name = " A ",
            Price = 100
        });

        var badRequest =
            Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("between 2 and 50", MessageFrom(badRequest));
        await repository.DidNotReceive().Create(Arg.Any<Character>());
    }

    private static CharactersController CreateController(
        ICharacterRepository repository)
    {
        return new CharactersController(
            new CharacterService(repository),
            NullLogger<CharactersController>.Instance);
    }

    private static string MessageFrom(ObjectResult result)
    {
        var property = result.Value?
            .GetType()
            .GetProperty("message");

        return Assert.IsType<string>(
            property?.GetValue(result.Value));
    }
}
