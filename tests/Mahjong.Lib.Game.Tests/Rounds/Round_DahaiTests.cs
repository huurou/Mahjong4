using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_DahaiTests
{
    [Fact]
    public void Dahai_手牌から牌が除かれて河に追加される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        var discarded = new Tile(83);

        // Act
        var result = round.Dahai(discarded);

        // Assert
        Assert.DoesNotContain(discarded, result.HandArray[round.Turn]);
        Assert.Contains(discarded, result.RiverArray[round.Turn]);
    }

    [Fact]
    public void Dahai後_手牌が十三枚()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        var discarded = round.HandArray[round.Turn].First();

        // Act
        var result = round.Dahai(discarded);

        // Assert
        Assert.Equal(13, result.HandArray[round.Turn].Count());
    }
}
