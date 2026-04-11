using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Akadora_ValidTests
{
    [Fact]
    public void Valid_赤ドラが1つ_成立する()
    {
        // Arrange
        var winSituation = new WinSituation
        {
            AkadoraCount = 1
        };

        // Act
        var actual = Akadora.Valid(winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_赤ドラが複数_成立する()
    {
        // Arrange
        var winSituation = new WinSituation
        {
            AkadoraCount = 3
        };

        // Act
        var actual = Akadora.Valid(winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_赤ドラなし_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation
        {
            AkadoraCount = 0
        };

        // Act
        var actual = Akadora.Valid(winSituation);

        // Assert
        Assert.False(actual);
    }
}
