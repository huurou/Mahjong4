using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindListList_ToStringTests
{
    [Fact]
    public void 文字列表現_正しい文字列を返す()
    {
        // Arrange
        var list = new TileKindListList([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        ]);

        // Act
        var result = list.ToString();

        // Assert
        Assert.Equal("[一二三][(4)(5)(6)]", result);
    }

    [Fact]
    public void 空のリスト_空文字列を返す()
    {
        // Arrange
        var list = new TileKindListList();

        // Act
        var result = list.ToString();

        // Assert
        Assert.Equal("", result);
    }
}
