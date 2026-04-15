using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rivers;

public class RiverArray_EqualsTests
{
    [Fact]
    public void 同じ内容の別インスタンス_等価になる()
    {
        // Arrange
        var a = new RiverArray().AddTile(new PlayerIndex(0), new Tile(0));
        var b = new RiverArray().AddTile(new PlayerIndex(0), new Tile(0));

        // Act & Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void 異なるプレイヤーに追加した配列_非等価になる()
    {
        // Arrange
        var a = new RiverArray().AddTile(new PlayerIndex(0), new Tile(0));
        var b = new RiverArray().AddTile(new PlayerIndex(1), new Tile(0));

        // Act & Assert
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void 異なる牌を追加した配列_非等価になる()
    {
        // Arrange
        var a = new RiverArray().AddTile(new PlayerIndex(0), new Tile(0));
        var b = new RiverArray().AddTile(new PlayerIndex(0), new Tile(1));

        // Act & Assert
        Assert.NotEqual(a, b);
    }
}
