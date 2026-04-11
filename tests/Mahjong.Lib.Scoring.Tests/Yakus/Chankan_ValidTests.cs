using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Chankan_ValidTests
{
    [Fact]
    public void Valid_槍槓の場合_成立する()
    {
        // Arrange
        var winSituation = new WinSituation { IsChankan = true };

        // Act
        var actual = Chankan.Valid(winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_槍槓でない場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation { IsChankan = false };

        // Act
        var actual = Chankan.Valid(winSituation);

        // Assert
        Assert.False(actual);
    }
}
