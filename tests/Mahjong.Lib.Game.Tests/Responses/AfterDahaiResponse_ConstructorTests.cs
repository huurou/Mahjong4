using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Responses;

public class AfterDahaiResponse_ConstructorTests
{
    [Fact]
    public void ChiResponse_HandTilesが保持される()
    {
        // Arrange
        var tiles = ImmutableArray.Create(new Tile(0), new Tile(4));

        // Act
        var response = new ChiResponse(tiles);

        // Assert
        Assert.Equal(2, response.HandTiles.Length);
    }

    [Fact]
    public void 全サブ型がAfterDahaiResponseを継承している()
    {
        // Assert
        Assert.IsType<AfterDahaiResponse>(new PassResponse(), exactMatch: false);
        Assert.IsType<AfterDahaiResponse>(new ChiResponse([]), exactMatch: false);
        Assert.IsType<AfterDahaiResponse>(new PonResponse([]), exactMatch: false);
        Assert.IsType<AfterDahaiResponse>(new DaiminkanResponse([]), exactMatch: false);
        Assert.IsType<AfterDahaiResponse>(new RonResponse(), exactMatch: false);
    }

    [Fact]
    public void 全サブ型がPlayerResponseを継承している()
    {
        // Assert
        Assert.IsType<PlayerResponse>(new PassResponse(), exactMatch: false);
        Assert.IsType<PlayerResponse>(new RonResponse(), exactMatch: false);
    }
}
