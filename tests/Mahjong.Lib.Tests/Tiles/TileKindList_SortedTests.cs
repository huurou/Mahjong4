using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindList_SortedTests
{
    [Fact]
    public void ソート済みリストを取得_正しい順序で返される()
    {
        // Arrange
        var list = new TileKindList([TileKind.Man3, TileKind.Man1, TileKind.Man2]);

        // Act
        var sortedList = list.Sorted();

        // Assert
        Assert.Equal(TileKind.Man1, sortedList[0]);
        Assert.Equal(TileKind.Man2, sortedList[1]);
        Assert.Equal(TileKind.Man3, sortedList[2]);
        // 元インスタンスは変化しない
        Assert.Equal(TileKind.Man3, list[0]);
        Assert.Equal(TileKind.Man1, list[1]);
        Assert.Equal(TileKind.Man2, list[2]);
    }
}
