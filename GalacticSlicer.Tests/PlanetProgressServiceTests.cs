
using GalacticSlicer.Models;
using GalacticSlicer.Services;
using Xunit;

namespace GalacticSlicer.Tests;

public class PlanetProgressServiceTests
{
    private readonly PlanetProgressService _service = new();

    // Positive test: Naboo (chapter 2) unlocks after one passed mission (2000 XP)
    [Fact]
    public void IsPlanetUnlocked_WithEnoughXp_ReturnsTrue()
    {
        // Arrange
        var planet = new Planet { Order = 2 };
        var progress = new PlayerProgress { ExperiencePoints = 2000 };

        // Act
        bool result = _service.IsPlanetUnlocked(planet, progress);

        // Assert
        Assert.True(result);
    }

    // Negative test: Coruscant (chapter 3, needs 4000 XP) stays locked after only one passed mission
    [Fact]
    public void IsPlanetUnlocked_WithInsufficientXp_ReturnsFalse()
    {
        // Arrange
        var planet = new Planet { Order = 3 };
        var progress = new PlayerProgress { ExperiencePoints = 2000 };

        // Act
        bool result = _service.IsPlanetUnlocked(planet, progress);

        // Assert
        Assert.False(result);
    }
}
