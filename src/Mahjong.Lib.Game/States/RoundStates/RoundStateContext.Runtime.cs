using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Mahjong.Lib.Game.States.RoundStates;

/// <summary>
/// 通知・応答集約のメインループ (本番駆動経路)。
/// <see cref="StartAsync(Round, CancellationToken)"/> で開始し、<see cref="RoundStateChanged"/> を購読して
/// state ごとに候補列挙 → プレイヤー問い合わせ → 優先順位解決 → フリテン検出 → dispatch のパイプラインを回す
/// </summary>
public partial class RoundStateContext
{
    private readonly Channel<RoundState> stateChannel_ = Channel.CreateUnbounded<RoundState>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        }
    );

    private Task? mainLoopTask_;
    private CancellationTokenSource? runtimeCts_;
    private TaskCompletionSource<RoundEndedEventArgs>? roundEndedTcs_;
    private PlayerResponse? pendingAfterKanTsumoResponse_;
    private bool runtimeStarted_;

    /// <summary>
    /// プレイヤー応答の既定タイムアウト
    /// </summary>
    public static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// 1 局分の通知・応答ループを開始します。
    /// 戻り値 <see cref="Task{RoundEndedEventArgs}"/> は局終了時 (和了 / 流局) に完了します。
    /// full-deps コンストラクタで生成された場合のみ呼び出せる
    /// </summary>
    public Task<RoundEndedEventArgs> StartAsync(Round initial, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
        if (runtimeStarted_)
        {
            throw new InvalidOperationException("StartAsync() は既に呼び出されています。");
        }

        runtimeStarted_ = true;
        roundEndedTcs_ = new TaskCompletionSource<RoundEndedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        RoundStateChanged += OnRuntimeStateChanged;

        State = new RoundStateHaipai();
        Round = initial.Haipai();
        // 牌譜記録側は配牌済みの Round を期待する (HandArray 等がキャプチャできるよう)
        tracer.OnRoundStarted(Round);
        // 配牌ドラは HaipaiNotification に含まれるため DoraRevealNotification は送らない規約。
        // Haipai 直後の DoraRevealedCount を初期値として保持し、槓由来の新ドラ増加のみ通知する
        lastDoraRevealedCount_ = Round.Wall.DoraRevealedCount;
        State.Entry(this);
        // Dispose 時に内部 cts のキャンセルで CollectResponsesAsync まで含めて ProcessRuntimeAsync を停止させるため、
        // 外部 ct と内部 cts をリンクしたトークンを mainLoopTask_ に渡す。
        // eventProcessingTask_ は既存どおり cancellationTokenSource_.Token を ReadAllAsync に渡して停止する
        runtimeCts_ = CancellationTokenSource.CreateLinkedTokenSource(ct, cancellationTokenSource_.Token);
        eventProcessingTask_ = Task.Run(ProcessEventAsync, cancellationTokenSource_.Token);
        mainLoopTask_ = Task.Run(() => ProcessRuntimeAsync(runtimeCts_.Token), runtimeCts_.Token);
        return roundEndedTcs_.Task;
    }

    private void OnRuntimeStateChanged(object? sender, RoundStateChangedEventArgs args)
    {
        stateChannel_.Writer.TryWrite(args.State);
    }

    private void OnRuntimeRoundEnded(RoundEndedEventArgs args)
    {
        tracer.OnRoundEnded(AdoptedRoundActionBuilder.Build(args));
        roundEndedTcs_?.TrySetResult(args);
        stateChannel_.Writer.TryComplete();
    }

    private async Task ProcessRuntimeAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var state in stateChannel_.Reader.ReadAllAsync(ct))
            {
                // KanTsumo → AfterKanTsumo 遷移時に溜まった pending 応答を消費
                if (state is RoundStateAfterKanTsumo && pendingAfterKanTsumoResponse_ is not null)
                {
                    var pending = pendingAfterKanTsumoResponse_;
                    pendingAfterKanTsumoResponse_ = null;
                    await DispatchAfterKanTsumoAsync(pending);
                    continue;
                }

                // 副露通知の整合性確保: 大明槓は本状態後に RinshanTsumo を伴う KanTsumo へ遷移するため、
                // CollectResponsesAsync の通知送信は state.SnapshotRound (副露直後・RinshanTsumo 前) を参照する必要がある
                var round = state is RoundStateCall call && call.SnapshotRound is not null
                    ? call.SnapshotRound
                    : Round;
                // 槓由来の新ドラ表示を検知した場合、意思決定通知の前に DoraRevealNotification を全員にブロードキャストする。
                // Wall.DoraRevealedCount は単調増加のため差分分だけ順次通知できる
                var currentDoraCount = Round.Wall.DoraRevealedCount;
                if (currentDoraCount > lastDoraRevealedCount_)
                {
                    await BroadcastDoraRevealAsync(lastDoraRevealedCount_, currentDoraCount, ct);
                    lastDoraRevealedCount_ = currentDoraCount;
                }
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
                ApplyTemporaryFuriten(temporaryFuritenPlayers);
                try
                {
                    var pending = await DispatchAsync(spec, adopted);
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
            logger.LogError(ex, "RoundStateContext メインループで例外が発生しました。");
            roundEndedTcs_?.TrySetException(ex);
        }
    }

    private void DisposeRuntime()
    {
        if (!runtimeStarted_) { return; }

        // 内部 cts のキャンセルで CollectResponsesAsync 内の player 応答待ち (最大 10 秒) を含めて
        // runtime ループを即時停止させる。本呼び出しより前は DefaultTimeout まで待ってしまいリークする
        cancellationTokenSource_.Cancel();
        stateChannel_.Writer.TryComplete();
        RoundStateChanged -= OnRuntimeStateChanged;

        if (mainLoopTask_ is not null)
        {
            try
            {
                mainLoopTask_.Wait(DisposeTimeout);
            }
            catch
            {
                // 破棄時は握り潰す
            }
        }

        runtimeCts_?.Dispose();
        runtimeCts_ = null;
        pendingAfterKanTsumoResponse_ = null;
    }
}
