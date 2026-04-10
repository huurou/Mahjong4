using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindListList_AddRangeTests
{
    [Fact]
    public void 複数のTileKindListを追加_新しいリストが返される()
    {
        // Arrange
        var originalList = new TileKindListList([new TileKindList(man: "123")]);
        var tilesToAdd = new[]
        {
            new TileKindList(pin: "456"),
            new TileKindList(sou: "789")
        };

        // Act
        var newList = originalList.AddRange(tilesToAdd);

        // Assert
        Assert.Equal(1, originalList.Count);
        Assert.Equal(3, newList.Count);
        Assert.Equal(new TileKindList(pin: "456"), newList[1]);
        Assert.Equal(new TileKindList(sou: "789"), newList[2]);
    }
}
