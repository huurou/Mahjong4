using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKind_TryGetAtDistanceTests
{
    [Theory]
    [InlineData(0, 1, true, 1)]   // Man1 + 1 → Man2
    [InlineData(0, 2, true, 2)]   // Man1 + 2 → Man3
    [InlineData(8, -1, true, 7)]  // Man9 - 1 → Man8
    [InlineData(4, -4, true, 0)]  // Man5 - 4 → Man1
    [InlineData(9, 1, true, 10)]  // Pin1 + 1 → Pin2
    [InlineData(17, -1, true, 16)] // Pin9 - 1 → Pin8
    [InlineData(18, 3, true, 21)] // Sou1 + 3 → Sou4
    [InlineData(26, -5, true, 21)] // Sou9 - 5 → Sou4
    public void 同じスート内での距離計算_成功する(int baseValue, int distance, bool expectedResult, int expectedValue)
    {
        // Arrange
        var baseTile = new TileKind(baseValue);

        // Act
        var result = baseTile.TryGetAtDistance(distance, out var resultTile);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.NotNull(resultTile);
            Assert.Equal(expectedValue, resultTile.Value);
        }
        else
        {
            Assert.Null(resultTile);
        }
    }

    [Theory]
    [InlineData(0, 9)]    // Man1 + 9 → Pin1 (異なるスート)
    [InlineData(0, 18)]   // Man1 + 18 → Sou1 (異なるスート)
    [InlineData(8, 1)]    // Man9 + 1 → Pin1 (異なるスート)
    [InlineData(17, 1)]   // Pin9 + 1 → Sou1 (異なるスート)
    [InlineData(26, 1)]   // Sou9 + 1 → 範囲外
    [InlineData(0, -1)]   // Man1 - 1 → 範囲外
    [InlineData(9, -1)]   // Pin1 - 1 → Man9 (異なるスート)
    [InlineData(18, -1)]  // Sou1 - 1 → Pin9 (異なるスート)
    public void 異なるスートまたは範囲外への距離計算_失敗する(int baseValue, int distance)
    {
        // Arrange
        var baseTile = new TileKind(baseValue);

        // Act
        var result = baseTile.TryGetAtDistance(distance, out var resultTile);

        // Assert
        Assert.False(result);
        Assert.Null(resultTile);
    }

    [Theory]
    [InlineData(27, 1)]   // 東 + 1
    [InlineData(28, -1)]  // 南 - 1
    [InlineData(29, 2)]   // 西 + 2
    [InlineData(30, -3)]  // 北 - 3
    [InlineData(31, 1)]   // 白 + 1
    [InlineData(32, -1)]  // 發 - 1
    [InlineData(33, 0)]   // 中 + 0
    public void 字牌での距離計算_失敗する(int baseValue, int distance)
    {
        // Arrange
        var baseTile = new TileKind(baseValue);

        // Act
        var result = baseTile.TryGetAtDistance(distance, out var resultTile);

        // Assert
        Assert.False(result);
        Assert.Null(resultTile);
    }

    [Theory]
    [InlineData(0, 0, true, 0)]   // Man1 + 0 → Man1
    [InlineData(4, 0, true, 4)]   // Man5 + 0 → Man5
    [InlineData(13, 0, true, 13)] // Pin5 + 0 → Pin5
    [InlineData(22, 0, true, 22)] // Sou5 + 0 → Sou5
    public void 距離0での計算_同じ牌が返される(int baseValue, int distance, bool expectedResult, int expectedValue)
    {
        // Arrange
        var baseTile = new TileKind(baseValue);

        // Act
        var result = baseTile.TryGetAtDistance(distance, out var resultTile);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.NotNull(resultTile);
            Assert.Equal(expectedValue, resultTile.Value);
        }
        else
        {
            Assert.Null(resultTile);
        }
    }
}
