using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tests.Rounds;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

/// <summary>
/// 通知・応答集約ループ (本番経路) を駆動する <see cref="RoundStateContext"/> 用のテストヘルパ。
/// state 単体テスト用のヘルパは <see cref="RoundStateContextTestHelper"/> を使用する。
/// </summary>
internal static class RoundStateContextRuntimeTestHelper
{
    internal static readonly TimeSpan DEFAULT_TEST_TIMEOUT = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 4 人の FakePlayer を生成する
    /// </summary>
    internal static FakePlayer[] CreateFakePlayers()
    {
        return [.. Enumerable.Range(0, PlayerIndex.PLAYER_COUNT).Select(FakePlayer.Create)];
    }

    /// <summary>
    /// 親 (PlayerIndex 0) の 1 巡目 / 2 巡目のツモ牌種 (既定山: kind 13 / 14) を待ち牌とする <see cref="ITenpaiChecker"/>。
    /// 全牌種を waits にすると <see cref="Round.Dahai"/> 後の <see cref="Round.EvaluateFuriten"/> が自分の打牌を待ち牌とみなして
    /// IsFuriten=true を立て、直後のツモ和了候補が消えてしまう問題があるため、
    /// 親の TsumoAgari だけを成立させる最小限の waits 集合にしている
    /// </summary>
    internal static ITenpaiChecker CreatePermissiveTenpaiChecker()
    {
        var mock = new Mock<ITenpaiChecker>();
        mock.Setup(x => x.IsTenpai(It.IsAny<Hand>(), It.IsAny<CallList>())).Returns(false);
        mock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([13, 14]);
        mock.Setup(x => x.IsKoutsuOnlyInAllInterpretations(It.IsAny<Hand>(), It.IsAny<CallList>(), It.IsAny<int>()))
            .Returns(true);
        return mock.Object;
    }

    /// <summary>
    /// NoOp サービスを注入した <see cref="RoundStateContext"/> を生成する
    /// </summary>
    internal static RoundStateContext CreateDefaultContext(IEnumerable<Player> players)
    {
        return CreateContext(
            players,
            RoundTestHelper.NoOpTenpaiChecker,
            RoundTestHelper.NoOpScoreCalculator,
            new GameRules(),
            NullGameTracer.Instance,
            NullLogger<RoundStateContext>.Instance
        );
    }

    /// <summary>
    /// 全牌種を待ち牌とする緩和 TenpaiChecker を使って <see cref="RoundStateContext"/> を生成する。
    /// H4 合法応答検証の下で Ron/TsumoAgari を任意のタイミングで成立させたいテストで使用する
    /// </summary>
    internal static RoundStateContext CreatePermissiveContext(IEnumerable<Player> players)
    {
        return CreateContext(
            players,
            CreatePermissiveTenpaiChecker(),
            RoundTestHelper.NoOpScoreCalculator,
            new GameRules(),
            NullGameTracer.Instance,
            NullLogger<RoundStateContext>.Instance
        );
    }

    /// <summary>
    /// 全依存を明示指定した <see cref="RoundStateContext"/> を生成する
    /// </summary>
    internal static RoundStateContext CreateContext(
        IEnumerable<Player> players,
        ITenpaiChecker tenpaiChecker,
        IScoreCalculator scoreCalculator,
        GameRules rules,
        IGameTracer tracer,
        Microsoft.Extensions.Logging.ILogger<RoundStateContext> logger
    )
    {
        return new RoundStateContext(
            new PlayerList(players),
            new RoundViewProjector(),
            new ResponseCandidateEnumerator(tenpaiChecker, rules),
            new TenhouResponsePriorityPolicy(),
            new DefaultResponseFactory(),
            tenpaiChecker,
            scoreCalculator,
            tracer,
            logger
        );
    }

    /// <summary>
    /// 指定時間内に <paramref name="task"/> が完了することを保証する
    /// </summary>
    internal static async Task<RoundEndedEventArgs> AwaitRoundEndAsync(Task<RoundEndedEventArgs> task, TimeSpan timeout)
    {
        return await task.WaitAsync(timeout);
    }
}
