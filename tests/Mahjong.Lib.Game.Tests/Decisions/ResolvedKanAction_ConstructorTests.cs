using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Decisions;

public class ResolvedAnkanAction_ConstructorTests
{
    [Fact]
    public void 牌を4枚指定_正常に生成される()
    {
        // Arrange
        var tiles = ImmutableArray.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3));

        // Act
        var action = new ResolvedAnkanAction(tiles);

        // Assert
        Assert.Equal(4, action.Tiles.Length);
        Assert.IsType<ResolvedKanAction>(action, exactMatch: false);
    }

    [Fact]
    public void 牌が4枚以外_例外が発生する()
    {
        // Arrange
        var tiles = ImmutableArray.Create(new Tile(0));

        // Act
        var ex = Record.Exception(() => new ResolvedAnkanAction(tiles));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }
}

public class ResolvedKakanAction_ConstructorTests
{
    [Fact]
    public void 牌を1枚指定_正常に生成される()
    {
        // Act
        var action = new ResolvedKakanAction(new Tile(16));

        // Assert
        Assert.Equal(16, action.Tile.Id);
        Assert.IsType<ResolvedKanAction>(action, exactMatch: false);
    }
}
