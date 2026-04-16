using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tests.Games;
using Mahjong.Lib.Game.Tests.States.RoundStates;
using Moq;

namespace Mahjong.Lib.Game.Tests.States.GameStates;

public class GameStateContext_IntegrationTests : IDisposable
{
    private readonly GameStateContext context_ = new(GamesTestHelper.CreateWallGenerator(), GamesTestHelper.CreateNoOpScoreCalculator(), GamesTestHelper.CreateNoOpTenpaiChecker());

    public void Dispose()
    {
        context_.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SingleRound_流局でGameEndに到達する()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.SingleRound };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        context_.Init(game);

        // Act
        await context_.ResponseOkAsync();
        var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previous: null);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
        await roundStateContext.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
        await RoundStateContextTestHelper.ResponseKouhaiHeikyokuAsync(roundStateContext);
        await RoundStateContextTestHelper.AcknowledgeRoundEndAsync<RoundStateRyuukyoku>(roundStateContext);
        await GameStateContextTestHelper.WaitForStateAsync<GameStateEnd>(context_);

        // Assert
        Assert.IsType<GameStateEnd>(context_.State);
    }

    [Fact]
    public async Task Tonpuu_4局消化でGameEndに到達する()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.Tonpuu };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        context_.Init(game);

        // Act
        await context_.ResponseOkAsync();
        RoundStateContext? previousRoundStateContext = null;
        for (var i = 0; i < 4; i++)
        {
            var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previousRoundStateContext);
            previousRoundStateContext = roundStateContext;
            await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
            await roundStateContext.ResponseOkAsync();
            await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
            await RoundStateContextTestHelper.ResponseKouhaiHeikyokuAsync(roundStateContext);
            await RoundStateContextTestHelper.AcknowledgeRoundEndAsync<RoundStateRyuukyoku>(roundStateContext);
        }
        await GameStateContextTestHelper.WaitForStateAsync<GameStateEnd>(context_);

        // Assert
        Assert.IsType<GameStateEnd>(context_.State);
    }

    [Fact]
    public async Task 子ロン和了_放銃者から子に点数移動し次局に進む()
    {
        // Arrange
        var scoreMock = new Mock<IScoreCalculator>();
        scoreMock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(new ScoreResult(0, 0,
                new PointArray(new Point(0))
                    .AddPoint(new PlayerIndex(1), 8000)
                    .SubtractPoint(new PlayerIndex(0), 8000)));
        using var context = new GameStateContext(GamesTestHelper.CreateWallGenerator(), scoreMock.Object, GamesTestHelper.CreateNoOpTenpaiChecker());
        var rules = new GameRules { Format = GameFormat.Tonpuu };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        context.Init(game);
        var initialPoints = rules.InitialPoints;

        // Act
        await context.ResponseOkAsync();
        var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context, previous: null);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
        await roundStateContext.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
        // 親 (index 0) が打牌 → 子 (index 1) がロン
        var tile = RoundStateContextTestHelper.PickTileToDahai(roundStateContext);
        await roundStateContext.ResponseDahaiAsync(tile);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateDahai>(roundStateContext);
        await roundStateContext.ResponseWinAsync(
            [new PlayerIndex(1)], new PlayerIndex(0), WinType.Ron);
        await RoundStateContextTestHelper.AcknowledgeRoundEndAsync<RoundStateWin>(roundStateContext);

        await GameStateContextTestHelper.WaitForNewRoundContextAsync(context, roundStateContext);

        // Assert: 注入した ScoreCalculator のデルタが Round → Game に反映
        Assert.Equal(initialPoints + 8000, context.Game.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(initialPoints - 8000, context.Game.PointArray[new PlayerIndex(0)].Value);
        Assert.Equal(initialPoints, context.Game.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(initialPoints, context.Game.PointArray[new PlayerIndex(3)].Value);
        // 子和了なので親流れ (東二局)、本場リセット
        Assert.Equal(1, context.Game.RoundNumber.Value);
        Assert.Equal(0, context.Game.Honba.Value);
    }

    [Fact]
    public async Task 親ツモ和了で連荘しHonbaが1増える()
    {
        // Arrange
        var rules = new GameRules { Format = GameFormat.Tonpuu };
        var game = GameAggregate.Create(GamesTestHelper.CreatePlayerList(), rules);
        context_.Init(game);

        // Act
        await context_.ResponseOkAsync();
        var roundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, previous: null);
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateHaipai>(roundStateContext);
        await roundStateContext.ResponseOkAsync();
        await RoundStateContextTestHelper.WaitForStateAsync<RoundStateTsumo>(roundStateContext);
        // 親ツモ和了 (Turn=PlayerIndex(0) のまま ResponseWinAsync)
        await RoundStateContextTestHelper.ResponseTsumoWinAsync(roundStateContext);
        await RoundStateContextTestHelper.AcknowledgeRoundEndAsync<RoundStateWin>(roundStateContext);

        // 次局開始まで待つ (2回目の RoundStateContext が生成されるまで)
        var nextRoundStateContext = await GameStateContextTestHelper.WaitForNewRoundContextAsync(context_, roundStateContext);

        // Assert
        Assert.Equal(1, context_.Game.Honba.Value);
        Assert.Equal(0, context_.Game.RoundNumber.Value);    // 連荘で東一局のまま
        Assert.NotSame(roundStateContext, nextRoundStateContext);
    }
}
