using Mahjong.Lib.Game.States.RoundStates.Impl;
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

    protected virtual TimeSpan DisposeTimeout => TimeSpan.FromSeconds(5);

    public void Init()
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
        if (eventProcessingTask_ is not null)
        {
            throw new InvalidOperationException("Init() は既に呼び出されています。");
        }

        State = new RoundStateHaipai();
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
    public async Task ResponseDahaiAsync()
    {
        await EnqueueEventAsync(new RoundEventResponseDahai());
    }

    /// <summary>
    /// 槓応答イベントを発行する
    /// </summary>
    public async Task ResponseKanAsync()
    {
        await EnqueueEventAsync(new RoundEventResponseKan());
    }

    /// <summary>
    /// 副露応答イベントを発行する
    /// </summary>
    public async Task ResponseCallAsync()
    {
        await EnqueueEventAsync(new RoundEventResponseCall());
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
                        break;
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task EnqueueEventAsync(RoundEvent evt)
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
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
