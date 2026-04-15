using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;

namespace Mahjong.Lib.Game.Tests.Walls;

public class Wall_DrawRinshanTests
{
    private static Wall CreateWall()
    {
        return new Wall(Enumerable.Range(Tile.ID_MIN, Tile.ID_MAX + 1).Select(x => new Tile(x)));
    }

    [Fact]
    public void 一枚目の嶺上_yama1の牌が返される()
    {
        // Arrange
        var wall = CreateWall();

        // Act
        var next = wall.DrawRinshan(out var tile);

        // Assert
        Assert.Equal(new Tile(1), tile);
        Assert.Equal(1, next.RinshanDrawnCount);
    }

    [Fact]
    public void 四枚続けて引く_1_0_3_2の順で取られる()
    {
        // Arrange
        var wall = CreateWall();

        // Act
        wall = wall.DrawRinshan(out var first);
        wall = wall.DrawRinshan(out var second);
        wall = wall.DrawRinshan(out var third);
        wall = wall.DrawRinshan(out var fourth);

        // Assert
        Assert.Equal(new Tile(1), first);
        Assert.Equal(new Tile(0), second);
        Assert.Equal(new Tile(3), third);
        Assert.Equal(new Tile(2), fourth);
        Assert.Equal(0, wall.RinshanRemaining);
    }

    [Fact]
    public void 四枚取った後にさらに引く_InvalidOperationExceptionが発生する()
    {
        // Arrange
        var wall = CreateWall();
        for (var i = 0; i < 4; i++)
        {
            wall = wall.DrawRinshan(out _);
        }

        // Act
        var ex = Record.Exception(() => wall.DrawRinshan(out _));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void 嶺上ツモ後_LiveRemainingが1減る()
    {
        // Arrange
        var wall = CreateWall();
        var initial = wall.LiveRemaining;

        // Act
        var result = wall.DrawRinshan(out _);

        // Assert
        Assert.Equal(initial - 1, result.LiveRemaining);
    }

    [Fact]
    public void ツモ山が残り0枚_DrawRinshan_InvalidOperationExceptionが発生する()
    {
        // Arrange
        var wall = CreateWall();
        for (var i = 0; i < 122; i++)
        {
            wall = wall.Draw(out _);
        }

        // Act
        var ex = Record.Exception(() => wall.DrawRinshan(out _));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }
}
