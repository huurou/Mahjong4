using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Calls;

public class Call_ConstructorTests
{
    [Fact]
    public void チー_連続する数牌3枚_正常に作成される()
    {
        // Arrange
        // 1m, 2m, 3m (kind 0, 1, 2)
        var tiles = ImmutableList.Create(new Tile(0), new Tile(4), new Tile(8));

        // Act
        var call = new Call(CallType.Chi, tiles, new PlayerIndex(0), new Tile(8));

        // Assert
        Assert.Equal(CallType.Chi, call.Type);
        Assert.Equal(3, call.Tiles.Count);
    }

    [Fact]
    public void チー_順不同の連続する数牌3枚_正常に作成される()
    {
        // Arrange (ソート不要)
        var tiles = ImmutableList.Create(new Tile(8), new Tile(0), new Tile(4));

        // Act
        var call = new Call(CallType.Chi, tiles, new PlayerIndex(0), new Tile(0));

        // Assert
        Assert.Equal(CallType.Chi, call.Type);
    }

    [Fact]
    public void チー_字牌を含む_ArgumentExceptionが発生する()
    {
        // Arrange (kind 27 = 東)
        var tiles = ImmutableList.Create(new Tile(108), new Tile(112), new Tile(116));

        // Act
        var ex = Record.Exception(() => new Call(CallType.Chi, tiles, new PlayerIndex(0), new Tile(108)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void チー_異なるスーツ_ArgumentExceptionが発生する()
    {
        // Arrange (8m, 9m, 1p = kind 7, 8, 9)
        var tiles = ImmutableList.Create(new Tile(28), new Tile(32), new Tile(36));

        // Act
        var ex = Record.Exception(() => new Call(CallType.Chi, tiles, new PlayerIndex(0), new Tile(36)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void チー_連続しない_ArgumentExceptionが発生する()
    {
        // Arrange (1m, 2m, 4m = kind 0, 1, 3)
        var tiles = ImmutableList.Create(new Tile(0), new Tile(4), new Tile(12));

        // Act
        var ex = Record.Exception(() => new Call(CallType.Chi, tiles, new PlayerIndex(0), new Tile(12)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void チー_枚数不正_ArgumentExceptionが発生する()
    {
        // Arrange
        var tiles = ImmutableList.Create(new Tile(0), new Tile(4));

        // Act
        var ex = Record.Exception(() => new Call(CallType.Chi, tiles, new PlayerIndex(0), new Tile(0)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void ポン_同じ牌種3枚_正常に作成される()
    {
        // Arrange
        var tiles = ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2));

        // Act
        var call = new Call(CallType.Pon, tiles, new PlayerIndex(0), new Tile(0));

        // Assert
        Assert.Equal(CallType.Pon, call.Type);
    }

    [Fact]
    public void ポン_異なる牌種を含む_ArgumentExceptionが発生する()
    {
        // Arrange (kind 0, 0, 1)
        var tiles = ImmutableList.Create(new Tile(0), new Tile(1), new Tile(4));

        // Act
        var ex = Record.Exception(() => new Call(CallType.Pon, tiles, new PlayerIndex(0), new Tile(0)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Theory]
    [InlineData(CallType.Ankan)]
    [InlineData(CallType.Daiminkan)]
    [InlineData(CallType.Kakan)]
    public void 槓_同じ牌種4枚_正常に作成される(CallType type)
    {
        // Arrange
        var tiles = ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3));

        // Act
        var call = new Call(type, tiles, new PlayerIndex(0), new Tile(0));

        // Assert
        Assert.Equal(type, call.Type);
    }

    [Theory]
    [InlineData(CallType.Ankan)]
    [InlineData(CallType.Daiminkan)]
    [InlineData(CallType.Kakan)]
    public void 槓_異なる牌種を含む_ArgumentExceptionが発生する(CallType type)
    {
        // Arrange (kind 0, 0, 0, 1)
        var tiles = ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(4));

        // Act
        var ex = Record.Exception(() => new Call(type, tiles, new PlayerIndex(0), new Tile(0)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Theory]
    [InlineData(CallType.Ankan, 3)]
    [InlineData(CallType.Daiminkan, 5)]
    [InlineData(CallType.Kakan, 3)]
    [InlineData(CallType.Pon, 4)]
    public void 枚数不正_ArgumentExceptionが発生する(CallType type, int tileCount)
    {
        // Arrange: 全て同じ牌種にする
        var tiles = Enumerable.Range(0, tileCount).Select(x => new Tile(x)).ToImmutableList();

        // Act
        var ex = Record.Exception(() => new Call(type, tiles, new PlayerIndex(0), new Tile(0)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void ポン_CalledTileがTilesに含まれない_ArgumentExceptionが発生する()
    {
        // Arrange (Tiles は 1m 3枚、CalledTile は Tiles に含まれない Tile(99))
        var tiles = ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2));

        // Act
        var ex = Record.Exception(() => new Call(CallType.Pon, tiles, new PlayerIndex(0), new Tile(99)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 大明槓_CalledTileがTilesに含まれない_ArgumentExceptionが発生する()
    {
        // Arrange
        var tiles = ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3));

        // Act
        var ex = Record.Exception(() => new Call(CallType.Daiminkan, tiles, new PlayerIndex(0), new Tile(99)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void チー_CalledTileがTilesに含まれない_ArgumentExceptionが発生する()
    {
        // Arrange (1m, 2m, 3m)
        var tiles = ImmutableList.Create(new Tile(0), new Tile(4), new Tile(8));

        // Act
        var ex = Record.Exception(() => new Call(CallType.Chi, tiles, new PlayerIndex(0), new Tile(99)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 暗槓_CalledTileがTilesに含まれない_例外を投げない()
    {
        // Arrange (暗槓は CalledTile が任意のため Tiles に含まれなくても例外にならない)
        var tiles = ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3));

        // Act
        var ex = Record.Exception(() => new Call(CallType.Ankan, tiles, new PlayerIndex(0), new Tile(99)));

        // Assert
        Assert.Null(ex);
    }
}
