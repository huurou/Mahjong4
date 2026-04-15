using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;

namespace Mahjong.Lib.Game.Tests.Walls;

public class Wall_DoraTests
{
    private static Wall CreateWall()
    {
        return new Wall(Enumerable.Range(Tile.ID_MIN, Tile.ID_MAX + 1).Select(x => new Tile(x)));
    }

    [Fact]
    public void 初期状態_DoraRevealedCountが一()
    {
        // Arrange
        var wall = CreateWall();

        // Assert
        Assert.Equal(1, wall.DoraRevealedCount);
    }

    [Fact]
    public void 初期ドラ表示牌_yama5の牌が返される()
    {
        // Arrange
        var wall = CreateWall();

        // Act
        var dora = wall.GetDoraIndicator(0);

        // Assert
        Assert.Equal(new Tile(5), dora);
    }

    [Fact]
    public void 初期裏ドラ表示牌_yama4の牌が返される()
    {
        // Arrange
        var wall = CreateWall();

        // Act
        var uradora = wall.GetUradoraIndicator(0);

        // Assert
        Assert.Equal(new Tile(4), uradora);
    }

    [Fact]
    public void RevealDora四回_新ドラが順にyama7_9_11_13を返す()
    {
        // Arrange
        var wall = CreateWall();

        // Act
        wall = wall.RevealDora();
        wall = wall.RevealDora();
        wall = wall.RevealDora();
        wall = wall.RevealDora();

        // Assert
        Assert.Equal(5, wall.DoraRevealedCount);
        Assert.Equal(new Tile(5), wall.GetDoraIndicator(0));
        Assert.Equal(new Tile(7), wall.GetDoraIndicator(1));
        Assert.Equal(new Tile(9), wall.GetDoraIndicator(2));
        Assert.Equal(new Tile(11), wall.GetDoraIndicator(3));
        Assert.Equal(new Tile(13), wall.GetDoraIndicator(4));
    }

    [Fact]
    public void RevealDora五回目_InvalidOperationExceptionが発生する()
    {
        // Arrange
        var wall = CreateWall();
        for (var i = 0; i < 4; i++)
        {
            wall = wall.RevealDora();
        }

        // Act
        var ex = Record.Exception(() => wall.RevealDora());

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void 未表示のドラ_ArgumentOutOfRangeExceptionが発生する()
    {
        // Arrange
        var wall = CreateWall();

        // Act
        var ex = Record.Exception(() => wall.GetDoraIndicator(1));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
    }
}
