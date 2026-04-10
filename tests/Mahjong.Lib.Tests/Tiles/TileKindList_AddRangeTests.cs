using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindList_AddRangeTests
{
    [Fact]
    public void 複数牌を追加_新しいリストが返される()
    {
        // Arrange
        var originalList = new TileKindList(man: "12");
        var tilesToAdd = new[] { TileKind.Man3, TileKind.Man4 };

        // Act
        var newList = originalList.AddRange(tilesToAdd);

        // Assert
        Assert.Equal(2, originalList.Count);
        Assert.Equal(4, newList.Count);
        Assert.Equal(TileKind.Man3, newList[2]);
        Assert.Equal(TileKind.Man4, newList[3]);
    }
}
