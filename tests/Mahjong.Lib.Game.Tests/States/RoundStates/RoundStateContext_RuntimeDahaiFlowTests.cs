using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tests.Rounds;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateContext_RuntimeDahaiFlowTests
{
    [Fact]
    public async Task 他家がRonResponse_放銃者と和了者が正しく設定される()
    {
        // Arrange: 親の第1打 Tile(135) kind 33 を player[2] がロンする想定
        var tenpaiMock = new Mock<ITenpaiChecker>();
        tenpaiMock.Setup(x => x.IsTenpai(It.IsAny<Hand>(), It.IsAny<CallList>())).Returns(false);
        tenpaiMock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([33]);

        var players = new FakePlayer[]
        {
            FakePlayer.Create(0),
            FakePlayer.Create(1),
            new(PlayerId.NewId(), "F2", new PlayerIndex(2)) { OnDahai = (_, _) => new RonResponse() },
            FakePlayer.Create(3),
        };
        using var ctx = RoundStateContextRuntimeTestHelper.CreateContext(
            players,
            tenpaiMock.Object,
            RoundTestHelper.NoOpScoreCalculator,
            new GameRules(),
            NullGameTracer.Instance,
            NullLogger<RoundStateContext>.Instance
        );

        // Act
        var task = ctx.StartAsync(RoundTestHelper.CreateRound(), TestContext.Current.CancellationToken);
        var result = await RoundStateContextRuntimeTestHelper.AwaitRoundEndAsync(task, RoundStateContextRuntimeTestHelper.DEFAULT_TEST_TIMEOUT);

        // Assert
        var win = Assert.IsType<RoundEndedByWinEventArgs>(result);
        Assert.Equal(WinType.Ron, win.WinType);
        Assert.Single(win.WinnerIndices);
        Assert.Equal(2, win.WinnerIndices[0].Value);
        Assert.Equal(0, win.LoserIndex.Value);
    }

    [Fact]
    public async Task 二人がRonResponse_ダブロンで両者採用される()
    {
        // Arrange
        var tenpaiMock = new Mock<ITenpaiChecker>();
        tenpaiMock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([33]);

        var players = new FakePlayer[]
        {
            FakePlayer.Create(0),
            new(PlayerId.NewId(), "F1", new PlayerIndex(1)) { OnDahai = (_, _) => new RonResponse() },
            new(PlayerId.NewId(), "F2", new PlayerIndex(2)) { OnDahai = (_, _) => new RonResponse() },
            FakePlayer.Create(3),
        };
        using var ctx = RoundStateContextRuntimeTestHelper.CreateContext(
            players,
            tenpaiMock.Object,
            RoundTestHelper.NoOpScoreCalculator,
            new GameRules(),
            NullGameTracer.Instance,
            NullLogger<RoundStateContext>.Instance
        );

        // Act
        var task = ctx.StartAsync(RoundTestHelper.CreateRound(), TestContext.Current.CancellationToken);
        var result = await RoundStateContextRuntimeTestHelper.AwaitRoundEndAsync(task, RoundStateContextRuntimeTestHelper.DEFAULT_TEST_TIMEOUT);

        // Assert
        var win = Assert.IsType<RoundEndedByWinEventArgs>(result);
        Assert.Equal(WinType.Ron, win.WinType);
        Assert.Equal(2, win.WinnerIndices.Length);
        // 放銃者 0 起点の上家優先: 1 (下家) → 2 (対面)
        Assert.Equal(1, win.WinnerIndices[0].Value);
        Assert.Equal(2, win.WinnerIndices[1].Value);
    }

    [Fact]
    public async Task RonCandidateありでもOkResponse_見逃しとして受理される()
    {
        // Arrange: 全員 OkResponse (=見逃し) でも次ツモへ進行、2 ツモ目でツモ和了させて終了。
        // H4 合法応答検証下で TsumoAgari を成立させるため CreatePermissiveContext (全牌種 waits) を使用
        var players = new FakePlayer[]
        {
            new(PlayerId.NewId(), "F0", new PlayerIndex(0))
            {
                OnTsumo = TsumoAgariOnSecondTsumo(),
            },
            FakePlayer.Create(1),
            FakePlayer.Create(2),
            FakePlayer.Create(3),
        };
        using var ctx = RoundStateContextRuntimeTestHelper.CreatePermissiveContext(players);

        // Act
        var task = ctx.StartAsync(RoundTestHelper.CreateRound(), TestContext.Current.CancellationToken);
        var result = await RoundStateContextRuntimeTestHelper.AwaitRoundEndAsync(task, RoundStateContextRuntimeTestHelper.DEFAULT_TEST_TIMEOUT);

        // Assert
        var win = Assert.IsType<RoundEndedByWinEventArgs>(result);
        Assert.Equal(WinType.Tsumo, win.WinType);
    }

    private static Func<TsumoNotification, CancellationToken, AfterTsumoResponse> TsumoAgariOnSecondTsumo()
    {
        var count = 0;
        return (notification, _) =>
        {
            count++;
            if (count >= 2)
            {
                return new TsumoAgariResponse();
            }
            // 第1ツモでは先頭打牌候補を打牌
            var dahai = notification.CandidateList.GetCandidates<DahaiCandidate>().First();
            return new DahaiResponse(dahai.DahaiOptionList[0].Tile);
        };
    }
}
