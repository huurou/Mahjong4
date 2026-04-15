using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rivers;

public class River_EqualsTests
{
    [Fact]
    public void 同じ牌列で作られた別インスタンス_等価になる()
    {
        // Arrange
        var a = new River([new Tile(0), new Tile(1), new Tile(2)]);
        var b = new River([new Tile(0), new Tile(1), new Tile(2)]);

        // Act & Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void 異なる牌列_非等価になる()
    {
        // Arrange
        var a = new River([new Tile(0), new Tile(1)]);
        var b = new River([new Tile(0), new Tile(2)]);

        // Act & Assert
        Assert.NotEqual(a, b);
    }
}
