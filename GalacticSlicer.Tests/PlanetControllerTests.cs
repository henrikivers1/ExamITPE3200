using GalacticSlicer.Controllers;
using GalacticSlicer.DAL;
using GalacticSlicer.Models;
using GalacticSlicer.Services;
using GalacticSlicer.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GalacticSlicer.Tests;

// Unit tests for PlanetController's admin pages (Manage / Edit / Delete), with a mocked repository.
public class PlanetControllerTests
{
    private static List<Planet> SamplePlanets() => new()
    {
        new Planet { PlanetId = 1, Name = "Tatooine", Order = 1, CssClass = "tatooine", Challenges = new List<Challenge>() },
        new Planet { PlanetId = 2, Name = "Naboo", Order = 2, CssClass = "naboo", Challenges = new List<Challenge>() }
    };

    // Only the repository is used by these actions; the other services are not needed
    private static PlanetController CreateController(Mock<IPlanetRepository> repo)
    {
        var controller = new PlanetController(
            repo.Object,
            NullLogger<PlanetController>.Instance,
            null!,                        // ChallengeCompletionService
            null!,                        // UserManager
            new PlanetProgressService(),  // no database, safe to create
            null!,                        // PlayerProgressService
            null!,                        // QuestionService
            new RankService());           // no database, safe to create

        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        return controller;
    }

    // ---------- Positive tests ----------

    [Fact]
    public async Task Manage_ReturnsViewWithOneRowPerPlanet()
    {
        var repo = new Mock<IPlanetRepository>();
        repo.Setup(r => r.GetAll()).ReturnsAsync(SamplePlanets());
        var controller = CreateController(repo);

        var result = await controller.Manage();

        var view = Assert.IsType<ViewResult>(result);
        var rows = Assert.IsAssignableFrom<List<PlanetManageViewModel>>(view.Model);
        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public async Task DeleteConfirmed_EmptyPlanetThatIsNotFirst_DeletesIt()
    {
        var repo = new Mock<IPlanetRepository>();
        repo.Setup(r => r.GetPlanetById(2)).ReturnsAsync(SamplePlanets()[1]);
        repo.Setup(r => r.Delete(2)).ReturnsAsync(true);
        var controller = CreateController(repo);

        var result = await controller.DeleteConfirmed(2);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Manage", redirect.ActionName);
        repo.Verify(r => r.Delete(2), Times.Once);
    }

    // ---------- Negative tests ----------

    [Fact]
    public async Task DeleteConfirmed_FirstPlanet_IsBlocked()
    {
        var repo = new Mock<IPlanetRepository>();
        repo.Setup(r => r.GetPlanetById(1)).ReturnsAsync(SamplePlanets()[0]);
        var controller = CreateController(repo);

        // Domain rule: chapter 1 can never be deleted, or new players have nothing to play
        var result = await controller.DeleteConfirmed(1);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(controller.TempData["ErrorMessage"]);
        repo.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Edit_UnknownPlanet_ReturnsNotFound()
    {
        var repo = new Mock<IPlanetRepository>();
        repo.Setup(r => r.GetPlanetById(99)).ReturnsAsync((Planet?)null);
        var controller = CreateController(repo);

        var result = await controller.Edit(99);

        Assert.IsType<NotFoundResult>(result);
    }
}
