using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_TsumoTests
{
    [Fact]
    public void 配牌後のTsumo_Turnの手牌が十四枚になる()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0).Haipai();

        // Act
        var result = round.Tsumo();

        // Assert
        Assert.Equal(14, result.HandArray[result.Turn].Count());
    }

    [Fact]
    public void 配牌後のTsumo_yama83の牌が手牌に追加される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0).Haipai();

        // Act
        var result = round.Tsumo();

        // Assert
        Assert.Contains(new Tile(83), result.HandArray[result.Turn]);
    }

    [Fact]
    public void 配牌後のTsumo_山のDrawnCountが五十三()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0).Haipai();

        // Act
        var result = round.Tsumo();

        // Assert
        Assert.Equal(53, result.Wall.DrawnCount);
    }
}
