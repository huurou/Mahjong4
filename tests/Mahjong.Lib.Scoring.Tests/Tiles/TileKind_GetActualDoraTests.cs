using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKind_GetActualDoraTests
{
    [Theory]
    [InlineData(0, 1)]   // Man1 → Man2
    [InlineData(1, 2)]   // Man2 → Man3
    [InlineData(4, 5)]   // Man5 → Man6
    [InlineData(9, 10)]  // Pin1 → Pin2
    [InlineData(13, 14)] // Pin5 → Pin6
    [InlineData(18, 19)] // Sou1 → Sou2
    [InlineData(21, 22)] // Sou4 → Sou5
    public void 数牌_次の牌が返される(int indicatorValue, int expectedValue)
    {
        // Arrange
        var indicator = new TileKind(indicatorValue);

        // Act
        var actual = TileKind.GetActualDora(indicator);

        // Assert
        Assert.Equal(expectedValue, actual.Value);
    }

    [Theory]
    [InlineData(8, 0)]   // Man9 → Man1
    [InlineData(17, 9)]  // Pin9 → Pin1
    [InlineData(26, 18)] // Sou9 → Sou1
    public void 数牌の9_同スートの1に戻る(int indicatorValue, int expectedValue)
    {
        // Arrange
        var indicator = new TileKind(indicatorValue);

        // Act
        var actual = TileKind.GetActualDora(indicator);

        // Assert
        Assert.Equal(expectedValue, actual.Value);
    }

    [Theory]
    [InlineData(27, 28)] // 東 → 南
    [InlineData(28, 29)] // 南 → 西
    [InlineData(29, 30)] // 西 → 北
    [InlineData(30, 27)] // 北 → 東
    public void 風牌_次の風に循環する(int indicatorValue, int expectedValue)
    {
        // Arrange
        var indicator = new TileKind(indicatorValue);

        // Act
        var actual = TileKind.GetActualDora(indicator);

        // Assert
        Assert.Equal(expectedValue, actual.Value);
    }

    [Theory]
    [InlineData(31, 32)] // 白 → 發
    [InlineData(32, 33)] // 發 → 中
    [InlineData(33, 31)] // 中 → 白
    public void 三元牌_次の三元牌に循環する(int indicatorValue, int expectedValue)
    {
        // Arrange
        var indicator = new TileKind(indicatorValue);

        // Act
        var actual = TileKind.GetActualDora(indicator);

        // Assert
        Assert.Equal(expectedValue, actual.Value);
    }
}
