using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindList_IndexerTests
{
    [Fact]
    public void 有効なインデックス_正しい牌を返す()
    {
        // Arrange
        var list = new TileKindList(man: "123");

        // Act & Assert
        Assert.Equal(TileKind.Man1, list[0]);
        Assert.Equal(TileKind.Man2, list[1]);
        Assert.Equal(TileKind.Man3, list[2]);
    }
}
