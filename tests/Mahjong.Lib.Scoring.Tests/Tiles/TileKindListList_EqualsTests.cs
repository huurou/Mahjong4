using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindListList_EqualsTests
{
    [Fact]
    public void 同じ内容のリスト_Trueを返す()
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
        Assert.True(list1.Equals(list2));
        Assert.Equal(list1, list2);
    }

    [Fact]
    public void 順序が異なるが内容が同じリスト_Trueを返す()
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
        Assert.True(list1.Equals(list2));
        Assert.Equal(list1, list2);
    }

    [Fact]
    public void 異なる内容のリスト_Falseを返す()
    {
        // Arrange
        var list1 = new TileKindListList([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        ]);
        var list2 = new TileKindListList([
            new TileKindList(man: "123"),
            new TileKindList(sou: "789"),
        ]);

        // Act & Assert
        Assert.False(list1.Equals(list2));
        Assert.NotEqual(list1, list2);
    }

    [Fact]
    public void Null_Falseを返す()
    {
        // Arrange
        var list = new TileKindListList([new TileKindList(man: "123")]);

        // Act & Assert
        Assert.False(list.Equals(null));
    }

    [Fact]
    public void 同一参照_Trueを返す()
    {
        // Arrange
        var list = new TileKindListList([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        ]);

        // Act & Assert
        Assert.True(list.Equals(list));
    }

    [Fact]
    public void Object型で同じ内容_Trueを返す()
    {
        // Arrange
        var list1 = new TileKindListList([new TileKindList(man: "123")]);
        object list2 = new TileKindListList([new TileKindList(man: "123")]);

        // Act & Assert
        Assert.True(list1.Equals(list2));
    }

    [Fact]
    public void Object型で異なる型_Falseを返す()
    {
        // Arrange
        var list = new TileKindListList([new TileKindList(man: "123")]);
        object other = "not a TileKindListList";

        // Act & Assert
        Assert.False(list.Equals(other));
    }

    [Fact]
    public void Object型でnull_Falseを返す()
    {
        // Arrange
        var list = new TileKindListList([new TileKindList(man: "123")]);

        // Act & Assert
        Assert.False(list.Equals((object?)null));
    }
}
