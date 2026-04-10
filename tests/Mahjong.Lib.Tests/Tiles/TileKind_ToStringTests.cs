using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKind_ToStringTests
{
    [Theory]
    [InlineData(0, "一")]
    [InlineData(1, "二")]
    [InlineData(2, "三")]
    [InlineData(3, "四")]
    [InlineData(4, "五")]
    [InlineData(5, "六")]
    [InlineData(6, "七")]
    [InlineData(7, "八")]
    [InlineData(8, "九")]
    public void 萬子_漢数字で表示される(int value, string expected)
    {
        // Arrange
        var tile = new TileKind(value);

        // Act
        var actual = tile.ToString();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(9, "(1)")]
    [InlineData(10, "(2)")]
    [InlineData(11, "(3)")]
    [InlineData(12, "(4)")]
    [InlineData(13, "(5)")]
    [InlineData(14, "(6)")]
    [InlineData(15, "(7)")]
    [InlineData(16, "(8)")]
    [InlineData(17, "(9)")]
    public void 筒子_括弧付き数字で表示される(int value, string expected)
    {
        // Arrange
        var tile = new TileKind(value);

        // Act
        var actual = tile.ToString();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(18, "1")]
    [InlineData(19, "2")]
    [InlineData(20, "3")]
    [InlineData(21, "4")]
    [InlineData(22, "5")]
    [InlineData(23, "6")]
    [InlineData(24, "7")]
    [InlineData(25, "8")]
    [InlineData(26, "9")]
    public void 索子_数字で表示される(int value, string expected)
    {
        // Arrange
        var tile = new TileKind(value);

        // Act
        var actual = tile.ToString();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(27, "東")]
    [InlineData(28, "南")]
    [InlineData(29, "西")]
    [InlineData(30, "北")]
    [InlineData(31, "白")]
    [InlineData(32, "發")]
    [InlineData(33, "中")]
    public void 字牌_漢字で表示される(int value, string expected)
    {
        // Arrange
        var tile = new TileKind(value);

        // Act
        var actual = tile.ToString();

        // Assert
        Assert.Equal(expected, actual);
    }
}
