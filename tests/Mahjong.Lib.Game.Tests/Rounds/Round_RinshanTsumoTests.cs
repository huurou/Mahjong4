using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_RinshanTsumoTests
{
    [Fact]
    public void RinshanTsumo_嶺上からyama1の牌が手牌に追加される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0).Haipai();
        var initialCount = round.HandArray[round.Turn].Count();

        // Act
        var result = round.RinshanTsumo();

        // Assert
        Assert.Contains(new Tile(1), result.HandArray[round.Turn]);
        Assert.Equal(initialCount + 1, result.HandArray[round.Turn].Count());
        Assert.Equal(1, result.Wall.RinshanDrawnCount);
    }
}
