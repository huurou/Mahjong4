using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_HaipaiTests
{
    [Fact]
    public void 東一局_配牌後_各プレイヤーの手牌が十三枚()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0);

        // Act
        var result = round.Haipai();

        // Assert
        Assert.Equal(13, result.HandArray[new PlayerIndex(0)].Count());
        Assert.Equal(13, result.HandArray[new PlayerIndex(1)].Count());
        Assert.Equal(13, result.HandArray[new PlayerIndex(2)].Count());
        Assert.Equal(13, result.HandArray[new PlayerIndex(3)].Count());
    }

    [Fact]
    public void 東一局_配牌後_山のDrawnCountが五十二()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0);

        // Act
        var result = round.Haipai();

        // Assert
        Assert.Equal(52, result.Wall.DrawnCount);
        Assert.Equal(70, result.Wall.LiveRemaining);
    }

    [Fact]
    public void 東一局_配牌後_親の手牌が所定の順序で配られる()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0);

        // Act
        var result = round.Haipai();

        // Assert
        int[] expectedIds = [135, 134, 133, 132, 119, 118, 117, 116, 103, 102, 101, 100, 87];
        var actualIds = result.HandArray[new PlayerIndex(0)].Select(x => x.Id).ToArray();
        Assert.Equal(expectedIds, actualIds);
    }

    [Fact]
    public void 東一局_配牌後_次家の手牌が所定の順序で配られる()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0);

        // Act
        var result = round.Haipai();

        // Assert
        int[] expectedIds = [131, 130, 129, 128, 115, 114, 113, 112, 99, 98, 97, 96, 86];
        var actualIds = result.HandArray[new PlayerIndex(1)].Select(x => x.Id).ToArray();
        Assert.Equal(expectedIds, actualIds);
    }

    [Fact]
    public void 東二局_親がPlayerIndex1_PlayerIndex1から配られる()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(1);

        // Act
        var result = round.Haipai();

        // Assert
        int[] expectedDealerIds = [135, 134, 133, 132, 119, 118, 117, 116, 103, 102, 101, 100, 87];
        var actualDealerIds = result.HandArray[new PlayerIndex(1)].Select(x => x.Id).ToArray();
        Assert.Equal(expectedDealerIds, actualDealerIds);
    }

    [Fact]
    public void 配牌後_Turnが親になる()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(2);

        // Act
        var result = round.Haipai();

        // Assert
        Assert.Equal(2, result.Turn.Value);
    }
}
