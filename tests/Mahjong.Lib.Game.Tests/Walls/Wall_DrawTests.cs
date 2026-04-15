using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;

namespace Mahjong.Lib.Game.Tests.Walls;

public class Wall_DrawTests
{
    [Fact]
    public void 初期状態から1枚引く_末尾の牌が返される()
    {
        // Arrange
        var wall = new Wall(Enumerable.Range(Tile.ID_MIN, Tile.ID_MAX + 1).Select(x => new Tile(x)));

        // Act
        var next = wall.Draw(out var tile);

        // Assert
        Assert.Equal(new Tile(135), tile);
        Assert.Equal(1, next.DrawnCount);
    }

    [Fact]
    public void 三枚続けて引く_末尾から順に取られる()
    {
        // Arrange
        var wall = new Wall(Enumerable.Range(Tile.ID_MIN, Tile.ID_MAX + 1).Select(x => new Tile(x)));

        // Act
        wall = wall.Draw(out var first);
        wall = wall.Draw(out var second);
        wall = wall.Draw(out var third);

        // Assert
        Assert.Equal(new Tile(135), first);
        Assert.Equal(new Tile(134), second);
        Assert.Equal(new Tile(133), third);
        Assert.Equal(3, wall.DrawnCount);
    }

    [Fact]
    public void 百二十二枚取った後にさらに引く_InvalidOperationExceptionが発生する()
    {
        // Arrange
        var wall = new Wall(Enumerable.Range(Tile.ID_MIN, Tile.ID_MAX + 1).Select(x => new Tile(x)));
        for (var i = 0; i < 122; i++)
        {
            wall = wall.Draw(out _);
        }

        // Act
        var ex = Record.Exception(() => wall.Draw(out _));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void 百二十二枚引いた後のLiveRemaining_ゼロが返される()
    {
        // Arrange
        var wall = new Wall(Enumerable.Range(Tile.ID_MIN, Tile.ID_MAX + 1).Select(x => new Tile(x)));
        for (var i = 0; i < 122; i++)
        {
            wall = wall.Draw(out _);
        }

        // Assert
        Assert.Equal(0, wall.LiveRemaining);
    }
}
