using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_FirstTurnDrivingTests
{
    [Fact]
    public void Haipai直後_全員が第一打前()
    {
        // Arrange & Act
        var round = RoundTestHelper.CreateRound().Haipai();

        // Assert
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            Assert.True(round.PlayerRoundStatusArray[new PlayerIndex(i)].IsFirstTurnBeforeDiscard);
        }
    }

    [Fact]
    public void 打牌_その手番だけ第一打前falseになる()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var playerIndex = round.Turn;

        // Act
        var result = round.Dahai(round.HandArray[playerIndex].Last(), RoundTestHelper.NoOpTenpaiChecker);

        // Assert
        Assert.False(result.PlayerRoundStatusArray[playerIndex].IsFirstTurnBeforeDiscard);
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            if (i == playerIndex.Value) { continue; }
            Assert.True(result.PlayerRoundStatusArray[new PlayerIndex(i)].IsFirstTurnBeforeDiscard);
        }
    }

    [Fact]
    public void 鳴き発生_全員が第一打前falseになる()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        round = round.Dahai(new Tile(83), RoundTestHelper.NoOpTenpaiChecker);
        var caller = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, caller,
        [
            new Tile(84), new Tile(88),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16),
        ]);

        // Act
        var result = round.Chi(caller, ImmutableList.Create(new Tile(84), new Tile(88)));

        // Assert: 全員 false
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            Assert.False(result.PlayerRoundStatusArray[new PlayerIndex(i)].IsFirstTurnBeforeDiscard);
        }
    }
}
