using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;
using System.Threading.Channels;

namespace Mahjong.Lib.Game.States.RoundStates;

/// <summary>
/// システム状態遷移コンテキスト
/// </summary>
public class RoundStateContext(
    ITenpaiChecker tenpaiChecker,
    IScoreCalculator scoreCalculator
) : IDisposable
{
    /// <summary>
    /// テンパイ判定機 (フリテン判定・荒牌平局精算で使用)
    /// </summary>
    public ITenpaiChecker TenpaiChecker { get; } = tenpaiChecker;

    /// <summary>
    /// 和了時点数計算機
    /// </summary>
    public IScoreCalculator ScoreCalculator { get; } = scoreCalculator;
    public event EventHandler<RoundStateChangedEventArgs>? RoundStateChanged;

    /// <summary>
    /// 局終了(和了/流局)を通知する
    /// </summary>
    public event EventHandler<RoundEndedEventArgs>? RoundEnded;

    /// <summary>
    /// 不正な応答 (現状態で受け付けない WinType 等) を受信したことを通知する
    /// </summary>
    public event EventHandler<InvalidRoundEventEventArgs>? InvalidEventReceived;

    private readonly Channel<RoundEvent> eventChannel_ = Channel.CreateBounded<RoundEvent>(new BoundedChannelOptions(100) { SingleReader = true });
    private readonly CancellationTokenSource cancellationTokenSource_ = new();
    private Task? eventProcessingTask_;
    private bool roundEndNotified_;
    private bool disposed_;

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
    /// <param name="winnerIndices">和了者 (ダブロン/トリロンなら複数)</param>
    /// <param name="loserIndex">放銃者。ロン/槍槓では打牌者/加槓宣言者、ツモ/嶺上では <paramref name="winnerIndices"/>[0] と同値</param>
    /// <param name="winType">和了種別</param>
    public async Task ResponseWinAsync(ImmutableArray<PlayerIndex> winnerIndices, PlayerIndex loserIndex, WinType winType)
    {
        await EnqueueEventAsync(new RoundEventResponseWin(winnerIndices, loserIndex, winType));
    }

    /// <summary>
    /// 打牌応答イベントを発行する
    /// </summary>
    /// <param name="tile">打牌する牌</param>
    /// <param name="isRiichi">この打牌で立直宣言するか (既定: false)。
    /// 立直宣言は打牌時に保留状態となり、後続の応答 (ロンなし=確定、ロン=不成立) で確定/破棄される</param>
    public async Task ResponseDahaiAsync(Tile tile, bool isRiichi = false)
    {
        await EnqueueEventAsync(new RoundEventResponseDahai(tile, isRiichi));
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
    /// <param name="callerIndex">副露するプレイヤー</param>
    /// <param name="type">Chi / Pon / Daiminkan</param>
    /// <param name="handTiles">callerの手牌から使用する牌 (Chi・Pon: 2枚、Daiminkan: 3枚)</param>
    /// <param name="calledTile">鳴かれる牌 (直前の打牌)</param>
    public async Task ResponseCallAsync(PlayerIndex callerIndex, CallType type, ImmutableList<Tile> handTiles, Tile calledTile)
    {
        await EnqueueEventAsync(new RoundEventResponseCall(callerIndex, type, handTiles, calledTile));
    }

    /// <summary>
    /// 流局応答イベントを発行する
    /// </summary>
    /// <param name="type">流局種別</param>
    /// <param name="tenpaiPlayers">テンパイ者 (荒牌平局時のみ意味を持つ、他は空配列可)</param>
    public async Task ResponseRyuukyokuAsync(RyuukyokuType type, ImmutableArray<PlayerIndex> tenpaiPlayers)
    {
        await EnqueueEventAsync(new RoundEventResponseRyuukyoku(type, tenpaiPlayers));
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
    /// 局終了を一度だけ通知する。終端状態(Win/Ryuukyoku)の ResponseOk が複数回呼ばれても
    /// RoundEnded は二重発火しない (二重 OK で局結果が二重適用されるバグの予防)
    /// </summary>
    internal void OnRoundEnded(RoundEndedEventArgs args)
    {
        if (roundEndNotified_) { return; }

        roundEndNotified_ = true;
        RoundEnded?.Invoke(this, args);
    }

    /// <summary>
    /// 状態遷移イベントを処理します。
    /// 個々のイベント処理で発生した例外 (不正な応答等) はループを止めずに観測用イベントへ通知します
    /// (合法候補列挙とエラー通知の正式な仕組みは Phase 5 で導入予定)
    /// </summary>
    private async Task ProcessEventAsync()
    {
        try
        {
            await foreach (var evt in DequeueEventsAsync())
            {
                try
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
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // TODO: Phase 5 でエラーハンドリングを正式化 (購読者不在時のログ出力等)
                    InvalidEventReceived?.Invoke(this, new InvalidRoundEventEventArgs(evt, ex));
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

/// <summary>
/// 不正な応答イベントを受信した際の通知引数
/// </summary>
public record InvalidRoundEventEventArgs(RoundEvent Event, Exception Exception);
