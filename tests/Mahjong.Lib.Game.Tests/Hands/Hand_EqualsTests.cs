using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Hands;

public class Hand_EqualsTests
{
    [Fact]
    public void 同じ牌列で作られた別インスタンス_等価になる()
    {
        // Arrange
        var a = new Hand([new Tile(0), new Tile(1), new Tile(2)]);
        var b = new Hand([new Tile(0), new Tile(1), new Tile(2)]);

        // Act & Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void 異なる牌列_非等価になる()
    {
        // Arrange
        var a = new Hand([new Tile(0), new Tile(1)]);
        var b = new Hand([new Tile(0), new Tile(2)]);

        // Act & Assert
        Assert.NotEqual(a, b);
    }
}
