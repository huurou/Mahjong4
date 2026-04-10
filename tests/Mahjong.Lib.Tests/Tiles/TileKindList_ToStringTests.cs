using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindList_ToStringTests
{
    [Fact]
    public void 文字列表現_正しい文字列を返す()
    {
        // Arrange
        var list = new TileKindList(man: "123");

        // Act
        var result = list.ToString();

        // Assert
        Assert.Equal("一二三", result);
    }
}
