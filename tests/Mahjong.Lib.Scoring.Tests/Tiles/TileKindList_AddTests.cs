using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindList_AddTests
{
    [Fact]
    public void 末尾に追加_ソート順が維持される()
    {
        // Arrange
        var originalList = new TileKindList(man: "12");

        // Act
        var newList = originalList.Add(TileKind.Man3);

        // Assert
        Assert.Equal(2, originalList.Count);
        Assert.Equal(3, newList.Count);
        Assert.Equal(TileKind.Man1, newList[0]);
        Assert.Equal(TileKind.Man2, newList[1]);
        Assert.Equal(TileKind.Man3, newList[2]);
    }

    [Fact]
    public void 先頭に追加_ソート順が維持される()
    {
        // Arrange
        var originalList = new TileKindList(man: "23");

        // Act
        var newList = originalList.Add(TileKind.Man1);

        // Assert
        Assert.Equal(3, newList.Count);
        Assert.Equal(TileKind.Man1, newList[0]);
        Assert.Equal(TileKind.Man2, newList[1]);
        Assert.Equal(TileKind.Man3, newList[2]);
    }

    [Fact]
    public void 中間に追加_ソート順が維持される()
    {
        // Arrange
        var originalList = new TileKindList(man: "13");

        // Act
        var newList = originalList.Add(TileKind.Man2);

        // Assert
        Assert.Equal(3, newList.Count);
        Assert.Equal(TileKind.Man1, newList[0]);
        Assert.Equal(TileKind.Man2, newList[1]);
        Assert.Equal(TileKind.Man3, newList[2]);
    }
}
