using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

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

    [Fact]
    public void 未ソートの牌を追加_ソート済みで返される()
    {
        // Arrange
        var originalList = new TileKindList(man: "5");
        var tilesToAdd = new[] { TileKind.Man1, TileKind.Man9, TileKind.Man3 };

        // Act
        var newList = originalList.AddRange(tilesToAdd);

        // Assert
        Assert.Equal(4, newList.Count);
        Assert.Equal(TileKind.Man1, newList[0]);
        Assert.Equal(TileKind.Man3, newList[1]);
        Assert.Equal(TileKind.Man5, newList[2]);
        Assert.Equal(TileKind.Man9, newList[3]);
    }
}
