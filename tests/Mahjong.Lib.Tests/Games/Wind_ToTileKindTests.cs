using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Games;

public class Wind_ToTileKindTests
{
    [Fact]
    public void 東_TileKindのTonを返す()
    {
        // Arrange
        var wind = Wind.East;

        // Act
        var result = wind.ToTileKind();

        // Assert
        Assert.Equal(TileKind.Ton, result);
    }

    [Fact]
    public void 南_TileKindのNanを返す()
    {
        // Arrange
        var wind = Wind.South;

        // Act
        var result = wind.ToTileKind();

        // Assert
        Assert.Equal(TileKind.Nan, result);
    }

    [Fact]
    public void 西_TileKindのShaを返す()
    {
        // Arrange
        var wind = Wind.West;

        // Act
        var result = wind.ToTileKind();

        // Assert
        Assert.Equal(TileKind.Sha, result);
    }

    [Fact]
    public void 北_TileKindのPeiを返す()
    {
        // Arrange
        var wind = Wind.North;

        // Act
        var result = wind.ToTileKind();

        // Assert
        Assert.Equal(TileKind.Pei, result);
    }

    [Fact]
    public void 無効な値_ArgumentOutOfRangeException発生()
    {
        // Arrange
        var invalidWind = (Wind)999;

        // Act
        var ex = Record.Exception(() => invalidWind.ToTileKind());

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
        var argEx = (ArgumentOutOfRangeException)ex;
        Assert.Equal("wind", argEx.ParamName);
    }
}
