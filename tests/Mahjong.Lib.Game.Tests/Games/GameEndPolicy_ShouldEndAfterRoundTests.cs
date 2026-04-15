using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.States.GameStates.Impl;

namespace Mahjong.Lib.Game.Tests.Games;

public class GameEndPolicy_ShouldEndAfterRoundTests
{
    [Fact]
    public void トビ発生_trueを返す()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules()) with
        {
            PointArray = new PointArray(new Point(25000)).SubtractPoint(new PlayerIndex(0), 30000),
        };
        var evt = new GameEventRoundEndedByRyuukyoku(RyuukyokuType.KouhaiHeikyoku);

        // Act
        var result = GameEndPolicy.ShouldEndAfterRound(game, evt, dealerContinues: false);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SingleRound_1局終了でtrueを返す()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules { Format = GameFormat.SingleRound });
        var evt = new GameEventRoundEndedByRyuukyoku(RyuukyokuType.KouhaiHeikyoku);

        // Act
        var result = GameEndPolicy.ShouldEndAfterRound(game, evt, dealerContinues: false);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Tonpuu_東1局終了で連荘しない場合でもfalseを返す()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules { Format = GameFormat.Tonpuu });
        var evt = new GameEventRoundEndedByRyuukyoku(RyuukyokuType.KouhaiHeikyoku);

        // Act
        var result = GameEndPolicy.ShouldEndAfterRound(game, evt, dealerContinues: false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Tonpuu_東4局終了で親流れの場合trueを返す()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules { Format = GameFormat.Tonpuu })
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba)    // 東二局
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba)    // 東三局
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);   // 東四局
        var evt = new GameEventRoundEndedByRyuukyoku(RyuukyokuType.KouhaiHeikyoku);

        // Act
        var result = GameEndPolicy.ShouldEndAfterRound(game, evt, dealerContinues: false);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Tonpuu_東4局終了でも連荘中はfalseを返す()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules { Format = GameFormat.Tonpuu })
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba)
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba)
            .AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);
        var evt = new GameEventRoundEndedByWin(Winners: [new PlayerIndex(3)]);

        // Act
        var result = GameEndPolicy.ShouldEndAfterRound(game, evt, dealerContinues: true);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Tonnan_南4局終了で親流れの場合trueを返す()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules { Format = GameFormat.Tonnan });
        for (var i = 0; i < 7; i++)
        {
            game = game.AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);
        }
        // 南4局相当
        var evt = new GameEventRoundEndedByRyuukyoku(RyuukyokuType.KouhaiHeikyoku);

        // Act
        var result = GameEndPolicy.ShouldEndAfterRound(game, evt, dealerContinues: false);

        // Assert
        Assert.True(result);
    }
}
