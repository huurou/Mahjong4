using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindList_CompareToTests
{
    [Fact]
    public void 等しいリスト_0を返す()
    {
        // Arrange
        var list1 = new TileKindList(man: "123");
        var list2 = new TileKindList(man: "123");

        // Act & Assert
        Assert.Equal(0, list1.CompareTo(list2));
    }

    [Fact]
    public void 小さいリスト_正の値を返す()
    {
        // Arrange
        var list1 = new TileKindList(man: "123");
        var list2 = new TileKindList(man: "122");

        // Act & Assert
        Assert.True(list1.CompareTo(list2) > 0);
    }

    [Fact]
    public void 大きいリスト_負の値を返す()
    {
        // Arrange
        var list1 = new TileKindList(man: "122");
        var list2 = new TileKindList(man: "123");

        // Act & Assert
        Assert.True(list1.CompareTo(list2) < 0);
    }

    [Fact]
    public void Null_正の値を返す()
    {
        // Arrange
        var list = new TileKindList(man: "123");

        // Act & Assert
        Assert.Equal(1, list.CompareTo(null));
    }

    [Fact]
    public void 異なる長さのリスト_長さで比較される()
    {
        // Arrange
        var shorter = new TileKindList(man: "12");
        var longer = new TileKindList(man: "123");

        // Act & Assert
        Assert.True(shorter.CompareTo(longer) < 0);
        Assert.True(longer.CompareTo(shorter) > 0);
    }
}
