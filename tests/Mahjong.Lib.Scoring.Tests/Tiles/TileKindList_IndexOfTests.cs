using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindList_IndexOfTests
{
    [Fact]
    public void 牌のインデックスを取得_正しいインデックスが返される()
    {
        // Arrange
        var list = new TileKindList(man: "123");

        // Act & Assert
        Assert.Equal(0, list.IndexOf(TileKind.Man1));
        Assert.Equal(1, list.IndexOf(TileKind.Man2));
        Assert.Equal(2, list.IndexOf(TileKind.Man3));
        Assert.Equal(-1, list.IndexOf(TileKind.Pin1));
    }
}
