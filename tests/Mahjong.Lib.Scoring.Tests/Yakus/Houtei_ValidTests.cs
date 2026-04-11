using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Houtei_ValidTests
{
    [Fact]
    public void Valid_河底撈魚である場合_成立する()
    {
        // Arrange
        var winSituation = new WinSituation { IsHoutei = true };

        // Act
        var actual = Houtei.Valid(winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_河底撈魚でない場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation { IsHoutei = false };

        // Act
        var actual = Houtei.Valid(winSituation);

        // Assert
        Assert.False(actual);
    }
}
