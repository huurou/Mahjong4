using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindList_RemoveTests
{
    [Fact]
    public void 存在する牌を削除_最初の1つだけ削除される()
    {
        // Arrange
        var originalList = new TileKindList(man: "1123");

        // Act
        var newList = originalList.Remove(TileKind.Man1);

        // Assert
        Assert.Equal(4, originalList.Count);
        Assert.Equal(3, newList.Count);
        Assert.Equal(TileKind.Man1, newList[0]);
        Assert.Equal(TileKind.Man2, newList[1]);
        Assert.Equal(TileKind.Man3, newList[2]);
    }

    [Fact]
    public void 存在しない牌を削除_ArgumentException発生()
    {
        // Arrange
        var list = new TileKindList(man: "123");

        // Act
        var ex = Record.Exception(() => list.Remove(TileKind.Pin1));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Equal("tileKind", ((ArgumentException)ex).ParamName);
    }

    [Fact]
    public void 複数個削除_指定個数だけ削除される()
    {
        // Arrange
        var originalList = new TileKindList(man: "111223");

        // Act
        var newList = originalList.Remove(TileKind.Man1, 2);

        // Assert
        Assert.Equal(6, originalList.Count);
        Assert.Equal(4, newList.Count);
        Assert.Equal(1, newList.CountOf(TileKind.Man1));
        Assert.Equal(2, newList.CountOf(TileKind.Man2));
        Assert.Equal(1, newList.CountOf(TileKind.Man3));
    }

    [Fact]
    public void IEnumerableで複数削除_全て削除される()
    {
        // Arrange
        var originalList = new TileKindList(man: "12345");
        var tilesToRemove = new[] { TileKind.Man1, TileKind.Man3, TileKind.Man5 };

        // Act
        var newList = originalList.Remove(tilesToRemove);

        // Assert
        Assert.Equal(5, originalList.Count);
        Assert.Equal(2, newList.Count);
        Assert.Equal(TileKind.Man2, newList[0]);
        Assert.Equal(TileKind.Man4, newList[1]);
    }

    [Fact]
    public void IEnumerableで存在しない牌を含む削除_ArgumentException発生()
    {
        // Arrange
        var list = new TileKindList(man: "123");
        var tilesToRemove = new[] { TileKind.Man1, TileKind.Pin1 };

        // Act
        var ex = Record.Exception(() => list.Remove(tilesToRemove));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Equal("tileKinds", ((ArgumentException)ex).ParamName);
    }
}
