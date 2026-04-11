using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindList_GetHashCodeTests
{
    [Fact]
    public void 同じ内容のリスト_同じハッシュ値を返す()
    {
        // Arrange
        var list1 = new TileKindList(man: "123");
        var list2 = new TileKindList(man: "123");

        // Act & Assert
        Assert.Equal(list1.GetHashCode(), list2.GetHashCode());
    }

    [Fact]
    public void 異なる内容のリスト_異なるハッシュ値を返す()
    {
        // Arrange
        var list1 = new TileKindList(man: "123");
        var list2 = new TileKindList(man: "124");

        // Act & Assert
        Assert.NotEqual(list1.GetHashCode(), list2.GetHashCode());
    }
}
