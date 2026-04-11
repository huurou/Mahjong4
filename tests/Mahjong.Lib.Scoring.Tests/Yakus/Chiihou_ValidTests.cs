using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Chiihou_ValidTests
{
    [Fact]
    public void Valid_地和成立時_成立する()
    {
        // Arrange
        var winSituation = new WinSituation { IsChiihou = true };

        // Act
        var actual = Chiihou.Valid(winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_地和不成立時_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation { IsChiihou = false };

        // Act
        var actual = Chiihou.Valid(winSituation);

        // Assert
        Assert.False(actual);
    }
}
