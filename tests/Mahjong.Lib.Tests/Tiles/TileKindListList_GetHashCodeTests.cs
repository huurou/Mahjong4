using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindListList_GetHashCodeTests
{
    [Fact]
    public void 同じ内容のリスト_同じハッシュ値を返す()
    {
        // Arrange
        var list1 = new TileKindListList([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        ]);
        var list2 = new TileKindListList([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        ]);

        // Act & Assert
        Assert.Equal(list1.GetHashCode(), list2.GetHashCode());
    }

    [Fact]
    public void 順序が異なるが内容が同じリスト_同じハッシュ値を返す()
    {
        // Arrange
        var list1 = new TileKindListList([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        ]);
        var list2 = new TileKindListList([
            new TileKindList(pin: "456"),
            new TileKindList(man: "123"),
        ]);

        // Act & Assert
        Assert.Equal(list1.GetHashCode(), list2.GetHashCode());
    }
}
