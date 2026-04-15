using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;
using System.Threading.Channels;

namespace Mahjong.Lib.Game.States.RoundStates;

/// <summary>
/// システム状態遷移コンテキスト
/// </summary>
public class RoundStateContext : IDisposable
{
    public event EventHandler<RoundStateChangedEventArgs>? RoundStateChanged;

    private bool disposed_;
    private readonly Channel<RoundEvent> eventChannel_ = Channel.CreateBounded<RoundEvent>(new BoundedChannelOptions(100) { SingleReader = true });
    private readonly CancellationTokenSource cancellationTokenSource_ = new();
    private Task? eventProcessingTask_;

    public RoundState State
    {
        get => field ?? throw new InvalidOperationException("Init() が呼び出されていません。");
        private set;
    }

    public Round Round
    {
        get => field ?? throw new InvalidOperationException("Init() が呼び出されていません。");
        internal set;
    }

    protected virtual TimeSpan DisposeTimeout => TimeSpan.FromSeconds(5);

    public void Init(Round round)
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
        if (eventProcessingTask_ is not null)
        {
            throw new InvalidOperationException("Init() は既に呼び出されています。");
        }

        State = new RoundStateHaipai();
        Round = round.Haipai();

        State.Entry(this);

        eventProcessingTask_ = Task.Run(ProcessEventAsync);
    }

    /// <summary>
    /// OK応答イベントを発行する
    /// </summary>
    public async Task ResponseOkAsync()
    {
        await EnqueueEventAsync(new RoundEventResponseOk());
    }

    /// <summary>
    /// 和了応答イベントを発行する
    /// </summary>
    public async Task ResponseWinAsync()
    {
        await EnqueueEventAsync(new RoundEventResponseWin());
    }

    /// <summary>
    /// 打牌応答イベントを発行する
    /// </summary>
    public async Task ResponseDahaiAsync(Tile tile)
    {
        await EnqueueEventAsync(new RoundEventResponseDahai(tile));
    }

    /// <summary>
    /// 槓応答イベントを発行する
    /// </summary>
    /// <param name="type">Ankan または Kakan</param>
    /// <param name="tile">暗槓: 槓する牌種の牌、加槓: 追加する手牌の牌</param>
    public async Task ResponseKanAsync(CallType type, Tile tile)
    {
        await EnqueueEventAsync(new RoundEventResponseKan(type, tile));
    }

    /// <summary>
    /// 副露応答イベントを発行する
    /// </summary>
    /// <param name="caller">副露するプレイヤー</param>
    /// <param name="type">Chi / Pon / Daiminkan</param>
    /// <param name="handTiles">callerの手牌から使用する牌 (Chi・Pon: 2枚、Daiminkan: 3枚)</param>
    /// <param name="calledTile">鳴かれる牌 (直前の打牌)</param>
    public async Task ResponseCallAsync(PlayerIndex caller, CallType type, ImmutableList<Tile> handTiles, Tile calledTile)
    {
        await EnqueueEventAsync(new RoundEventResponseCall(caller, type, handTiles, calledTile));
    }

    /// <summary>
    /// 流局応答イベントを発行する
    /// </summary>
    public async Task ResponseRyuukyokuAsync()
    {
        await EnqueueEventAsync(new RoundEventResponseRyuukyoku());
    }

    /// <summary>
    /// 指定された状態に遷移します。
    /// </summary>
    /// <param name="nextState">遷移先状態</param>
    /// <param name="action">遷移時アクション</param>
    internal void Transit(RoundState nextState, Action? action = null)
    {
        State.Exit(this);
        action?.Invoke();
        State = nextState;
        State.Entry(this);
    }

    internal void OnStateChanged(RoundState state)
    {
        RoundStateChanged?.Invoke(this, new RoundStateChangedEventArgs(state));
    }

    /// <summary>
    /// 状態遷移イベントを処理します。
    /// </summary>
    private async Task ProcessEventAsync()
    {
        try
        {
            await foreach (var evt in DequeueEventsAsync())
            {
                switch (evt)
                {
                    case RoundEventResponseOk ok:
                        State.ResponseOk(this, ok);
                        break;

                    case RoundEventResponseWin win:
                        State.ResponseWin(this, win);
                        break;

                    case RoundEventResponseDahai dahai:
                        State.ResponseDahai(this, dahai);
                        break;

                    case RoundEventResponseKan kan:
                        State.ResponseKan(this, kan);
                        break;

                    case RoundEventResponseCall call:
                        State.ResponseCall(this, call);
                        break;

                    case RoundEventResponseRyuukyoku ryuukyoku:
                        State.ResponseRyuukyoku(this, ryuukyoku);
                        break;

                    default:
                        throw new NotSupportedException($"未対応のイベント種別:{evt?.GetType().Name}");
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task EnqueueEventAsync(RoundEvent evt)
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
        if (eventProcessingTask_ is null)
        {
            throw new InvalidOperationException("Init() が呼び出されていません。");
        }

        await eventChannel_.Writer.WriteAsync(evt);
    }

    private IAsyncEnumerable<RoundEvent> DequeueEventsAsync()
    {
        return eventChannel_.Reader.ReadAllAsync(cancellationTokenSource_.Token);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed_) { return; }

        if (disposing)
        {
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

public record RoundStateChangedEventArgs(RoundState State);
