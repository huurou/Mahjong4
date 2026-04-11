using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKind_SingletonPropertyTests
{
    [Fact]
    public void 数値の確認_正しい値が取得できる()
    {
        // Arrange & Act & Assert
        Assert.Equal(0, TileKind.Man1.Value);
        Assert.Equal(1, TileKind.Man2.Value);
        Assert.Equal(2, TileKind.Man3.Value);
        Assert.Equal(3, TileKind.Man4.Value);
        Assert.Equal(4, TileKind.Man5.Value);
        Assert.Equal(5, TileKind.Man6.Value);
        Assert.Equal(6, TileKind.Man7.Value);
        Assert.Equal(7, TileKind.Man8.Value);
        Assert.Equal(8, TileKind.Man9.Value);
        Assert.Equal(9, TileKind.Pin1.Value);
        Assert.Equal(10, TileKind.Pin2.Value);
        Assert.Equal(11, TileKind.Pin3.Value);
        Assert.Equal(12, TileKind.Pin4.Value);
        Assert.Equal(13, TileKind.Pin5.Value);
        Assert.Equal(14, TileKind.Pin6.Value);
        Assert.Equal(15, TileKind.Pin7.Value);
        Assert.Equal(16, TileKind.Pin8.Value);
        Assert.Equal(17, TileKind.Pin9.Value);
        Assert.Equal(18, TileKind.Sou1.Value);
        Assert.Equal(19, TileKind.Sou2.Value);
        Assert.Equal(20, TileKind.Sou3.Value);
        Assert.Equal(21, TileKind.Sou4.Value);
        Assert.Equal(22, TileKind.Sou5.Value);
        Assert.Equal(23, TileKind.Sou6.Value);
        Assert.Equal(24, TileKind.Sou7.Value);
        Assert.Equal(25, TileKind.Sou8.Value);
        Assert.Equal(26, TileKind.Sou9.Value);
        Assert.Equal(27, TileKind.Ton.Value);
        Assert.Equal(28, TileKind.Nan.Value);
        Assert.Equal(29, TileKind.Sha.Value);
        Assert.Equal(30, TileKind.Pei.Value);
        Assert.Equal(31, TileKind.Haku.Value);
        Assert.Equal(32, TileKind.Hatsu.Value);
        Assert.Equal(33, TileKind.Chun.Value);
    }
}
