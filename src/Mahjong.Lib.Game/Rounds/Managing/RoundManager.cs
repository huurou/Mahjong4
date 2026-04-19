using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Threading.Channels;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 1局分の通知・応答集約を担う
/// プレイヤーへ通知を送信し、応答を集約・優先順位解決したうえで <see cref="RoundStateContext"/> に委譲する
/// </summary>
/// <remarks>M4 対応: 継承不要のため sealed とし Dispose パターンを単純化している</remarks>
public sealed class RoundManager(
    PlayerList players,
    IRoundViewProjector projector,
    IResponseCandidateEnumerator enumerator,
    IResponsePriorityPolicy priorityPolicy,
    IDefaultResponseFactory defaultFactory,
    ITenpaiChecker tenpaiChecker,
    IScoreCalculator scoreCalculator,
    IGameTracer tracer,
    ILogger<RoundManager> logger
) : IDisposable
{
    /// <summary>
    /// プレイヤー応答の既定タイムアウト
    /// </summary>
    public static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(10);

    private readonly Channel<RoundState> stateChannel_ = Channel.CreateUnbounded<RoundState>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        }
    );

    private RoundStateContext? context_;
    private Task? mainLoopTask_;
    private TaskCompletionSource<RoundEndedEventArgs>? roundEndedTcs_;
    private PlayerResponse? pendingAfterKanTsumoResponse_;
    private bool disposed_;

    /// <summary>
    /// 局終了イベント (和了 / 流局)
    /// </summary>
    public event EventHandler<RoundEndedEventArgs>? RoundEnded;

    /// <summary>
    /// 内部で保持している <see cref="RoundStateContext"/> (StartAsync 未呼び出し時は null)。
    /// GameStateContext が局終了時に <see cref="RoundStateContext.Round"/> を参照するために公開する
    /// </summary>
    internal RoundStateContext? InternalContext => context_;

    /// <summary>
    /// 1 局分の通知・応答ループを開始します
    /// 戻り値 <see cref="Task{RoundEndedEventArgs}"/> は局終了時に完了します
    /// </summary>
    public Task<RoundEndedEventArgs> StartAsync(Round initial, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
        ArgumentNullException.ThrowIfNull(initial);
        if (context_ is not null)
        {
            throw new InvalidOperationException("StartAsync() は既に呼び出されています。");
        }

        roundEndedTcs_ = new TaskCompletionSource<RoundEndedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        context_ = new RoundStateContext(tenpaiChecker, scoreCalculator);
        context_.RoundStateChanged += OnRoundStateChanged;
        context_.RoundEnded += OnRoundEnded;
        context_.InvalidEventReceived += OnInvalidEventReceived;

        tracer.OnRoundStarted(initial);
        context_.Init(initial);

        mainLoopTask_ = Task.Run(() => ProcessAsync(ct), ct);
        return roundEndedTcs_.Task;
    }

    /// <summary>
    /// <see cref="AdoptedWinAction"/> 境界変換ヘルパ
    /// <see cref="WinType.Tsumo"/> / <see cref="WinType.Rinshan"/> のとき loserIndex を self → null に正規化する
    /// </summary>
    internal static PlayerIndex? NormalizeLoserIndex(PlayerIndex contextLoserIndex, WinType winType)
    {
        ArgumentNullException.ThrowIfNull(contextLoserIndex);
        return winType is WinType.Tsumo or WinType.Rinshan ? null : contextLoserIndex;
    }

    private void OnRoundStateChanged(object? sender, RoundStateChangedEventArgs args)
    {
        stateChannel_.Writer.TryWrite(args.State);
    }

    private void OnRoundEnded(object? sender, RoundEndedEventArgs args)
    {
        tracer.OnRoundEnded(BuildAdoptedActionForTrace(args));
        RoundEnded?.Invoke(this, args);
        roundEndedTcs_?.TrySetResult(args);
        stateChannel_.Writer.TryComplete();
    }

    private void OnInvalidEventReceived(object? sender, InvalidRoundEventEventArgs args)
    {
        logger.LogError(args.Exception, "不正な応答イベントが検出されました。event:{Event}", args.Event.Name);
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var state in stateChannel_.Reader.ReadAllAsync(ct))
            {
                if (context_ is null) { break; }

                if (state is RoundStateAfterKanTsumo && pendingAfterKanTsumoResponse_ is not null)
                {
                    var pending = pendingAfterKanTsumoResponse_;
                    pendingAfterKanTsumoResponse_ = null;
                    await DispatchAfterKanTsumoAsync(pending);
                    continue;
                }

                // 副露通知の整合性確保: 大明槓は本状態後に RinshanTsumo を伴う KanTsumo へ遷移するため、
                // CollectResponsesAsync の通知送信は state.SnapshotRound (副露直後・RinshanTsumo 前) を参照する必要がある。
                // 通知ビルド/ディスパッチは共通フローに乗せる
                var round = state is RoundStateCall call && call.SnapshotRound is not null
                    ? call.SnapshotRound
                    : context_.Round;
                var spec = state.CreateInquirySpec(round, enumerator);
                var responses = await CollectResponsesAsync(state, round, spec, ct);
                var adopted = priorityPolicy.Resolve(spec, responses);
                ApplyTemporaryFuritenIfRonMissed(spec, responses);
                foreach (var adoptedResponse in adopted)
                {
                    tracer.OnAdoptedAction(spec.Phase, adoptedResponse);
                }
                await DispatchAsync(spec, adopted);
            }
        }
        catch (OperationCanceledException)
        {
            // キャンセル時は呼び出し側が StartAsync の Task を永続待機しないよう明示的にキャンセル完了させる
            roundEndedTcs_?.TrySetCanceled(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RoundManager メインループで例外が発生しました。");
            roundEndedTcs_?.TrySetException(ex);
        }
    }

    /// <summary>
    /// 打牌フェーズでロン候補を提示されたが Ron 応答しなかったプレイヤーに同巡フリテンを適用する。
    /// 候補外応答で Ron が却下され Ok に置換された場合も「Ron 応答でない」に該当するため同じ扱い。
    /// </summary>
    private void ApplyTemporaryFuritenIfRonMissed(RoundInquirySpec spec, ImmutableArray<AdoptedPlayerResponse> responses)
    {
        if (context_ is null) { return; }
        if (spec.Phase != RoundInquiryPhase.Dahai) { return; }
        if (responses.Any(x => x.Response is RonResponse)) { return; }

        var responseByIndex = responses.ToDictionary(x => x.PlayerIndex, x => x.Response);
        foreach (var playerSpec in spec.PlayerSpecs)
        {
            if (playerSpec.CandidateList.HasCandidate<RonCandidate>()
                && responseByIndex.TryGetValue(playerSpec.PlayerIndex, out var response)
                && response is not RonResponse)
            {
                context_.Round = context_.Round.SetTemporaryFuriten(playerSpec.PlayerIndex, true);
            }
        }
    }

    private async Task<ImmutableArray<AdoptedPlayerResponse>> CollectResponsesAsync(
        RoundState state,
        Round round,
        RoundInquirySpec spec,
        CancellationToken ct
    )
    {
        var tasks = spec.PlayerSpecs
            .Select(x => CollectSingleAsync(state, round, spec.Phase, x, ct))
            .ToArray();
        var results = await Task.WhenAll(tasks);
        return [.. results];
    }

    private async Task<AdoptedPlayerResponse> CollectSingleAsync(
        RoundState state,
        Round round,
        RoundInquiryPhase phase,
        PlayerInquirySpec playerSpec,
        CancellationToken ct
    )
    {
        var notificationId = NotificationId.NewId();
        var notification = BuildNotification(state, round, playerSpec);
        tracer.OnNotificationSent(notificationId, playerSpec.PlayerIndex, notification);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(DefaultTimeout);

        try
        {
            var response = await InvokePlayerAsync(notification, playerSpec.PlayerIndex, linkedCts.Token);
            tracer.OnResponseReceived(notificationId, playerSpec.PlayerIndex, response);
            if (!ResponseValidator.IsResponseInCandidates(response, playerSpec.CandidateList))
            {
                tracer.OnInvalidResponse(notificationId, playerSpec.PlayerIndex, response, playerSpec.CandidateList);
                logger.LogWarning(
                    "候補外応答 player:{Index} phase:{Phase} response:{Response}",
                    playerSpec.PlayerIndex.Value, phase, response.GetType().Name
                );
                var fallback = defaultFactory.CreateDefault(playerSpec, phase);
                return new AdoptedPlayerResponse(playerSpec.PlayerIndex, fallback);
            }
            return new AdoptedPlayerResponse(playerSpec.PlayerIndex, response);
        }
        catch (OperationCanceledException)
        {
            tracer.OnResponseTimeout(notificationId, playerSpec.PlayerIndex);
            logger.LogWarning("プレイヤー応答タイムアウト player:{Index} phase:{Phase}", playerSpec.PlayerIndex.Value, phase);
            var fallback = defaultFactory.CreateDefault(playerSpec, phase);
            return new AdoptedPlayerResponse(playerSpec.PlayerIndex, fallback);
        }
        catch (Exception ex)
        {
            tracer.OnResponseException(notificationId, playerSpec.PlayerIndex, ex);
            logger.LogWarning(ex, "プレイヤー応答例外 player:{Index} phase:{Phase}", playerSpec.PlayerIndex.Value, phase);
            var fallback = defaultFactory.CreateDefault(playerSpec, phase);
            return new AdoptedPlayerResponse(playerSpec.PlayerIndex, fallback);
        }
    }

    private async Task<PlayerResponse> InvokePlayerAsync(RoundNotification notification, PlayerIndex recipientIndex, CancellationToken ct)
    {
        var player = players[recipientIndex];
        return notification switch
        {
            HaipaiNotification n => await player.OnHaipaiAsync(n, ct),
            TsumoNotification n => await player.OnTsumoAsync(n, ct),
            DahaiNotification n => await player.OnDahaiAsync(n, ct),
            KanNotification n => await player.OnKanAsync(n, ct),
            KanTsumoNotification n => await player.OnKanTsumoAsync(n, ct),
            CallNotification n => await player.OnCallAsync(n, ct),
            WinNotification n => await player.OnWinAsync(n, ct),
            RyuukyokuNotification n => await player.OnRyuukyokuAsync(n, ct),
            _ => throw new NotSupportedException($"意思決定通知として未対応の通知種別です。実際:{notification.GetType().Name}"),
        };
    }

    private RoundNotification BuildNotification(RoundState state, Round round, PlayerInquirySpec playerSpec)
    {
        var view = projector.Project(round, playerSpec.PlayerIndex);
        return state switch
        {
            RoundStateHaipai => new HaipaiNotification(view),
            RoundStateTsumo => new TsumoNotification(view, round.HandArray[round.Turn].Last(), playerSpec.CandidateList),
            RoundStateDahai => BuildDahaiNotification(view, round, playerSpec.CandidateList),
            RoundStateKan kan => BuildKanNotification(view, round, kan, playerSpec.CandidateList),
            RoundStateKanTsumo => new KanTsumoNotification(view, round.HandArray[round.Turn].Last(), playerSpec.CandidateList),
            RoundStateAfterKanTsumo => new KanTsumoNotification(view, round.HandArray[round.Turn].Last(), playerSpec.CandidateList),
            RoundStateCall => BuildCallNotification(view, round),
            RoundStateWin win => new WinNotification(view, (AdoptedWinAction)BuildAdoptedActionForTrace(win.EventArgs)),
            RoundStateRyuukyoku ryu => new RyuukyokuNotification(view, (AdoptedRyuukyokuAction)BuildAdoptedActionForTrace(ryu.EventArgs)),
            _ => throw new NotSupportedException($"意思決定通知として未対応の状態です。実際:{state.Name}"),
        };
    }

    private static CallNotification BuildCallNotification(PlayerRoundView view, Round round)
    {
        var callerIndex = round.Turn;
        var madeCall = round.CallListArray[callerIndex].Last();
        return new CallNotification(view, madeCall, callerIndex);
    }

    private static DahaiNotification BuildDahaiNotification(PlayerRoundView view, Round round, CandidateList candidates)
    {
        var discarderIndex = round.Turn;
        var discardedTile = round.RiverArray[discarderIndex].Last();
        return new DahaiNotification(view, discardedTile, discarderIndex, candidates);
    }

    private static KanNotification BuildKanNotification(PlayerRoundView view, Round round, RoundStateKan kan, CandidateList candidates)
    {
        var kanCall = round.CallListArray[round.Turn].Last(x => x.Type == kan.KanType);
        return new KanNotification(view, kanCall, round.Turn, candidates);
    }

    private async Task DispatchAsync(RoundInquirySpec spec, ImmutableArray<AdoptedPlayerResponse> adopted)
    {
        if (context_ is null) { return; }

        switch (spec.Phase)
        {
            case RoundInquiryPhase.Haipai:
            case RoundInquiryPhase.Call:
            case RoundInquiryPhase.Win:
            case RoundInquiryPhase.Ryuukyoku:
                // 通知観測フェーズ: 全員 OK 応答を集約し ResponseOk で次状態へ進める
                await context_.ResponseOkAsync();
                break;

            case RoundInquiryPhase.Tsumo:
                await DispatchTsumoAsync(adopted[0]);
                break;

            case RoundInquiryPhase.Dahai:
                await DispatchDahaiAsync(adopted, spec.LoserIndex!);
                break;

            case RoundInquiryPhase.Kan:
                await DispatchKanAsync(adopted, spec.LoserIndex!);
                break;

            case RoundInquiryPhase.KanTsumo:
                await DispatchKanTsumoAsync(adopted[0]);
                break;

            case RoundInquiryPhase.AfterKanTsumo:
                await DispatchAfterKanTsumoAsync(adopted[0].Response);
                break;

            default:
                throw new InvalidOperationException($"未対応のフェーズです。実際:{spec.Phase}");
        }
    }

    private async Task DispatchTsumoAsync(AdoptedPlayerResponse adopted)
    {
        if (context_ is null) { return; }

        switch (adopted.Response)
        {
            case DahaiResponse d:
                await context_.ResponseDahaiAsync(d.Tile, d.IsRiichi);
                break;

            case AnkanResponse a:
                await context_.ResponseKanAsync(CallType.Ankan, a.Tile);
                break;

            case KakanResponse k:
                await context_.ResponseKanAsync(CallType.Kakan, k.Tile);
                break;

            case TsumoAgariResponse:
                await context_.ResponseWinAsync([adopted.PlayerIndex], adopted.PlayerIndex, WinType.Tsumo);
                break;

            case KyuushuKyuuhaiResponse:
                await context_.ResponseRyuukyokuAsync(RyuukyokuType.KyuushuKyuuhai, []);
                break;

            default:
                throw new InvalidOperationException($"ツモフェーズの応答として未対応です。実際:{adopted.Response.GetType().Name}");
        }
    }

    private async Task DispatchDahaiAsync(ImmutableArray<AdoptedPlayerResponse> adopted, PlayerIndex loserIndex)
    {
        if (context_ is null) { return; }

        var primary = adopted[0].Response;
        switch (primary)
        {
            case RonResponse:
                var winners = adopted
                    .Where(x => x.Response is RonResponse)
                    .Select(x => x.PlayerIndex)
                    .ToImmutableArray();
                await context_.ResponseWinAsync(winners, loserIndex, WinType.Ron);
                break;

            case ChiResponse chi:
                await context_.ResponseCallAsync(adopted[0].PlayerIndex, CallType.Chi, [.. chi.HandTiles], GetDiscardedTile(loserIndex));
                break;

            case PonResponse pon:
                await context_.ResponseCallAsync(adopted[0].PlayerIndex, CallType.Pon, [.. pon.HandTiles], GetDiscardedTile(loserIndex));
                break;

            case DaiminkanResponse daiminkan:
                await context_.ResponseCallAsync(adopted[0].PlayerIndex, CallType.Daiminkan, [.. daiminkan.HandTiles], GetDiscardedTile(loserIndex));
                break;

            case OkResponse:
                await context_.ResponseOkAsync();
                break;

            default:
                throw new InvalidOperationException($"打牌フェーズの応答として未対応です。実際:{primary.GetType().Name}");
        }
    }

    private async Task DispatchKanAsync(ImmutableArray<AdoptedPlayerResponse> adopted, PlayerIndex loserIndex)
    {
        if (context_ is null) { return; }

        var primary = adopted[0].Response;
        switch (primary)
        {
            case ChankanRonResponse:
                var winners = adopted
                    .Where(x => x.Response is ChankanRonResponse)
                    .Select(x => x.PlayerIndex)
                    .ToImmutableArray();
                await context_.ResponseWinAsync(winners, loserIndex, WinType.Chankan);
                break;

            case OkResponse:
                await context_.ResponseOkAsync();
                break;

            default:
                throw new InvalidOperationException($"槓フェーズの応答として未対応です。実際:{primary.GetType().Name}");
        }
    }

    private async Task DispatchKanTsumoAsync(AdoptedPlayerResponse adopted)
    {
        if (context_ is null) { return; }

        if (adopted.Response is RinshanTsumoResponse)
        {
            await context_.ResponseWinAsync([adopted.PlayerIndex], adopted.PlayerIndex, WinType.Rinshan);
            return;
        }

        if (adopted.Response is not (KanTsumoDahaiResponse or KanTsumoAnkanResponse or KanTsumoKakanResponse))
        {
            throw new InvalidOperationException($"嶺上ツモフェーズの応答として未対応です。実際:{adopted.Response.GetType().Name}");
        }

        // 2 段階ディスパッチ: ResponseOk で RoundStateAfterKanTsumo に遷移 → メインループが pending を消費してアクションをディスパッチ
        pendingAfterKanTsumoResponse_ = adopted.Response;
        try
        {
            await context_.ResponseOkAsync();
        }
        catch
        {
            pendingAfterKanTsumoResponse_ = null;
            throw;
        }
    }

    private async Task DispatchAfterKanTsumoAsync(PlayerResponse response)
    {
        if (context_ is null) { return; }

        switch (response)
        {
            case KanTsumoDahaiResponse d:
                await context_.ResponseDahaiAsync(d.Tile, d.IsRiichi);
                break;

            case KanTsumoAnkanResponse a:
                await context_.ResponseKanAsync(CallType.Ankan, a.Tile);
                break;

            case KanTsumoKakanResponse k:
                await context_.ResponseKanAsync(CallType.Kakan, k.Tile);
                break;

            default:
                throw new InvalidOperationException($"嶺上ツモ後フェーズの応答として未対応です。実際:{response.GetType().Name}");
        }
    }

    private Tile GetDiscardedTile(PlayerIndex discarderIndex)
    {
        if (context_ is null) { throw new InvalidOperationException("Context is null."); }

        return context_.Round.RiverArray[discarderIndex].Last();
    }

    private static AdoptedRoundAction BuildAdoptedActionForTrace(RoundEndedEventArgs args)
    {
        return args switch
        {
            RoundEndedByWinEventArgs win => new AdoptedWinAction(
                winnerIndices: [.. (win.Winners.IsDefaultOrEmpty
                    ? win.WinnerIndices.Select(x => new AdoptedWinner(x, default!, default!))
                    : win.Winners)],
                loserIndex: NormalizeLoserIndex(win.LoserIndex, win.WinType),
                winType: win.WinType,
                kyoutakuRiichiAward: win.KyoutakuRiichiAward,
                honba: win.Honba ?? new Honba(0),
                dealerContinues: false
            ),
            RoundEndedByRyuukyokuEventArgs ryu => new AdoptedRyuukyokuAction(
                Type: ryu.Type,
                TenpaiPlayerIndices: [.. ryu.TenpaiPlayerIndices],
                NagashiManganPlayerIndices: [.. ryu.NagashiManganPlayerIndices],
                DealerContinues: false
            ),
            _ => throw new NotSupportedException($"未対応の局終了引数: {args?.GetType().Name}"),
        };
    }

    public void Dispose()
    {
        if (disposed_) { return; }

        stateChannel_.Writer.TryComplete();

        if (context_ is not null)
        {
            context_.RoundStateChanged -= OnRoundStateChanged;
            context_.RoundEnded -= OnRoundEnded;
            context_.InvalidEventReceived -= OnInvalidEventReceived;
            context_.Dispose();
            context_ = null;
        }

        if (mainLoopTask_ is not null)
        {
            try
            {
                mainLoopTask_.Wait(TimeSpan.FromSeconds(5));
            }
            catch
            {
                // 破棄時は握り潰す
            }
        }

        disposed_ = true;
    }
}
