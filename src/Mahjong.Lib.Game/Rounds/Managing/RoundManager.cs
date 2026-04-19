using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tenpai;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Threading.Channels;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 1局分の通知・応答集約を担う
/// プレイヤーへ通知を送信し、応答を集約・優先順位解決したうえで <see cref="RoundStateContext"/> に委譲する。
/// <para>
/// 通知生成は <see cref="IRoundNotificationBuilder"/>、応答のイベント化ディスパッチは <see cref="IResponseDispatcher"/> に分離しており、
/// 本クラスはメインループ・プレイヤー呼び出し (タイムアウト/例外制御)・優先順位解決とフリテン検出・ライフサイクル管理に専念する
/// </para>
/// </summary>
public sealed class RoundManager(
    PlayerList players,
    IRoundViewProjector projector,
    IResponseCandidateEnumerator enumerator,
    IResponsePriorityPolicy priorityPolicy,
    IDefaultResponseFactory defaultFactory,
    IRoundNotificationBuilder notificationBuilder,
    IResponseDispatcher dispatcher,
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

    private void OnRoundStateChanged(object? sender, RoundStateChangedEventArgs args)
    {
        stateChannel_.Writer.TryWrite(args.State);
    }

    private void OnRoundEnded(object? sender, RoundEndedEventArgs args)
    {
        tracer.OnRoundEnded(AdoptedRoundActionBuilder.Build(args));
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

                // KanTsumo → AfterKanTsumo 遷移時に溜まった pending 応答を消費
                if (state is RoundStateAfterKanTsumo && pendingAfterKanTsumoResponse_ is not null)
                {
                    var pending = pendingAfterKanTsumoResponse_;
                    pendingAfterKanTsumoResponse_ = null;
                    await dispatcher.DispatchAfterKanTsumoAsync(context_, pending);
                    continue;
                }

                // 副露通知の整合性確保: 大明槓は本状態後に RinshanTsumo を伴う KanTsumo へ遷移するため、
                // CollectResponsesAsync の通知送信は state.SnapshotRound (副露直後・RinshanTsumo 前) を参照する必要がある
                var round = state is RoundStateCall call && call.SnapshotRound is not null
                    ? call.SnapshotRound
                    : context_.Round;
                var spec = state.CreateInquirySpec(round, enumerator);
                var responses = await CollectResponsesAsync(state, round, spec, ct);
                var adopted = priorityPolicy.Resolve(spec, responses);
                foreach (var adoptedResponse in adopted)
                {
                    tracer.OnAdoptedAction(spec.Phase, adoptedResponse);
                }

                // ロン見逃しによる同巡フリテンを Ok/Call ディスパッチ前に直接 Round へ反映。
                // 状態遷移を伴わない局所更新のため event queue を介さず同期メソッド呼び出し
                var temporaryFuritenPlayers = DetectRonMissedFuritenPlayers(spec, responses);
                context_.ApplyTemporaryFuriten(temporaryFuritenPlayers);
                try
                {
                    var pending = await dispatcher.DispatchAsync(context_, spec, adopted);
                    if (pending is not null)
                    {
                        // KanTsumo のアクション応答: 後続 AfterKanTsumo 遷移時に消費する
                        pendingAfterKanTsumoResponse_ = pending;
                    }
                }
                catch
                {
                    pendingAfterKanTsumoResponse_ = null;
                    throw;
                }
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
    /// 打牌フェーズでロン候補を提示されたが Ron 応答しなかったプレイヤーを検出して返します。
    /// 候補外応答で Ron が却下され Ok に置換された場合も「Ron 応答でない」に該当するため同じ扱い。
    /// ロン応答が 1 件でも含まれる場合は空配列を返す (ロン成立シナリオでは同巡フリテンは無関係)。
    /// 結果は <see cref="IResponseDispatcher.DispatchAsync"/> 経由で RoundStateContext のイベントに詰められ、
    /// RoundState 内部で <see cref="Round.ApplyTemporaryFuriten"/> により適用される
    /// </summary>
    private static ImmutableArray<PlayerIndex> DetectRonMissedFuritenPlayers(
        RoundInquirySpec spec,
        ImmutableArray<AdoptedPlayerResponse> responses
    )
    {
        if (spec.Phase != RoundInquiryPhase.Dahai) { return []; }
        if (responses.Any(x => x.Response is RonResponse)) { return []; }

        var responseByIndex = responses.ToDictionary(x => x.PlayerIndex, x => x.Response);
        var builder = ImmutableArray.CreateBuilder<PlayerIndex>();
        foreach (var playerSpec in spec.PlayerSpecs)
        {
            if (playerSpec.CandidateList.HasCandidate<Candidates.RonCandidate>()
                && responseByIndex.TryGetValue(playerSpec.PlayerIndex, out var response)
                && response is not RonResponse)
            {
                builder.Add(playerSpec.PlayerIndex);
            }
        }
        return builder.ToImmutable();
    }

    private async Task<ImmutableArray<AdoptedPlayerResponse>> CollectResponsesAsync(
        RoundState state,
        Round round,
        RoundInquirySpec spec,
        CancellationToken ct
    )
    {
        var tasks = spec.PlayerSpecs
            .Select(x => CollectSingleAsync(state, round, spec, x, ct))
            .ToArray();
        var results = await Task.WhenAll(tasks);
        return [.. results];
    }

    private async Task<AdoptedPlayerResponse> CollectSingleAsync(
        RoundState state,
        Round round,
        RoundInquirySpec spec,
        PlayerInquirySpec playerSpec,
        CancellationToken ct
    )
    {
        var notificationId = NotificationId.NewId();
        var notification = notificationBuilder.Build(state, round, spec, playerSpec, projector);
        var isInquired = spec.IsInquired(playerSpec.PlayerIndex);
        tracer.OnNotificationSent(notificationId, playerSpec.PlayerIndex, notification);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(DefaultTimeout);

        PlayerResponse response;
        try
        {
            response = await InvokePlayerAsync(notification, playerSpec.PlayerIndex, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            tracer.OnResponseTimeout(notificationId, playerSpec.PlayerIndex);
            logger.LogWarning("プレイヤー応答タイムアウト player:{Index} phase:{Phase}", playerSpec.PlayerIndex.Value, spec.Phase);
            var fallback = defaultFactory.CreateDefault(playerSpec, spec.Phase);
            return new AdoptedPlayerResponse(playerSpec.PlayerIndex, fallback);
        }
        catch (Exception ex)
        {
            tracer.OnResponseException(notificationId, playerSpec.PlayerIndex, ex);
            logger.LogWarning(ex, "プレイヤー応答例外 player:{Index} phase:{Phase}", playerSpec.PlayerIndex.Value, spec.Phase);
            var fallback = defaultFactory.CreateDefault(playerSpec, spec.Phase);
            return new AdoptedPlayerResponse(playerSpec.PlayerIndex, fallback);
        }

        tracer.OnResponseReceived(notificationId, playerSpec.PlayerIndex, response);
        if (!ResponseValidator.IsResponseInCandidates(response, playerSpec.CandidateList))
        {
            tracer.OnInvalidResponse(notificationId, playerSpec.PlayerIndex, response, playerSpec.CandidateList);
            if (!isInquired)
            {
                // 問い合わせ対象外 (= OK のみ許可) のプレイヤーが非 OK 応答を返すのはクライアント契約違反
                throw new InvalidOperationException(
                    $"問い合わせ対象外プレイヤー {playerSpec.PlayerIndex.Value} が phase:{spec.Phase} で非 OK 応答を返しました。応答型:{response.GetType().Name}"
                );
            }
            logger.LogWarning(
                "候補外応答 player:{Index} phase:{Phase} response:{Response}",
                playerSpec.PlayerIndex.Value, spec.Phase, response.GetType().Name
            );
            var fallback = defaultFactory.CreateDefault(playerSpec, spec.Phase);
            return new AdoptedPlayerResponse(playerSpec.PlayerIndex, fallback);
        }
        return new AdoptedPlayerResponse(playerSpec.PlayerIndex, response);
    }

    private async Task<PlayerResponse> InvokePlayerAsync(RoundNotification notification, PlayerIndex recipientIndex, CancellationToken ct)
    {
        var player = players[recipientIndex];
        return notification switch
        {
            HaipaiNotification n => await player.OnHaipaiAsync(n, ct),
            TsumoNotification n => await player.OnTsumoAsync(n, ct),
            OtherPlayerTsumoNotification n => await player.OnOtherPlayerTsumoAsync(n, ct),
            DahaiNotification n => await player.OnDahaiAsync(n, ct),
            KanNotification n => await player.OnKanAsync(n, ct),
            KanTsumoNotification n => await player.OnKanTsumoAsync(n, ct),
            OtherPlayerKanTsumoNotification n => await player.OnOtherPlayerKanTsumoAsync(n, ct),
            CallNotification n => await player.OnCallAsync(n, ct),
            WinNotification n => await player.OnWinAsync(n, ct),
            RyuukyokuNotification n => await player.OnRyuukyokuAsync(n, ct),
            _ => throw new NotSupportedException($"意思決定通知として未対応の通知種別です。実際:{notification.GetType().Name}"),
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
