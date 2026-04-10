using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindList_AddTests
{
    [Fact]
    public void 牌を追加_新しいリストが返される()
    {
        // Arrange
        var originalList = new TileKindList(man: "12");

        // Act
        var newList = originalList.Add(TileKind.Man3);

        // Assert
        Assert.Equal(2, originalList.Count);
        Assert.Equal(3, newList.Count);
        Assert.Equal(TileKind.Man3, newList[2]);
    }
}
