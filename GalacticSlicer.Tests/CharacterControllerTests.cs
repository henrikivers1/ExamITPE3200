using GalacticSlicer.Controllers;
using GalacticSlicer.DAL;
using GalacticSlicer.Models;
using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GalacticSlicer.Tests;

// Unit tests for CharacterController (Manage / Create / Edit / Delete).
// Same pattern as the course's ItemController tests: the repository is faked with NSubstitute (Substitute.For),
// so no database is used and only the controller's own logic is tested.
public class CharacterControllerTests
{
    // Two characters: one free starter and one that costs Credits
    private static List<Character> SampleCharacters() => new()
    {
        new Character { CharacterId = 1, Name = "Rookie Slicer", Price = 0 },
        new Character { CharacterId = 2, Name = "Smuggler", Price = 300 }
    };

    // The services and UserManager are not used by the CRUD actions, so they are null here.
    // TempData is needed because the actions set success/error messages.
    private static CharacterController CreateController(ICharacterRepository repo)
    {
        var controller = new CharacterController(
            repo,
            Substitute.For<ILogger<CharacterController>>(),
            null!,   // CharacterService
            null!,   // UserManager
            null!);  // PlayerProgressService

        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Substitute.For<ITempDataProvider>());
        return controller;
    }

    // ---------- Positive tests ----------

    [Fact]
    public async Task Create_ValidCharacter_SavesAndRedirectsToManage()
    {
        var repo = Substitute.For<ICharacterRepository>();
        repo.GetAll().Returns(SampleCharacters());
        repo.Create(Arg.Any<Character>()).Returns(true);
        var controller = CreateController(repo);

        var result = await controller.Create(new CharacterFormViewModel { Name = "Bounty Hunter", Price = 500 });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Manage", redirect.ActionName);
        await repo.Received(1).Create(Arg.Any<Character>());
    }

    [Fact]
    public async Task Edit_ValidCharacter_UpdatesAndRedirectsToManage()
    {
        var repo = Substitute.For<ICharacterRepository>();
        var characters = SampleCharacters();
        repo.GetAll().Returns(characters);
        repo.GetCharacterById(2).Returns(characters[1]);
        repo.Update(Arg.Any<Character>()).Returns(true);
        var controller = CreateController(repo);

        var result = await controller.Edit(2, new CharacterFormViewModel { CharacterId = 2, Name = "Smuggler", Price = 350 });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Manage", redirect.ActionName);
        await repo.Received(1).Update(Arg.Is<Character>(c => c.Price == 350));
    }

    [Fact]
    public async Task DeleteConfirmed_UnownedCharacter_DeletesIt()
    {
        var repo = Substitute.For<ICharacterRepository>();
        var characters = SampleCharacters();
        repo.GetCharacterById(2).Returns(characters[1]);
        repo.GetAll().Returns(characters);
        repo.CountOwners(2).Returns(0);
        repo.Delete(2).Returns(true);
        var controller = CreateController(repo);

        var result = await controller.DeleteConfirmed(2);

        Assert.IsType<RedirectToActionResult>(result);
        await repo.Received(1).Delete(2);
    }

    [Fact]
    public async Task Manage_ReturnsViewWithOneRowPerCharacter()
    {
        var repo = Substitute.For<ICharacterRepository>();
        repo.GetAll().Returns(SampleCharacters());
        repo.CountOwners(Arg.Any<int>()).Returns(0);
        var controller = CreateController(repo);

        var result = await controller.Manage();

        var view = Assert.IsType<ViewResult>(result);
        var rows = Assert.IsAssignableFrom<List<CharacterManageViewModel>>(view.Model);
        Assert.Equal(2, rows.Count);
        // The only free character can't be deleted, the paid one can
        Assert.False(rows[0].CanDelete);
        Assert.True(rows[1].CanDelete);
    }

    // ---------- Negative tests ----------

    [Fact]
    public async Task Create_DuplicateName_ReturnsViewAndDoesNotSave()
    {
        var repo = Substitute.For<ICharacterRepository>();
        repo.GetAll().Returns(SampleCharacters());
        var controller = CreateController(repo);

        // "smuggler" already exists (the check ignores upper/lower case)
        var result = await controller.Create(new CharacterFormViewModel { Name = "smuggler", Price = 100 });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        await repo.DidNotReceive().Create(Arg.Any<Character>());
    }

    [Fact]
    public async Task Edit_LastFreeCharacterGetsAPrice_ReturnsViewAndDoesNotSave()
    {
        var repo = Substitute.For<ICharacterRepository>();
        var characters = SampleCharacters();
        repo.GetAll().Returns(characters);
        repo.GetCharacterById(1).Returns(characters[0]);
        var controller = CreateController(repo);

        // Domain rule: at least one character must stay free for new players
        var result = await controller.Edit(1, new CharacterFormViewModel { CharacterId = 1, Name = "Rookie Slicer", Price = 50 });

        Assert.IsType<ViewResult>(result);
        Assert.True(controller.ModelState.ContainsKey(nameof(Character.Price)));
        await repo.DidNotReceive().Update(Arg.Any<Character>());
    }

    [Fact]
    public async Task Edit_IdMismatch_ReturnsBadRequest()
    {
        var repo = Substitute.For<ICharacterRepository>();
        var controller = CreateController(repo);

        var result = await controller.Edit(1, new CharacterFormViewModel { CharacterId = 2, Name = "Smuggler" });

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_OwnedCharacter_IsBlocked()
    {
        var repo = Substitute.For<ICharacterRepository>();
        var characters = SampleCharacters();
        repo.GetCharacterById(2).Returns(characters[1]);
        repo.GetAll().Returns(characters);
        repo.CountOwners(2).Returns(3);   // three players own it
        var controller = CreateController(repo);

        var result = await controller.DeleteConfirmed(2);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(controller.TempData["ErrorMessage"]);
        await repo.DidNotReceive().Delete(Arg.Any<int>());
    }

    [Fact]
    public async Task Create_RepositoryThrows_ShowsErrorInsteadOfCrashing()
    {
        var repo = Substitute.For<ICharacterRepository>();
        repo.GetAll().Returns(SampleCharacters());
        repo.Create(Arg.Any<Character>()).ThrowsAsync(new Exception("database down"));
        var controller = CreateController(repo);

        var result = await controller.Create(new CharacterFormViewModel { Name = "Bounty Hunter", Price = 500 });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }
}
