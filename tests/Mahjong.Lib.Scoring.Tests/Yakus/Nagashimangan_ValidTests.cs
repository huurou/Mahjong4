using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Nagashimangan_ValidTests
{
    [Fact]
    public void Valid_流し満貫である場合_成立する()
    {
        // Arrange
        var winSituation = new WinSituation { IsNagashimangan = true };

        // Act
        var actual = Nagashimangan.Valid(winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_流し満貫でない場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation { IsNagashimangan = false };

        // Act
        var actual = Nagashimangan.Valid(winSituation);

        // Assert
        Assert.False(actual);
    }
}
