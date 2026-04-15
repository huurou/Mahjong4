using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Calls;

public class Call_EqualsTests
{
    [Fact]
    public void ポン_同じ内容の別インスタンス_等価になる()
    {
        // Arrange
        var a = new Call(
            CallType.Pon,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2)),
            new PlayerIndex(1),
            new Tile(0)
        );
        var b = new Call(
            CallType.Pon,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2)),
            new PlayerIndex(1),
            new Tile(0)
        );

        // Act & Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void チー_同じ内容の別インスタンス_等価になる()
    {
        // Arrange (1m, 2m, 3m)
        var a = new Call(
            CallType.Chi,
            ImmutableList.Create(new Tile(0), new Tile(4), new Tile(8)),
            new PlayerIndex(3),
            new Tile(8)
        );
        var b = new Call(
            CallType.Chi,
            ImmutableList.Create(new Tile(0), new Tile(4), new Tile(8)),
            new PlayerIndex(3),
            new Tile(8)
        );

        // Act & Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void 暗槓_同じ内容の別インスタンス_等価になる()
    {
        // Arrange
        var a = new Call(
            CallType.Ankan,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3)),
            new PlayerIndex(0),
            new Tile(0)
        );
        var b = new Call(
            CallType.Ankan,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3)),
            new PlayerIndex(0),
            new Tile(0)
        );

        // Act & Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Tilesが異なる_非等価になる()
    {
        // Arrange
        var a = new Call(
            CallType.Pon,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2)),
            new PlayerIndex(1),
            new Tile(0)
        );
        var b = new Call(
            CallType.Pon,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(3)),
            new PlayerIndex(1),
            new Tile(0)
        );

        // Act & Assert
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void CalledTileが異なる_非等価になる()
    {
        // Arrange
        var a = new Call(
            CallType.Pon,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2)),
            new PlayerIndex(1),
            new Tile(0)
        );
        var b = new Call(
            CallType.Pon,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2)),
            new PlayerIndex(1),
            new Tile(1)
        );

        // Act & Assert
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Fromが異なる_非等価になる()
    {
        // Arrange
        var a = new Call(
            CallType.Pon,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2)),
            new PlayerIndex(1),
            new Tile(0)
        );
        var b = new Call(
            CallType.Pon,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2)),
            new PlayerIndex(2),
            new Tile(0)
        );

        // Act & Assert
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Tilesの順序が異なる_非等価になる()
    {
        // Arrange (SequenceEqual は順序を区別する)
        var a = new Call(
            CallType.Pon,
            ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2)),
            new PlayerIndex(1),
            new Tile(0)
        );
        var b = new Call(
            CallType.Pon,
            ImmutableList.Create(new Tile(2), new Tile(1), new Tile(0)),
            new PlayerIndex(1),
            new Tile(0)
        );

        // Act & Assert
        Assert.NotEqual(a, b);
    }
}
