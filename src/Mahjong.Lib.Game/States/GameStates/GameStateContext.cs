using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.GameStates.Impl;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Walls;
using System.Collections.Immutable;
using System.Threading.Channels;

namespace Mahjong.Lib.Game.States.GameStates;

/// <summary>
/// 対局状態遷移コンテキスト
/// </summary>
public class GameStateContext(IWallGenerator wallGenerator) : IDisposable
{
    public event EventHandler<GameStateChangedEventArgs>? GameStateChanged;

    private bool disposed_;
    private readonly Channel<GameEvent> eventChannel_ = Channel.CreateBounded<GameEvent>(new BoundedChannelOptions(100) { SingleReader = true });
    private readonly CancellationTokenSource cancellationTokenSource_ = new();
    private Task? eventProcessingTask_;

    private EventHandler<RoundStateChangedEventArgs>? currentRoundHandler_;
    private bool roundEnded_;

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

        roundEnded_ = false;
        currentRoundHandler_ = OnRoundStateChanged;

        RoundStateContext = new RoundStateContext();
        RoundStateContext.RoundStateChanged += currentRoundHandler_;

        RoundStateContext.Init(round);
    }

    /// <summary>
    /// 現在の RoundStateContext の購読を解除し破棄します
    /// </summary>
    internal void DisposeRoundContext()
    {
        if (RoundStateContext is null) { return; }

        if (currentRoundHandler_ is not null)
        {
            RoundStateContext.RoundStateChanged -= currentRoundHandler_;
        }
        RoundStateContext.Dispose();
        RoundStateContext = null;
        currentRoundHandler_ = null;
    }

    /// <summary>
    /// Round 側の状態変化を監視し Win/Ryuukyoku 到達時に局終了イベントを内部発行します
    /// </summary>
    private void OnRoundStateChanged(object? sender, RoundStateChangedEventArgs args)
    {
        if (roundEnded_) { return; }
        if (RoundStateContext is null) { return; }

        GameEvent? evt = args.State switch
        {
            RoundStateWin => BuildWinEvent(RoundStateContext.Round),
            RoundStateRyuukyoku => BuildRyuukyokuEvent(RoundStateContext.Round),
            _ => null,
        };

        if (evt is null) { return; }

        roundEnded_ = true;
        _ = NotifyRoundEndedSafelyAsync(evt);
    }

    /// <summary>
    /// 局終了イベントを発行します Dispose 済み等で失敗した場合は握りつぶします
    /// </summary>
    private async Task NotifyRoundEndedSafelyAsync(GameEvent evt)
    {
        try
        {
            await EnqueueEventAsync(evt);
        }
        catch (ObjectDisposedException) { }
        catch (ChannelClosedException) { }
    }

    /// <summary>
    /// Phase 1 スタブ 和了者として常に <see cref="Round.Turn"/> を返す
    /// ツモ和了では Turn == 和了者なので正しいが
    /// ロン和了では Turn == 打牌者(放銃者) となるため Winners に誤った値が入る
    /// その結果 <see cref="Impl.GameStateRoundRunning.RoundEndedByWin"/> の親連荘判定
    /// (<c>evt.Winners.Contains(dealer)</c>) が誤判定し 親連荘/親流れが逆転するケースがある
    /// Phase 2 で <see cref="RoundStateWin"/> に和了者情報を持たせて正確化予定
    /// </summary>
    private static GameEventRoundEndedByWin BuildWinEvent(Round round)
    {
        return new GameEventRoundEndedByWin([round.Turn]);
    }

    /// <summary>
    /// Phase 1 スタブ 流局種別として常に <see cref="RyuukyokuType.KouhaiHeikyoku"/> を返す
    /// 途中流局・親テンパイ流局などの区別が付かないため
    /// Phase 2 で <see cref="RoundStateRyuukyoku"/> に流局種別情報を持たせて正確化予定
    /// </summary>
    private static GameEventRoundEndedByRyuukyoku BuildRyuukyokuEvent(Round round)
    {
        return new GameEventRoundEndedByRyuukyoku(RyuukyokuType.KouhaiHeikyoku);
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
