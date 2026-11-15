using GalacticSlicer.Controllers;
using GalacticSlicer.DAL;
using GalacticSlicer.Models;
using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GalacticSlicer.Tests;

// Unit tests for CharacterController (Manage / Create / Edit / Delete).
// Same pattern as the course's ItemController tests: the repository is a Moq mock,
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
    private static CharacterController CreateController(Mock<ICharacterRepository> repo)
    {
        var controller = new CharacterController(
            repo.Object,
            NullLogger<CharacterController>.Instance,
            null!,   // CharacterService
            null!,   // UserManager
            null!);  // PlayerProgressService

        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        return controller;
    }

    // ---------- Positive tests ----------

    [Fact]
    public async Task Create_ValidCharacter_SavesAndRedirectsToManage()
    {
        var repo = new Mock<ICharacterRepository>();
        repo.Setup(r => r.GetAll()).ReturnsAsync(SampleCharacters());
        repo.Setup(r => r.Create(It.IsAny<Character>())).ReturnsAsync(true);
        var controller = CreateController(repo);

        var result = await controller.Create(new Character { Name = "Bounty Hunter", Price = 500 });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Manage", redirect.ActionName);
        repo.Verify(r => r.Create(It.IsAny<Character>()), Times.Once);
    }

    [Fact]
    public async Task Edit_ValidCharacter_UpdatesAndRedirectsToManage()
    {
        var repo = new Mock<ICharacterRepository>();
        var characters = SampleCharacters();
        repo.Setup(r => r.GetAll()).ReturnsAsync(characters);
        repo.Setup(r => r.GetCharacterById(2)).ReturnsAsync(characters[1]);
        repo.Setup(r => r.Update(It.IsAny<Character>())).ReturnsAsync(true);
        var controller = CreateController(repo);

        var result = await controller.Edit(2, new Character { CharacterId = 2, Name = "Smuggler", Price = 350 });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Manage", redirect.ActionName);
        repo.Verify(r => r.Update(It.Is<Character>(c => c.Price == 350)), Times.Once);
    }

    [Fact]
    public async Task DeleteConfirmed_UnownedCharacter_DeletesIt()
    {
        var repo = new Mock<ICharacterRepository>();
        var characters = SampleCharacters();
        repo.Setup(r => r.GetCharacterById(2)).ReturnsAsync(characters[1]);
        repo.Setup(r => r.GetAll()).ReturnsAsync(characters);
        repo.Setup(r => r.CountOwners(2)).ReturnsAsync(0);
        repo.Setup(r => r.Delete(2)).ReturnsAsync(true);
        var controller = CreateController(repo);

        var result = await controller.DeleteConfirmed(2);

        Assert.IsType<RedirectToActionResult>(result);
        repo.Verify(r => r.Delete(2), Times.Once);
    }

    [Fact]
    public async Task Manage_ReturnsViewWithOneRowPerCharacter()
    {
        var repo = new Mock<ICharacterRepository>();
        repo.Setup(r => r.GetAll()).ReturnsAsync(SampleCharacters());
        repo.Setup(r => r.CountOwners(It.IsAny<int>())).ReturnsAsync(0);
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
        var repo = new Mock<ICharacterRepository>();
        repo.Setup(r => r.GetAll()).ReturnsAsync(SampleCharacters());
        var controller = CreateController(repo);

        // "smuggler" already exists (the check ignores upper/lower case)
        var result = await controller.Create(new Character { Name = "smuggler", Price = 100 });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        repo.Verify(r => r.Create(It.IsAny<Character>()), Times.Never);
    }

    [Fact]
    public async Task Edit_LastFreeCharacterGetsAPrice_ReturnsViewAndDoesNotSave()
    {
        var repo = new Mock<ICharacterRepository>();
        var characters = SampleCharacters();
        repo.Setup(r => r.GetAll()).ReturnsAsync(characters);
        repo.Setup(r => r.GetCharacterById(1)).ReturnsAsync(characters[0]);
        var controller = CreateController(repo);

        // Domain rule: at least one character must stay free for new players
        var result = await controller.Edit(1, new Character { CharacterId = 1, Name = "Rookie Slicer", Price = 50 });

        Assert.IsType<ViewResult>(result);
        Assert.True(controller.ModelState.ContainsKey(nameof(Character.Price)));
        repo.Verify(r => r.Update(It.IsAny<Character>()), Times.Never);
    }

    [Fact]
    public async Task Edit_IdMismatch_ReturnsBadRequest()
    {
        var repo = new Mock<ICharacterRepository>();
        var controller = CreateController(repo);

        var result = await controller.Edit(1, new Character { CharacterId = 2, Name = "Smuggler" });

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_OwnedCharacter_IsBlocked()
    {
        var repo = new Mock<ICharacterRepository>();
        var characters = SampleCharacters();
        repo.Setup(r => r.GetCharacterById(2)).ReturnsAsync(characters[1]);
        repo.Setup(r => r.GetAll()).ReturnsAsync(characters);
        repo.Setup(r => r.CountOwners(2)).ReturnsAsync(3);   // three players own it
        var controller = CreateController(repo);

        var result = await controller.DeleteConfirmed(2);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(controller.TempData["ErrorMessage"]);
        repo.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Create_RepositoryThrows_ShowsErrorInsteadOfCrashing()
    {
        var repo = new Mock<ICharacterRepository>();
        repo.Setup(r => r.GetAll()).ReturnsAsync(SampleCharacters());
        repo.Setup(r => r.Create(It.IsAny<Character>())).ThrowsAsync(new Exception("database down"));
        var controller = CreateController(repo);

        var result = await controller.Create(new Character { Name = "Bounty Hunter", Price = 500 });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }
}
