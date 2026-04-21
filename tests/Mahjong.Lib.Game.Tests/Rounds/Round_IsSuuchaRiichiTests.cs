using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_IsSuuchaRiichiTests
{
    [Fact]
    public void 全員立直_true()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var statusArray = round.PlayerRoundStatusArray;
        for (var i = 0; i < 4; i++)
        {
            var index = new PlayerIndex(i);
            statusArray = statusArray.SetStatus(index, statusArray[index] with { IsRiichi = true });
        }
        round = round with { PlayerRoundStatusArray = statusArray };

        // Act
        var result = round.IsSuuchaRiichi();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void 三人立直_false()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var statusArray = round.PlayerRoundStatusArray;
        for (var i = 0; i < 3; i++)
        {
            var index = new PlayerIndex(i);
            statusArray = statusArray.SetStatus(index, statusArray[index] with { IsRiichi = true });
        }
        round = round with { PlayerRoundStatusArray = statusArray };

        // Act
        var result = round.IsSuuchaRiichi();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void 全員立直保留のみで未確定_false()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var statusArray = round.PlayerRoundStatusArray;
        for (var i = 0; i < 4; i++)
        {
            var index = new PlayerIndex(i);
            statusArray = statusArray.SetStatus(index, statusArray[index] with { IsPendingRiichi = true });
        }
        round = round with { PlayerRoundStatusArray = statusArray };

        // Act
        var result = round.IsSuuchaRiichi();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void 誰も立直していない_false()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();

        // Act
        var result = round.IsSuuchaRiichi();

        // Assert
        Assert.False(result);
    }
}
