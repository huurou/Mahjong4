using Mahjong.Lib.Games;
using Mahjong.Lib.Yakus;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Tenhou_ValidTests
{

    [Fact]
    public void Valid_天和成立時_成立する()
    {
        // Arrange
        var winSituation = new WinSituation { IsTenhou = true };

        // Act
        var actual = Tenhou.Valid(winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_天和不成立時_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation { IsTenhou = false };

        // Act
        var actual = Tenhou.Valid(winSituation);

        // Assert
        Assert.False(actual);
    }
}
