using Mahjong.Lib.Games;

namespace Mahjong.Lib.Tests.Games;

public class WinSituation_IsDealerTests
{
    [Fact]
    public void PlayerWindが東_trueを返す()
    {
        // Arrange
        var situation = new WinSituation { PlayerWind = Wind.East };

        // Act
        var result = situation.IsDealer;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PlayerWindが南_falseを返す()
    {
        // Arrange
        var situation = new WinSituation { PlayerWind = Wind.South };

        // Act
        var result = situation.IsDealer;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PlayerWindが西_falseを返す()
    {
        // Arrange
        var situation = new WinSituation { PlayerWind = Wind.West };

        // Act
        var result = situation.IsDealer;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PlayerWindが北_falseを返す()
    {
        // Arrange
        var situation = new WinSituation { PlayerWind = Wind.North };

        // Act
        var result = situation.IsDealer;

        // Assert
        Assert.False(result);
    }
}
