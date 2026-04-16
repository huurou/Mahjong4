using System.Diagnostics;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Walls;
using System.Threading.Channels;

namespace Mahjong.Lib.Game.States.GameStates;

/// <summary>
/// 対局状態遷移コンテキスト
/// </summary>
public class GameStateContext(
    IWallGenerator wallGenerator,
    IScoreCalculator scoreCalculator,
    ITenpaiChecker tenpaiChecker
) : IDisposable
{
    public event EventHandler<GameStateChangedEventArgs>? GameStateChanged;

    private bool disposed_;
    private readonly Channel<GameEvent> eventChannel_ = Channel.CreateBounded<GameEvent>(new BoundedChannelOptions(100) { SingleReader = true });
    private readonly CancellationTokenSource cancellationTokenSource_ = new();
    private Task? eventProcessingTask_;

    public GameState State
    {
        get => field ?? throw new InvalidOperationException("Init() が呼び出されていません。");
        private set;
    }

    public Games.Game Game
    {
        get => field ?? throw new InvalidOperationException("Init() が呼び出されていません。");
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

    protected virtual TimeSpan DisposeTimeout => TimeSpan.FromSeconds(5);

    /// <summary>
    /// 指定された対局で状態遷移を開始します
    /// </summary>
    public void Init(Games.Game game)
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
        if (eventProcessingTask_ is not null)
        {
            throw new InvalidOperationException("Init() は既に呼び出されています。");
        }

        State = new GameStateInit();
        Game = game;

        State.Entry(this);

        eventProcessingTask_ = Task.Run(ProcessEventAsync);
    }

    /// <summary>
    /// OK応答イベントを発行します
    /// </summary>
    public async Task ResponseOkAsync()
    {
        await EnqueueEventAsync(new GameEventResponseOk());
    }

    /// <summary>
    /// 新しい RoundStateContext を生成し 指定の Round で初期化します
    /// </summary>
    internal void StartRound(Round round)
    {
        ObjectDisposedException.ThrowIf(disposed_, this);

        var roundStateContext = new RoundStateContext(TenpaiChecker, ScoreCalculator);
        roundStateContext.RoundEnded += OnRoundEnded;
        roundStateContext.Init(round);

        // Init() 完了後にプロパティへ公開する。
        // テストの WaitForNewRoundContextAsync が非null を検出した時点で
        // Init() (State・eventProcessingTask_ の設定) が完了していることを保証する。
        RoundStateContext = roundStateContext;
    }

    /// <summary>
    /// 現在の RoundStateContext の購読を解除し破棄します
    /// </summary>
    internal void DisposeRoundContext()
    {
        if (RoundStateContext is null) { return; }

        RoundStateContext.RoundEnded -= OnRoundEnded;
        RoundStateContext.Dispose();
        RoundStateContext = null;
    }

    internal void Transit(GameState nextState, Action? action = null)
    {
        State.Exit(this);
        action?.Invoke();
        State = nextState;
        State.Entry(this);
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
            throw new InvalidOperationException("Init() が呼び出されていません。");
        }

        await eventChannel_.Writer.WriteAsync(evt);
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
                        State.ResponseOk(this, ok);
                        break;

                    case GameEventRoundEndedByWin win:
                        State.RoundEndedByWin(this, win);
                        break;

                    case GameEventRoundEndedByRyuukyoku ryuukyoku:
                        State.RoundEndedByRyuukyoku(this, ryuukyoku);
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
            RoundEndedByWinEventArgs win => new GameEventRoundEndedByWin(win.WinnerIndices, win.LoserIndex, win.WinType),
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
