using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Walls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Threading.Channels;

namespace Mahjong.Lib.Game.States.GameStates;

/// <summary>
/// 対局状態遷移コンテキスト
/// </summary>
/// <remarks>
/// 通知・応答集約経路を有効化するコンストラクタ
/// players / projector / enumerator / priorityPolicy / defaultFactory を揃えると
/// <see cref="StartRound"/> で RoundManager を生成し、通知・応答集約経路で動作する。
/// いずれかが null の場合は RoundStateContext 直接駆動経路にフォールバックする
/// </remarks>
public class GameStateContext(
    IWallGenerator wallGenerator,
    IScoreCalculator scoreCalculator,
    ITenpaiChecker tenpaiChecker,
    PlayerList? players,
    IRoundViewProjector? projector,
    IResponseCandidateEnumerator? enumerator,
    IResponsePriorityPolicy? priorityPolicy,
    IDefaultResponseFactory? defaultFactory,
    IGameTracer? tracer = null,
    ILoggerFactory? loggerFactory = null
    ) : IDisposable
{
    public event EventHandler<GameStateChangedEventArgs>? GameStateChanged;

    private bool disposed_;
    private readonly Channel<GameEvent> eventChannel_ = Channel.CreateBounded<GameEvent>(new BoundedChannelOptions(100) { SingleReader = true });
    private readonly CancellationTokenSource cancellationTokenSource_ = new();
    private Task? eventProcessingTask_;
    private readonly IGameTracer tracer_ = tracer ?? new NullGameTracer();
    private readonly ILoggerFactory loggerFactory_ = loggerFactory ?? NullLoggerFactory.Instance;

    public GameState State
    {
        get => field ?? throw new InvalidOperationException("InitAsync() が呼び出されていません。");
        private set;
    }

    public Games.Game Game
    {
        get => field ?? throw new InvalidOperationException("InitAsync() が呼び出されていません。");
        internal set;
    }

    /// <summary>
    /// 山牌生成機
    /// </summary>
    public IWallGenerator WallGenerator { get; } = wallGenerator;

    /// <summary>
    /// テンパイ判定機 (Round 生成時に注入され、フリテン判定・荒牌平局精算で使用される)
    /// </summary>
    public ITenpaiChecker TenpaiChecker { get; } = tenpaiChecker;

    /// <summary>
    /// 和了時点数計算機 (Round 生成時に注入され、和了精算で使用される)
    /// </summary>
    public IScoreCalculator ScoreCalculator { get; } = scoreCalculator;

    /// <summary>
    /// 現在進行中の局の RoundStateContext 局終了と共に破棄される
    /// </summary>
    public RoundStateContext? RoundStateContext { get; private set; }

    /// <summary>
    /// 現在進行中の局の RoundManager (通知・応答集約経路)
    /// players / projector / enumerator / priorityPolicy / defaultFactory がすべて与えられた場合に生成される
    /// </summary>
    public RoundManager? RoundManager { get; private set; }

    protected virtual TimeSpan DisposeTimeout => TimeSpan.FromSeconds(5);

    /// <summary>
    /// <see cref="GameNotification"/> の ACK タイムアウト
    /// </summary>
    protected virtual TimeSpan NotificationTimeout => TimeSpan.FromSeconds(10);

    /// <summary>
    /// RoundManager 経路 (通知・応答集約) が有効かどうか
    /// </summary>
    internal bool IsRoundManagerAvailable =>
        players is not null
        && projector is not null
        && enumerator is not null
        && priorityPolicy is not null
        && defaultFactory is not null;

    /// <summary>
    /// RoundStateContext 直接駆動経路のみで動作するコンストラクタ (通知・応答集約は行わない)
    /// </summary>
    public GameStateContext(
        IWallGenerator wallGenerator,
        IScoreCalculator scoreCalculator,
        ITenpaiChecker tenpaiChecker
    ) : this(
        wallGenerator,
        scoreCalculator,
        tenpaiChecker,
        players: null,
        projector: null,
        enumerator: null,
        priorityPolicy: null,
        defaultFactory: null,
        tracer: null,
        loggerFactory: null
    )
    {
    }

    /// <summary>
    /// 指定された対局で状態遷移を開始します
    /// 対局開始通知 → 局開始通知 → StartRound までを await で完了させる (通知・応答集約経路が有効な場合)
    /// </summary>
    public async Task InitAsync(Games.Game game, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
        if (eventProcessingTask_ is not null)
        {
            throw new InvalidOperationException("InitAsync() は既に呼び出されています。");
        }

        State = new GameStateInit();
        Game = game;

        eventProcessingTask_ = Task.Run(ProcessEventAsync, ct);

        await State.EntryAsync(this, ct);

        if (!IsRoundManagerAvailable) { return; }

        await BroadcastGameNotificationAsync(
            x => new GameStartNotification(Game.PlayerList, Game.Rules, x),
            ct
        );

        await TransitAsync(
            new GameStateRoundRunning(),
            action: async () =>
            {
                await BroadcastGameNotificationAsync(
                    _ => new RoundStartNotification(
                        Game.RoundWind,
                        Game.RoundNumber,
                        Game.Honba,
                        Game.RoundNumber.ToDealer()
                    ),
                    ct
                );
                StartRound(Game.CreateRound(WallGenerator));
            },
            ct
        );
    }

    /// <summary>
    /// OK応答イベントを発行します
    /// </summary>
    public async Task ResponseOkAsync()
    {
        await EnqueueEventAsync(new GameEventResponseOk());
    }

    /// <summary>
    /// 新しい RoundStateContext (および RoundManager 経路が有効なら RoundManager) を生成し 指定の Round で初期化します。
    /// RoundManager 経路が有効なときは RoundManager が RoundStateContext をホストし、通知・応答集約を担う。
    /// そうでないとき (旧コンストラクタ経由) は RoundStateContext を直接駆動する。
    /// いずれの経路でも <see cref="RoundStateContext"/> プロパティが現行の RoundStateContext を返すため
    /// <see cref="GameStateRoundRunning"/> などが <c>RoundStateContext.Round</c> を参照できる
    /// </summary>
    internal void StartRound(Round round)
    {
        ObjectDisposedException.ThrowIf(disposed_, this);

        if (IsRoundManagerAvailable)
        {
            var manager = new RoundManager(
                players!,
                projector!,
                enumerator!,
                priorityPolicy!,
                defaultFactory!,
                TenpaiChecker,
                ScoreCalculator,
                tracer_,
                loggerFactory_.CreateLogger<RoundManager>()
            );
            manager.RoundEnded += OnRoundEnded;
            _ = manager.StartAsync(round);
            RoundManager = manager;
            RoundStateContext = manager.InternalContext;
            return;
        }

        var roundStateContext = new RoundStateContext(TenpaiChecker, ScoreCalculator);
        roundStateContext.RoundEnded += OnRoundEnded;
        roundStateContext.Init(round);

        RoundStateContext = roundStateContext;
    }

    /// <summary>
    /// 現在の RoundStateContext / RoundManager の購読を解除し破棄します。
    /// RoundManager 経路の場合は RoundManager が内部 RoundStateContext も破棄する
    /// </summary>
    internal void DisposeRoundContext()
    {
        if (RoundManager is not null)
        {
            RoundManager.RoundEnded -= OnRoundEnded;
            RoundManager.Dispose();
            RoundManager = null;
            RoundStateContext = null;
            return;
        }

        if (RoundStateContext is null) { return; }

        RoundStateContext.RoundEnded -= OnRoundEnded;
        RoundStateContext.Dispose();
        RoundStateContext = null;
    }

    /// <summary>
    /// 指定された GameState に遷移する
    /// 遷移時アクションで <see cref="GameNotification"/> の送信や <see cref="StartRound"/> を await できる
    /// </summary>
    internal async Task TransitAsync(GameState nextState, Func<Task>? action = null, CancellationToken ct = default)
    {
        State.Exit(this);
        if (action is not null)
        {
            await action();
        }
        State = nextState;
        await State.EntryAsync(this, ct);
    }

    internal void OnStateChanged(GameState state)
    {
        GameStateChanged?.Invoke(this, new GameStateChangedEventArgs(state));
    }

    internal async Task EnqueueEventAsync(GameEvent evt)
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
        if (eventProcessingTask_ is null)
        {
            throw new InvalidOperationException("InitAsync() が呼び出されていません。");
        }

        await eventChannel_.Writer.WriteAsync(evt);
    }

    /// <summary>
    /// 全プレイヤーに <see cref="GameNotification"/> を並列送信し、OK ACK を集約する
    /// タイムアウト時はログを出すのみで例外なく継続 (プレイヤー未応答でも対局進行は止めない)
    /// </summary>
    /// <param name="notificationFactory">受信者 PlayerIndex から通知を生成するファクトリ
    /// (GameStartNotification など受信者別情報を含む通知にも対応するため)</param>
    internal async Task BroadcastGameNotificationAsync(
        Func<PlayerIndex, GameNotification> notificationFactory,
        CancellationToken ct = default
    )
    {
        if (!IsRoundManagerAvailable) { return; }

        var tasks = Enumerable.Range(0, PlayerIndex.PLAYER_COUNT)
            .Select(i => new PlayerIndex(i))
            .Select(x => InvokeGameNotificationAsync(notificationFactory(x), x, ct))
            .ToArray();
        await Task.WhenAll(tasks);
    }

    private async Task InvokeGameNotificationAsync(GameNotification notification, PlayerIndex recipientIndex, CancellationToken ct)
    {
        var player = players![recipientIndex];
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(NotificationTimeout);
        // L1: プレイヤーメソッド呼び出しで発生しうる同期例外も try 内で受け止める
        try
        {
            var task = notification switch
            {
                GameStartNotification n => player.OnGameStartAsync(n, linkedCts.Token),
                RoundStartNotification n => player.OnRoundStartAsync(n, linkedCts.Token),
                RoundEndNotification n => player.OnRoundEndAsync(n, linkedCts.Token),
                GameEndNotification n => player.OnGameEndAsync(n, linkedCts.Token),
                _ => throw new NotSupportedException($"GameNotification として未対応です。実際:{notification.GetType().Name}"),
            };
            await task;
        }
        catch (OperationCanceledException)
        {
            loggerFactory_.CreateLogger<GameStateContext>().LogWarning(
                "GameNotification ACK タイムアウト player:{Index} notification:{Notification}",
                recipientIndex.Value,
                notification.GetType().Name
            );
        }
        catch (Exception ex)
        {
            loggerFactory_.CreateLogger<GameStateContext>().LogWarning(
                ex,
                "GameNotification ACK 例外 player:{Index} notification:{Notification}",
                recipientIndex.Value,
                notification.GetType().Name
            );
        }
    }

    private async Task ProcessEventAsync()
    {
        try
        {
            await foreach (var evt in eventChannel_.Reader.ReadAllAsync(cancellationTokenSource_.Token))
            {
                switch (evt)
                {
                    case GameEventResponseOk ok:
                        await State.ResponseOkAsync(this, ok, cancellationTokenSource_.Token);
                        break;

                    case GameEventRoundEndedByWin win:
                        await State.RoundEndedByWinAsync(this, win, cancellationTokenSource_.Token);
                        break;

                    case GameEventRoundEndedByRyuukyoku ryuukyoku:
                        await State.RoundEndedByRyuukyokuAsync(this, ryuukyoku, cancellationTokenSource_.Token);
                        break;

                    default:
                        throw new NotSupportedException($"未対応のイベント種別:{evt?.GetType().Name}");
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    /// <summary>
    /// Round 側の局終了イベントを受けて GameState 側の局終了イベントをキューに投入します。
    /// 容量100の Bounded で局終了 1 件の TryWrite が full で失敗するケースは
    /// disposed/closed 時に限られるため、戻り値は無視する (実害なし)
    /// </summary>
    private void OnRoundEnded(object? sender, RoundEndedEventArgs args)
    {
        GameEvent evt = args switch
        {
            RoundEndedByWinEventArgs win => new GameEventRoundEndedByWin(
                win.WinnerIndices, win.LoserIndex, win.WinType, win.Winners, win.Honba, win.KyoutakuRiichiAward),
            RoundEndedByRyuukyokuEventArgs ryuukyoku => new GameEventRoundEndedByRyuukyoku(ryuukyoku.Type, ryuukyoku.TenpaiPlayerIndices, ryuukyoku.NagashiManganPlayerIndices),
            _ => throw new NotSupportedException($"未対応の局終了引数: {args?.GetType().Name}"),
        };
        var written = eventChannel_.Writer.TryWrite(evt);
        Debug.Assert(written, "局終了イベントの投入に失敗。チャネル容量超過。");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed_) { return; }

        if (disposing)
        {
            DisposeRoundContext();

            eventChannel_.Writer.Complete();
            if (eventProcessingTask_ is not null && !eventProcessingTask_.Wait(DisposeTimeout))
            {
                cancellationTokenSource_.Cancel();
                eventProcessingTask_.Wait(DisposeTimeout);
            }
            cancellationTokenSource_.Dispose();
        }

        disposed_ = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public record GameStateChangedEventArgs(GameState State);
