using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.GameStates.Impl;

namespace Mahjong.Lib.Game.Tests.Games;

public class GameEndPolicy_ShouldEndAfterRoundTests
{
    private static GameEventRoundEndedByWin CreateWinEvent(PlayerIndex winner, PlayerIndex loser, WinType winType)
    {
        return new GameEventRoundEndedByWin(
            [winner],
            loser,
            winType,
            [],
            new Honba(0),
            new KyoutakuRiichiAward(winner, 0),
            []
        );
    }

    private static GameEventRoundEndedByRyuukyoku CreateRyuukyokuEvent(RyuukyokuType type)
    {
        return new GameEventRoundEndedByRyuukyoku(type, [], [], new PointArray(new Point(0)));
    }

    [Fact]
    public void トビ発生_trueを返す()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules()) with
        {
            PointArray = new PointArray(new Point(25000)).SubtractPoint(new PlayerIndex(0), 30000),
        };
        var evt = CreateRyuukyokuEvent(RyuukyokuType.KouhaiHeikyoku);

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
        var evt = CreateRyuukyokuEvent(RyuukyokuType.KouhaiHeikyoku);

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
        var evt = CreateRyuukyokuEvent(RyuukyokuType.KouhaiHeikyoku);

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
        var evt = CreateRyuukyokuEvent(RyuukyokuType.KouhaiHeikyoku);

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
        var evt = CreateWinEvent(new PlayerIndex(3), new PlayerIndex(3), WinType.Tsumo);

        // Act
        var result = GameEndPolicy.ShouldEndAfterRound(game, evt, dealerContinues: true);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void オーラス親1位単独で和了_trueを返す()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules { Format = GameFormat.Tonpuu });
        for (var i = 0; i < 3; i++)
        {
            game = game.AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);
        }
        // 東4局。親 (PlayerIndex 3) が40000点でトップ、他は20000点
        game = game with
        {
            PointArray = new PointArray(new Point(20000))
                .AddPoint(new PlayerIndex(3), 20000),
        };
        var evt = CreateWinEvent(new PlayerIndex(3), new PlayerIndex(3), WinType.Tsumo);

        // Act
        var result = GameEndPolicy.ShouldEndAfterRound(game, evt, dealerContinues: true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void オーラス親和了でも2位と同点_falseを返す()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules { Format = GameFormat.Tonpuu });
        for (var i = 0; i < 3; i++)
        {
            game = game.AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);
        }
        // 親 (PlayerIndex 3) と PlayerIndex 0 が共に 30000 点 (同点トップ)
        game = game with
        {
            PointArray = new PointArray(new Point(20000))
                .AddPoint(new PlayerIndex(0), 10000)
                .AddPoint(new PlayerIndex(3), 10000),
        };
        var evt = CreateWinEvent(new PlayerIndex(3), new PlayerIndex(3), WinType.Tsumo);

        // Act
        var result = GameEndPolicy.ShouldEndAfterRound(game, evt, dealerContinues: true);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void オーラス子和了_falseを返す()
    {
        // Arrange
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), new GameRules { Format = GameFormat.Tonpuu });
        for (var i = 0; i < 3; i++)
        {
            game = game.AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);
        }
        // 子 (PlayerIndex 0) が1位だが子なので止まらない (ただし規定局数消化で true になる可能性あり)
        // ここでは dealerContinues:false かつ 子和了 → 規定局数消化で true になる
        // オーラス止め判定単独を見たいので、dealerContinues:true にして規定局数消化を回避する
        game = game with
        {
            PointArray = new PointArray(new Point(20000))
                .AddPoint(new PlayerIndex(0), 30000),
        };
        var evt = CreateWinEvent(new PlayerIndex(0), new PlayerIndex(3), WinType.Ron);

        // Act: dealerContinues=true (続行判定側) なので、ここで true が返るならオーラス止め由来
        var result = GameEndPolicy.ShouldEndAfterRound(game, evt, dealerContinues: true);

        // Assert: 子和了のためオーラス止めは発動せず false
        Assert.False(result);
    }

    [Fact]
    public void DealerWinStopAtAllLastFalse_親1位でも続行する()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.Tonpuu, DealerWinStopAtAllLast = false };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        for (var i = 0; i < 3; i++)
        {
            game = game.AdvanceToNextRound(RoundAdvanceMode.DealerChangeResetHonba);
        }
        game = game with
        {
            PointArray = new PointArray(new Point(20000))
                .AddPoint(new PlayerIndex(3), 20000),
        };
        var evt = CreateWinEvent(new PlayerIndex(3), new PlayerIndex(3), WinType.Tsumo);

        // Act: dealerContinues=true で規定局数消化による終了を回避し、オーラス止め単独を見る
        var result = GameEndPolicy.ShouldEndAfterRound(game, evt, dealerContinues: true);

        // Assert: DealerWinStopAtAllLast が false なら止まらない
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
        var evt = CreateRyuukyokuEvent(RyuukyokuType.KouhaiHeikyoku);

        // Act
        var result = GameEndPolicy.ShouldEndAfterRound(game, evt, dealerContinues: false);

        // Assert
        Assert.True(result);
    }
}
