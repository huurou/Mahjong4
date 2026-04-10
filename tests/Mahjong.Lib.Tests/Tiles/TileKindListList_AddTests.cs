using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindListList_AddTests
{
    [Fact]
    public void TileKindListを追加_新しいリストが返される()
    {
        // Arrange
        var originalList = new TileKindListList([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        ]);
        var newTileKindList = new TileKindList(sou: "789");

        // Act
        var newList = originalList.Add(newTileKindList);

        // Assert
        Assert.Equal(2, originalList.Count);
        Assert.Equal(3, newList.Count);
        Assert.Equal(newTileKindList, newList[2]);
    }
}
