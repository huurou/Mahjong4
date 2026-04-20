using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Threading.Channels;

namespace Mahjong.Lib.Game.States.RoundStates;

/// <summary>
/// 局進行コンテキスト (状態・イベントキュー・プレイヤー応答駆動ループの所有者)。
/// 状態機械 (event queue + Transit) と通知・応答集約ループ (StartAsync) を 1 クラスに統合する。
/// 公開 API は <see cref="StartAsync(Round, CancellationToken)"/> のみ。
/// 状態 / Round の直接書換は同アセンブリ (RoundState 実装と tests assembly) からのみ可能
/// </summary>
/// <remarks>
/// 通知・応答集約ループを駆動する <see cref="RoundStateContext"/> を生成する。
/// 駆動は <see cref="StartAsync(Round, CancellationToken)"/> から開始する
/// </remarks>
public partial class RoundStateContext(
    PlayerList players,
    IRoundViewProjector projector,
    IResponseCandidateEnumerator enumerator,
    IResponsePriorityPolicy priorityPolicy,
    IDefaultResponseFactory defaultFactory,
    ITenpaiChecker tenpaiChecker,
    IScoreCalculator scoreCalculator,
    IGameTracer tracer,
    ILogger<RoundStateContext> logger
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
    // DoraReveal 通知の差分検出用。Haipai 直後に初期化し、Wall.DoraRevealedCount が増えた際の差分のみ通知する
    private int lastDoraRevealedCount_;

    public RoundState State
    {
        get => field ?? throw new InvalidOperationException("StartAsync() / 初期 State セットが呼び出されていません。");
        internal set;
    }

    public Round Round
    {
        get => field ?? throw new InvalidOperationException("StartAsync() / 初期 Round セットが呼び出されていません。");
        internal set;
    }

    protected virtual TimeSpan DisposeTimeout => TimeSpan.FromSeconds(5);

    /// <summary>
    /// OK応答イベントを発行する。Dispatch 経路 (内部) からのみ呼ばれる
    /// </summary>
    private async Task ResponseOkAsync()
    {
        await EnqueueEventAsync(new RoundEventResponseOk());
    }

    /// <summary>
    /// 状態遷移を伴わない <see cref="Round"/> の局所更新として、同巡フリテンを指定プレイヤーに適用する。
    /// 応答集約後・Ok/Call ディスパッチ前に内部から呼び出される。
    /// <para>
    /// 本メソッドは <see cref="Transit(RoundState, Func{Round, Round})"/> 経由の通常経路ではなく、
    /// 状態遷移を伴わないが <see cref="Round"/> を更新する必要がある局所修正 (同巡フリテン) のための
    /// 明示的な例外経路。その他の <see cref="Round"/> 更新は必ず Transit 経由で行う。
    /// イベントキュー (<c>Channel&lt;RoundEvent&gt;</c>) は介さず同期的に <see cref="Round"/> を更新する
    /// </para>
    /// </summary>
    private void ApplyTemporaryFuriten(ImmutableArray<PlayerIndex> playerIndices)
    {
        if (playerIndices.IsDefaultOrEmpty) { return; }

        Round = Round.ApplyTemporaryFuriten(playerIndices);
    }

    /// <summary>
    /// 和了応答イベントを発行する。Dispatch 経路 (内部) からのみ呼ばれる
    /// </summary>
    /// <param name="winnerIndices">和了者 (ダブロン/トリロンなら複数)</param>
    /// <param name="loserIndex">放銃者。ロン/槍槓では打牌者/加槓宣言者、ツモ/嶺上では <paramref name="winnerIndices"/>[0] と同値</param>
    /// <param name="winType">和了種別</param>
    private async Task ResponseWinAsync(ImmutableArray<PlayerIndex> winnerIndices, PlayerIndex loserIndex, WinType winType)
    {
        await EnqueueEventAsync(new RoundEventResponseWin(winnerIndices, loserIndex, winType));
    }

    /// <summary>
    /// 打牌応答イベントを発行する。Dispatch 経路 (内部) からのみ呼ばれる
    /// </summary>
    /// <param name="tile">打牌する牌</param>
    /// <param name="isRiichi">この打牌で立直宣言するか (既定: false)。
    /// 立直宣言は打牌時に保留状態となり、後続の応答 (ロンなし=確定、ロン=不成立) で確定/破棄される</param>
    private async Task ResponseDahaiAsync(Tile tile, bool isRiichi = false)
    {
        await EnqueueEventAsync(new RoundEventResponseDahai(tile, isRiichi));
    }

    /// <summary>
    /// 槓応答イベントを発行する。Dispatch 経路 (内部) からのみ呼ばれる
    /// </summary>
    /// <param name="type">Ankan または Kakan</param>
    /// <param name="tile">暗槓: 槓する牌種の牌、加槓: 追加する手牌の牌</param>
    private async Task ResponseKanAsync(CallType type, Tile tile)
    {
        await EnqueueEventAsync(new RoundEventResponseKan(type, tile));
    }

    /// <summary>
    /// 副露応答イベントを発行する。Dispatch 経路 (内部) からのみ呼ばれる
    /// </summary>
    /// <param name="callerIndex">副露するプレイヤー</param>
    /// <param name="type">Chi / Pon / Daiminkan</param>
    /// <param name="handTiles">callerの手牌から使用する牌 (Chi・Pon: 2枚、Daiminkan: 3枚)</param>
    /// <param name="calledTile">鳴かれる牌 (直前の打牌)</param>
    private async Task ResponseCallAsync(
        PlayerIndex callerIndex,
        CallType type,
        ImmutableList<Tile> handTiles,
        Tile calledTile
    )
    {
        await EnqueueEventAsync(new RoundEventResponseCall(callerIndex, type, handTiles, calledTile));
    }

    /// <summary>
    /// 流局応答イベントを発行する。Dispatch 経路 (内部) からのみ呼ばれる
    /// </summary>
    /// <param name="type">流局種別</param>
    /// <param name="tenpaiPlayers">テンパイ者 (荒牌平局時のみ意味を持つ、他は空配列可)</param>
    private async Task ResponseRyuukyokuAsync(RyuukyokuType type, ImmutableArray<PlayerIndex> tenpaiPlayers)
    {
        await EnqueueEventAsync(new RoundEventResponseRyuukyoku(type, tenpaiPlayers));
    }

    /// <summary>
    /// 遷移先状態を生成して遷移します。
    /// 順序は Exit → updateRound 適用 (null 以外のとき) → createNextState 評価 → State 差替 → Entry。
    /// updateRound が例外を投げた場合は <see cref="Round"/> が部分更新されないことを保証する
    /// (新 Round は updateRound が正常終了した場合のみ代入される)。
    /// updateRound 適用後の <see cref="Round"/> を遷移先のプロパティへ封入したい場合は
    /// createNextState 内で <c>this.Round</c> を参照する (例: <see cref="Impl.RoundStateCall.SnapshotRound"/>)
    /// </summary>
    /// <param name="createNextState">updateRound 適用後に評価される遷移先状態ファクトリ</param>
    /// <param name="updateRound">遷移時 Round 更新関数。null の場合は Round を更新しない</param>
    internal void Transit(Func<RoundState> createNextState, Func<Round, Round>? updateRound = null)
    {
        State.Exit(this);
        if (updateRound is not null)
        {
            Round = updateRound(Round);
        }
        State = createNextState();
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
        OnRuntimeRoundEnded(args);
        RoundEnded?.Invoke(this, args);
    }

    /// <summary>
    /// 状態遷移イベントを処理します。
    /// 個々のイベント処理で発生した例外 (不正な応答等) はループを止めずに観測用イベントへ通知します
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
                            throw new NotSupportedException($"未対応のイベント種別:{evt.GetType().Name}");
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Channel<RoundEvent> からは non-null の evt のみ得られるため、exception 通知時も非 null で扱う
                    logger.LogError(ex, "State.Response{Event} 処理で例外が発生しました。", evt.GetType().Name);
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
            throw new InvalidOperationException("StartAsync() が呼び出されていません。");
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
            DisposeRuntime();

            eventChannel_.Writer.Complete();
            // DisposeRuntime 内で cancellationTokenSource_.Cancel() 済みのため ProcessEventAsync は
            // ReadAllAsync から OCE で抜け Task は Canceled 状態で完了する。Wait はこの場合 AggregateException(TaskCanceledException)
            // を投げるので try/catch で握り潰し、リーク防止のため最後に cts を Dispose する
            if (eventProcessingTask_ is not null)
            {
                try
                {
                    if (!eventProcessingTask_.Wait(DisposeTimeout))
                    {
                        cancellationTokenSource_.Cancel();
                        eventProcessingTask_.Wait(DisposeTimeout);
                    }
                }
                catch
                {
                    // 破棄時は握り潰す (キャンセル由来の AggregateException 等)
                }
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
