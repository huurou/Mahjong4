using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindList_EqualsTests
{
    [Fact]
    public void 同じ内容のリスト_Trueを返す()
    {
        // Arrange
        var list1 = new TileKindList(man: "123");
        var list2 = new TileKindList(man: "123");

        // Act & Assert
        Assert.True(list1.Equals(list2));
        Assert.Equal(list1, list2);
    }

    [Fact]
    public void 異なる内容のリスト_Falseを返す()
    {
        // Arrange
        var list1 = new TileKindList(man: "123");
        var list2 = new TileKindList(man: "124");

        // Act & Assert
        Assert.False(list1.Equals(list2));
        Assert.NotEqual(list1, list2);
    }

    [Fact]
    public void 同じ要素で異なる順序_Trueを返す()
    {
        // Arrange
        var list1 = new TileKindList([TileKind.Man1, TileKind.Man2]);
        var list2 = new TileKindList([TileKind.Man2, TileKind.Man1]);

        // Act & Assert
        Assert.True(list1.Equals(list2));
        Assert.Equal(list1, list2);
    }

    [Fact]
    public void Null_Falseを返す()
    {
        // Arrange
        var list = new TileKindList(man: "123");

        // Act & Assert
        Assert.False(list.Equals(null));
    }
}
