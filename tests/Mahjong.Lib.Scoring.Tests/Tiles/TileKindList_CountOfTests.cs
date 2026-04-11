using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindList_CountOfTests
{
    [Fact]
    public void 指定牌の個数を取得_正しい個数が返される()
    {
        // Arrange
        var list = new TileKindList(man: "111223");

        // Act & Assert
        Assert.Equal(3, list.CountOf(TileKind.Man1));
        Assert.Equal(2, list.CountOf(TileKind.Man2));
        Assert.Equal(1, list.CountOf(TileKind.Man3));
        Assert.Equal(0, list.CountOf(TileKind.Pin1));
    }
}
