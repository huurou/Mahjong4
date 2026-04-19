using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Walls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mahjong.Lib.Game.Games;

/// <summary>
/// 対局の統括管理
/// 対局開始・局間引き継ぎ・対局終了判定を担当する
/// </summary>
/// <param name="playerList">プレイヤー席のリスト (index 0 が起家)</param>
/// <param name="rules">対局ルール</param>
/// <param name="wallGenerator">山牌生成機</param>
/// <param name="scoreCalculator">和了時点数計算機</param>
/// <param name="tenpaiChecker">テンパイ判定機</param>
/// <param name="projector">視点射影</param>
/// <param name="enumerator">合法応答候補列挙</param>
/// <param name="priorityPolicy">応答優先順位解決</param>
/// <param name="defaultFactory">タイムアウト時既定応答生成</param>
/// <param name="notificationBuilder">通知ビルダー (既定実装: <see cref="RoundNotificationBuilder"/>)</param>
/// <param name="dispatcher">応答ディスパッチャ (既定実装: <see cref="ResponseDispatcher"/>)</param>
/// <param name="tracer">対局トレーサー (no-op が必要な場合は <see cref="NullGameTracer.Instance"/> を明示的に渡す)</param>
/// <param name="loggerFactory">ロガーファクトリ (no-op が必要な場合は <see cref="NullLoggerFactory.Instance"/> を明示的に渡す)</param>
public class GameManager(
    PlayerList playerList,
    GameRules rules,
    IWallGenerator wallGenerator,
    IScoreCalculator scoreCalculator,
    ITenpaiChecker tenpaiChecker,
    IRoundViewProjector projector,
    IResponseCandidateEnumerator enumerator,
    IResponsePriorityPolicy priorityPolicy,
    IDefaultResponseFactory defaultFactory,
    IRoundNotificationBuilder notificationBuilder,
    IResponseDispatcher dispatcher,
    IGameTracer tracer,
    ILoggerFactory loggerFactory
) : IDisposable
{
    private GameStateContext? context_;
    private bool disposed_;

    /// <summary>
    /// 対局状態遷移コンテキスト StartAsync() 呼び出し後に参照可能
    /// </summary>
    public GameStateContext Context => context_
        ?? throw new InvalidOperationException("StartAsync() が呼び出されていません。");

    /// <summary>
    /// 対局を開始します
    /// <see cref="Notifications.GameNotification"/> 送信経路を含む初期化を await する
    /// </summary>
    public async Task StartAsync(CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
        if (context_ is not null)
        {
            throw new InvalidOperationException("StartAsync() は既に呼び出されています。");
        }

        context_ = new GameStateContext(
            wallGenerator,
            scoreCalculator,
            tenpaiChecker,
            playerList,
            projector,
            enumerator,
            priorityPolicy,
            defaultFactory,
            notificationBuilder,
            dispatcher,
            tracer,
            loggerFactory
        );
        await context_.InitAsync(Game.Create(playerList, rules), ct);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed_) { return; }

        if (disposing)
        {
            context_?.Dispose();
        }

        disposed_ = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
