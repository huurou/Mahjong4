using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindListList_CompareToTests
{
    [Fact]
    public void 等しいリスト_0を返す()
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
        Assert.Equal(0, list1.CompareTo(list2));
    }

    [Fact]
    public void 小さいリスト_負の値を返す()
    {
        // Arrange
        var smallerList = new TileKindListList([new TileKindList(man: "122")]);
        var largerList = new TileKindListList([new TileKindList(man: "123")]);

        // Act & Assert
        Assert.True(smallerList.CompareTo(largerList) < 0);
    }

    [Fact]
    public void 大きいリスト_正の値を返す()
    {
        // Arrange
        var smallerList = new TileKindListList([new TileKindList(man: "122")]);
        var largerList = new TileKindListList([new TileKindList(man: "123")]);

        // Act & Assert
        Assert.True(largerList.CompareTo(smallerList) > 0);
    }

    [Fact]
    public void Null_正の値を返す()
    {
        // Arrange
        var list = new TileKindListList([new TileKindList(man: "123")]);

        // Act & Assert
        Assert.Equal(1, list.CompareTo(null));
    }

    [Fact]
    public void 異なる長さのリスト_正しい結果を返す()
    {
        // Arrange
        var shortList = new TileKindListList([new TileKindList(man: "123")]);
        var longList = new TileKindListList([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        ]);

        // Act & Assert
        Assert.True(shortList.CompareTo(longList) < 0);
        Assert.True(longList.CompareTo(shortList) > 0);
    }

    [Fact]
    public void 空のリスト同士_0を返す()
    {
        // Arrange
        var list1 = new TileKindListList();
        var list2 = new TileKindListList();

        // Act & Assert
        Assert.Equal(0, list1.CompareTo(list2));
    }

    [Fact]
    public void 空のリストと非空のリスト_負の値を返す()
    {
        // Arrange
        var emptyList = new TileKindListList();
        var nonEmptyList = new TileKindListList([new TileKindList(man: "123")]);

        // Act & Assert
        Assert.True(emptyList.CompareTo(nonEmptyList) < 0);
        Assert.True(nonEmptyList.CompareTo(emptyList) > 0);
    }
}
