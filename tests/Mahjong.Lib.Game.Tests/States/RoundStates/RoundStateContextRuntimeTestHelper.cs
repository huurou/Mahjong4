using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tests.Players;
using Microsoft.Extensions.Logging.Abstractions;

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
    /// 既定の <see cref="RoundStateContext"/> を生成する
    /// </summary>
    internal static RoundStateContext CreateDefaultContext(IEnumerable<Player> players)
    {
        return CreateContext(
            players,
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
        GameRules rules,
        IGameTracer tracer,
        Microsoft.Extensions.Logging.ILogger<RoundStateContext> logger
    )
    {
        return new RoundStateContext(
            new PlayerList(players),
            new RoundViewProjector(),
            new ResponseCandidateEnumerator(rules),
            new TenhouResponsePriorityPolicy(),
            new DefaultResponseFactory(),
            rules,
            tracer,
            logger
        );
    }

    /// <summary>
    /// 通知・応答集約ループの実計算ベースの <see cref="RoundStateContext"/> を返す。
    /// (旧 Mock ベースの Permissive TenpaiChecker は廃止され、実計算に統一済。ここでは既定実装を返す)
    /// </summary>
    internal static RoundStateContext CreatePermissiveContext(IEnumerable<Player> players)
    {
        return CreateDefaultContext(players);
    }

    /// <summary>
    /// 指定時間内に <paramref name="task"/> が完了することを保証する
    /// </summary>
    internal static async Task<RoundEndedEventArgs> AwaitRoundEndAsync(Task<RoundEndedEventArgs> task, TimeSpan timeout)
    {
        return await task.WaitAsync(timeout);
    }
}
