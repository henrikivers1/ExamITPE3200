
using GalacticSlicer.Models;
using GalacticSlicer.Services;
using Xunit;

namespace GalacticSlicer.Tests;

public class PlanetProgressServiceTests
{
    private readonly PlanetProgressService _service = new();

    // Positive test: Naboo should unlock when the player has enough XP
    [Fact]
    public void IsPlanetUnlocked_WithEnoughXp_ReturnsTrue()
    {
        // Arrange
        var planet = new Planet { Order = 2 };
        var progress = new PlayerProgress { ExperiencePoints = 600 };

        // Act
        bool result = _service.IsPlanetUnlocked(planet, progress);

        // Assert
        Assert.True(result);
    }

    // Negative test: Coruscant should remain locked with insufficient XP
    [Fact]
    public void IsPlanetUnlocked_WithInsufficientXp_ReturnsFalse()
    {
        // Arrange
        var planet = new Planet { Order = 3 };
        var progress = new PlayerProgress { ExperiencePoints = 600 };

        // Act
        bool result = _service.IsPlanetUnlocked(planet, progress);

        // Assert
        Assert.False(result);
    }
}
