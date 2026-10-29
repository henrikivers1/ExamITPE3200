
using GalacticSlicer.Services;
using Xunit;

namespace GalacticSlicer.Tests;

public class RankServiceTests
{
    private readonly RankService _rankService = new();

    // Positive test: A player with 500 XP should reach Slicer rank
    [Fact]
    public void GetRank_With500Xp_ReturnsSlicer()
    {
        // Arrange
        int xp = 500;

        // Act
        string rank = _rankService.GetRank(xp);

        // Assert
        Assert.Equal("Slicer", rank);
    }

    // Negative test: A player with insufficient XP should not reach Slicer
    [Fact]
    public void GetRank_With499Xp_DoesNotReturnSlicer()
    {
        // Arrange
        int xp = 499;

        // Act
        string rank = _rankService.GetRank(xp);

        // Assert
        Assert.Equal("Outsider", rank);
        Assert.NotEqual("Slicer", rank);
    }
}
