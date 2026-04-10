using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindListList_RemoveTests
{
    [Fact]
    public void 存在するTileKindListを削除_新しいリストが返される()
    {
        // Arrange
        var tileKindListToRemove = new TileKindList(pin: "456");
        var originalList = new TileKindListList([
            new TileKindList(man: "123"),
            tileKindListToRemove,
            new TileKindList(sou: "789"),
        ]);

        // Act
        var newList = originalList.Remove(tileKindListToRemove);

        // Assert
        Assert.Equal(3, originalList.Count);
        Assert.Equal(2, newList.Count);
        Assert.Equal(new TileKindList(man: "123"), newList[0]);
        Assert.Equal(new TileKindList(sou: "789"), newList[1]);
    }

    [Fact]
    public void 存在しないTileKindListを削除_ArgumentException発生()
    {
        // Arrange
        var originalList = new TileKindListList([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        ]);
        var nonExistentTileKindList = new TileKindList(sou: "789");

        // Act
        var ex = Record.Exception(() => originalList.Remove(nonExistentTileKindList));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Equal("tileKindList", ((ArgumentException)ex).ParamName);
    }
}
