using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKind_NumberTests
{
    [Fact]
    public void 各牌の数値が正しい()
    {
        // Arrange & Act & Assert
        // 萬子
        Assert.Equal(1, TileKind.Man1.Number);
        Assert.Equal(2, TileKind.Man2.Number);
        Assert.Equal(3, TileKind.Man3.Number);
        Assert.Equal(4, TileKind.Man4.Number);
        Assert.Equal(5, TileKind.Man5.Number);
        Assert.Equal(6, TileKind.Man6.Number);
        Assert.Equal(7, TileKind.Man7.Number);
        Assert.Equal(8, TileKind.Man8.Number);
        Assert.Equal(9, TileKind.Man9.Number);

        // 筒子
        Assert.Equal(1, TileKind.Pin1.Number);
        Assert.Equal(2, TileKind.Pin2.Number);
        Assert.Equal(3, TileKind.Pin3.Number);
        Assert.Equal(4, TileKind.Pin4.Number);
        Assert.Equal(5, TileKind.Pin5.Number);
        Assert.Equal(6, TileKind.Pin6.Number);
        Assert.Equal(7, TileKind.Pin7.Number);
        Assert.Equal(8, TileKind.Pin8.Number);
        Assert.Equal(9, TileKind.Pin9.Number);

        // 索子
        Assert.Equal(1, TileKind.Sou1.Number);
        Assert.Equal(2, TileKind.Sou2.Number);
        Assert.Equal(3, TileKind.Sou3.Number);
        Assert.Equal(4, TileKind.Sou4.Number);
        Assert.Equal(5, TileKind.Sou5.Number);
        Assert.Equal(6, TileKind.Sou6.Number);
        Assert.Equal(7, TileKind.Sou7.Number);
        Assert.Equal(8, TileKind.Sou8.Number);
        Assert.Equal(9, TileKind.Sou9.Number);

        // 字牌
        Assert.Equal(1, TileKind.Ton.Number);
        Assert.Equal(2, TileKind.Nan.Number);
        Assert.Equal(3, TileKind.Sha.Number);
        Assert.Equal(4, TileKind.Pei.Number);
        Assert.Equal(5, TileKind.Haku.Number);
        Assert.Equal(6, TileKind.Hatsu.Number);
        Assert.Equal(7, TileKind.Chun.Number);
    }
}
