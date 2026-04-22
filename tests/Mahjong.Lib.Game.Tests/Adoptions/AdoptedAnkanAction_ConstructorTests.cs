using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Adoptions;

public class AdoptedAnkanAction_ConstructorTests
{
    [Fact]
    public void 牌を4枚指定_正常に生成される()
    {
        // Arrange
        var tiles = ImmutableArray.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3));

        // Act
        var action = new AdoptedAnkanAction(tiles);

        // Assert
        Assert.Equal(4, action.Tiles.Length);
        Assert.IsType<AdoptedKanAction>(action, exactMatch: false);
    }

    [Fact]
    public void 牌が4枚以外_例外が発生する()
    {
        // Arrange
        var tiles = ImmutableArray.Create(new Tile(0));

        // Act
        var ex = Record.Exception(() => new AdoptedAnkanAction(tiles));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }
}
