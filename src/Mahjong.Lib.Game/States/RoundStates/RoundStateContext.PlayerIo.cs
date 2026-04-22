using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.RoundStates;

/// <summary>
/// 通知生成・プレイヤー応答収集・優先順位解決・フリテン検出・採用応答ディスパッチ。
/// 旧 RoundManager + RoundNotificationBuilder + ResponseDispatcher のロジックを inline 化したもの
/// </summary>
public partial class RoundStateContext
{
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
        var notification = BuildNotification(state, round, spec, playerSpec);
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

        // 2 段目: 意味的検証 (問い合わせ対象のみ)。1 段目通過後に手牌整合・フリテン・立直条件等を再確認する。
        // 失敗時はクライアント契約違反として InvalidOperationException を throw し進行を停止する
        // (3 段目 副作用防止は throw により Round 更新前に停止することで達成)
        if (isInquired)
        {
            var semantic = ResponseValidator.ValidateSemantic(response, round, playerSpec.PlayerIndex, spec.Phase);
            if (!semantic.IsValid)
            {
                tracer.OnInvalidResponse(notificationId, playerSpec.PlayerIndex, response, playerSpec.CandidateList);
                throw new InvalidOperationException(
                    $"意味的検証失敗 player:{playerSpec.PlayerIndex.Value} phase:{spec.Phase} response:{response.GetType().Name} reason:{semantic.Reason}"
                );
            }
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
            AfterCallNotification n => await player.OnAfterCallAsync(n, ct),
            OtherPlayerAfterCallNotification n => await player.OnOtherPlayerAfterCallAsync(n, ct),
            WinNotification n => await player.OnWinAsync(n, ct),
            RyuukyokuNotification n => await player.OnRyuukyokuAsync(n, ct),
            DoraRevealNotification n => await player.OnDoraRevealAsync(n, ct),
            _ => throw new NotSupportedException($"意思決定通知として未対応の通知種別です。実際:{notification.GetType().Name}"),
        };
    }

    /// <summary>
    /// 新しく表示されたドラ表示牌を全プレイヤーに観測通知する。
    /// 観測通知 (OK 応答のみ) のため候補検証・優先順位解決・Round 更新は行わない軽量パス。
    /// 応答は破棄し、個別プレイヤーの例外・タイムアウトはログ出力して握り潰す
    /// </summary>
    private async Task BroadcastDoraRevealAsync(int fromExclusive, int toInclusive, CancellationToken ct)
    {
        for (var n = fromExclusive; n < toInclusive; n++)
        {
            var newIndicator = Round.Wall.GetDoraIndicator(n);
            tracer.OnDoraRevealed(newIndicator);
            var tasks = new Task[PlayerIndex.PLAYER_COUNT];
            for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
            {
                var recipientIndex = new PlayerIndex(i);
                var view = projector.Project(Round, recipientIndex);
                var notification = new DoraRevealNotification(view, newIndicator, []);
                tasks[i] = InvokeDoraRevealAsync(notification, recipientIndex, ct);
            }
            await Task.WhenAll(tasks);
        }
    }

    private async Task InvokeDoraRevealAsync(DoraRevealNotification notification, PlayerIndex recipientIndex, CancellationToken ct)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(DefaultTimeout);
        try
        {
            await players[recipientIndex].OnDoraRevealAsync(notification, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("DoraReveal 通知タイムアウト player:{Index}", recipientIndex.Value);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "DoraReveal 通知で例外 player:{Index}", recipientIndex.Value);
        }
    }

    /// <summary>
    /// 打牌フェーズでロン候補を提示されたが Ron 応答しなかったプレイヤーを検出。
    /// 候補外応答で Ron が却下され Ok に置換された場合も「Ron 応答でない」に該当するため同じ扱い
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
            if (playerSpec.CandidateList.HasCandidate<RonCandidate>() &&
                responseByIndex.TryGetValue(playerSpec.PlayerIndex, out var response) &&
                response is not RonResponse)
            {
                builder.Add(playerSpec.PlayerIndex);
            }
        }
        return builder.ToImmutable();
    }

    private RoundNotification BuildNotification(
        RoundState state,
        Round round,
        RoundInquirySpec spec,
        PlayerInquirySpec playerSpec
    )
    {
        var view = projector.Project(round, playerSpec.PlayerIndex);
        var inquired = spec.InquiredPlayerIndices;
        var isInquired = spec.IsInquired(playerSpec.PlayerIndex);
        return state switch
        {
            RoundStateHaipai => new HaipaiNotification(view, inquired),
            RoundStateTsumo => isInquired
                // 問い合わせ対象 (手番): 自身のツモ牌を含む TsumoNotification
                ? new TsumoNotification(view, round.HandArray[round.Turn].Last(), playerSpec.CandidateList, inquired)
                // 非対象 (他家): ツモ牌は私的情報のため送らない OtherPlayerTsumoNotification
                : new OtherPlayerTsumoNotification(view, round.Turn, inquired),
            RoundStateDahai => BuildDahaiNotification(view, round, playerSpec.CandidateList, inquired),
            RoundStateKan kan => BuildKanNotification(view, round, kan, playerSpec.CandidateList, inquired),
            RoundStateKanTsumo => isInquired
                ? new KanTsumoNotification(view, round.HandArray[round.Turn].Last(), playerSpec.CandidateList, inquired)
                : new OtherPlayerKanTsumoNotification(view, round.Turn, inquired),
            RoundStateAfterKanTsumo => isInquired
                ? new KanTsumoNotification(view, round.HandArray[round.Turn].Last(), playerSpec.CandidateList, inquired)
                : new OtherPlayerKanTsumoNotification(view, round.Turn, inquired),
            RoundStateAfterCall => isInquired
                // 副露者には打牌選択を求める専用通知 (取得した牌 CalledTile を提示、打牌のみ許可)
                ? new AfterCallNotification(view, GetLastCalledTile(round, round.Turn), playerSpec.CandidateList, inquired)
                // 他家には「副露者が打牌選択中」の観測通知
                : new OtherPlayerAfterCallNotification(view, round.Turn, inquired),
            RoundStateCall => BuildCallNotification(view, round, inquired),
            RoundStateWin win => new WinNotification(view, (AdoptedWinAction)AdoptedRoundActionBuilder.Build(win.EventArgs), inquired),
            RoundStateRyuukyoku ryu => new RyuukyokuNotification(view, (AdoptedRyuukyokuAction)AdoptedRoundActionBuilder.Build(ryu.EventArgs), inquired),
            _ => throw new NotSupportedException($"意思決定通知として未対応の状態です。実際:{state.Name}"),
        };
    }

    private static CallNotification BuildCallNotification(PlayerRoundView view, Round round, ImmutableArray<PlayerIndex> inquired)
    {
        var callerIndex = round.Turn;
        var madeCall = round.CallListArray[callerIndex].Last();
        return new CallNotification(view, madeCall, callerIndex, inquired);
    }

    /// <summary>
    /// <see cref="RoundStateAfterCall"/> で副露者が直前に取得した牌 (= 副露の <see cref="Call.CalledTile"/>)
    /// を <see cref="AfterCallNotification.CalledTile"/> として提示するための取得ヘルパー。
    /// 副露時は必ず CalledTile が存在するため null にはならない想定 (暗槓はこの状態を経由しない)。
    /// </summary>
    private static Tile GetLastCalledTile(Round round, PlayerIndex callerIndex)
    {
        var lastCall = round.CallListArray[callerIndex].Last();
        return lastCall.CalledTile
            ?? throw new InvalidOperationException($"RoundStateAfterCall で CalledTile が null です。副露種別:{lastCall.Type}");
    }

    private static DahaiNotification BuildDahaiNotification(PlayerRoundView view, Round round, CandidateList candidates, ImmutableArray<PlayerIndex> inquired)
    {
        var discarderIndex = round.Turn;
        var discardedTile = round.RiverArray[discarderIndex].Last();
        return new DahaiNotification(view, discardedTile, discarderIndex, candidates, inquired);
    }

    private static KanNotification BuildKanNotification(PlayerRoundView view, Round round, RoundStateKan kan, CandidateList candidates, ImmutableArray<PlayerIndex> inquired)
    {
        var kanCall = round.CallListArray[round.Turn].Last(x => x.Type == kan.KanType);
        return new KanNotification(view, kanCall, round.Turn, candidates, inquired);
    }

    /// <summary>
    /// 採用応答をフェーズごとに ResponseXxxAsync イベントに変換してディスパッチする。
    /// KanTsumo でアクション応答が採用された場合、後続 AfterKanTsumo で消費する応答を戻り値で返す
    /// </summary>
    private async Task<PlayerResponse?> DispatchAsync(
        RoundInquirySpec spec,
        ImmutableArray<AdoptedPlayerResponse> adopted
    )
    {
        switch (spec.Phase)
        {
            case RoundInquiryPhase.Haipai:
            case RoundInquiryPhase.Call:
            case RoundInquiryPhase.Win:
            case RoundInquiryPhase.Ryuukyoku:
                // 通知観測フェーズ: 全員 OK 応答を集約し ResponseOk で次状態へ進める
                await ResponseOkAsync();
                return null;

            case RoundInquiryPhase.Tsumo:
                await DispatchTsumoAsync(FindInquiredResponse(spec, adopted));
                return null;

            case RoundInquiryPhase.Dahai:
                await DispatchDahaiAsync(FilterInquiredResponses(spec, adopted), spec.LoserIndex);
                return null;

            case RoundInquiryPhase.Kan:
                await DispatchKanAsync(FilterInquiredResponses(spec, adopted), spec.LoserIndex);
                return null;

            case RoundInquiryPhase.KanTsumo:
                return await DispatchKanTsumoAsync(FindInquiredResponse(spec, adopted));

            case RoundInquiryPhase.AfterKanTsumo:
                await DispatchAfterKanTsumoAsync(FindInquiredResponse(spec, adopted).Response);
                return null;

            case RoundInquiryPhase.AfterCall:
                await DispatchAfterCallAsync(FindInquiredResponse(spec, adopted).Response);
                return null;

            default:
                throw new InvalidOperationException($"未対応のフェーズです。実際:{spec.Phase}");
        }
    }

    /// <summary>
    /// 副露後フェーズの応答ディスパッチ。副露後は打牌のみ許可される
    /// </summary>
    private async Task DispatchAfterCallAsync(PlayerResponse pendingResponse)
    {
        if (pendingResponse is DahaiResponse d)
        {
            await ResponseDahaiAsync(d.Tile, d.IsRiichi);
            return;
        }
        throw new InvalidOperationException($"副露後フェーズの応答として未対応です。実際:{pendingResponse.GetType().Name}");
    }

    private async Task DispatchAfterKanTsumoAsync(PlayerResponse pendingResponse)
    {
        switch (pendingResponse)
        {
            case KanTsumoDahaiResponse d:
                await ResponseDahaiAsync(d.Tile, d.IsRiichi);
                break;

            case KanTsumoAnkanResponse a:
                await ResponseKanAsync(CallType.Ankan, a.Tile);
                break;

            case KanTsumoKakanResponse k:
                await ResponseKanAsync(CallType.Kakan, k.Tile);
                break;

            default:
                throw new InvalidOperationException($"嶺上ツモ後フェーズの応答として未対応です。実際:{pendingResponse.GetType().Name}");
        }
    }

    private async Task DispatchTsumoAsync(AdoptedPlayerResponse adopted)
    {
        switch (adopted.Response)
        {
            case DahaiResponse d:
                await ResponseDahaiAsync(d.Tile, d.IsRiichi);
                break;

            case AnkanResponse a:
                await ResponseKanAsync(CallType.Ankan, a.Tile);
                break;

            case KakanResponse k:
                await ResponseKanAsync(CallType.Kakan, k.Tile);
                break;

            case TsumoAgariResponse:
                await ResponseWinAsync([adopted.PlayerIndex], adopted.PlayerIndex, WinType.Tsumo);
                break;

            case KyuushuKyuuhaiResponse:
                await ResponseRyuukyokuAsync(RyuukyokuType.KyuushuKyuuhai, []);
                break;

            default:
                throw new InvalidOperationException($"ツモフェーズの応答として未対応です。実際:{adopted.Response.GetType().Name}");
        }
    }

    private async Task DispatchDahaiAsync(
        ImmutableArray<AdoptedPlayerResponse> adopted,
        PlayerIndex loserIndex
    )
    {
        var primary = adopted[0].Response;
        switch (primary)
        {
            case RonResponse:
                var winners = adopted
                    .Where(x => x.Response is RonResponse)
                    .Select(x => x.PlayerIndex)
                    .ToImmutableArray();
                if (winners.Length >= 3)
                {
                    // 三家和了 (SanchaHou): 3 人以上のロンは流局扱い
                    await ResponseRyuukyokuAsync(RyuukyokuType.SanchaHou, []);
                }
                else
                {
                    await ResponseWinAsync(winners, loserIndex, WinType.Ron);
                }
                break;

            case ChiResponse chi:
                await ResponseCallAsync(adopted[0].PlayerIndex, CallType.Chi, [.. chi.HandTiles], GetDiscardedTile(loserIndex));
                break;

            case PonResponse pon:
                await ResponseCallAsync(adopted[0].PlayerIndex, CallType.Pon, [.. pon.HandTiles], GetDiscardedTile(loserIndex));
                break;

            case DaiminkanResponse daiminkan:
                await ResponseCallAsync(adopted[0].PlayerIndex, CallType.Daiminkan, [.. daiminkan.HandTiles], GetDiscardedTile(loserIndex));
                break;

            case OkResponse:
                await ResponseOkAsync();
                break;

            default:
                throw new InvalidOperationException($"打牌フェーズの応答として未対応です。実際:{primary.GetType().Name}");
        }
    }

    private async Task DispatchKanAsync(
        ImmutableArray<AdoptedPlayerResponse> adopted,
        PlayerIndex loserIndex
    )
    {
        var primary = adopted[0].Response;
        switch (primary)
        {
            case ChankanRonResponse:
                var winners = adopted
                    .Where(x => x.Response is ChankanRonResponse)
                    .Select(x => x.PlayerIndex)
                    .ToImmutableArray();
                if (winners.Length >= 3)
                {
                    // 三家和了 (SanchaHou): 3 人以上の槍槓ロンは流局扱い
                    await ResponseRyuukyokuAsync(RyuukyokuType.SanchaHou, []);
                }
                else
                {
                    await ResponseWinAsync(winners, loserIndex, WinType.Chankan);
                }
                break;

            case OkResponse:
                await ResponseOkAsync();
                break;

            default:
                throw new InvalidOperationException($"槓フェーズの応答として未対応です。実際:{primary.GetType().Name}");
        }
    }

    /// <summary>
    /// KanTsumo 分岐: RinshanTsumoResponse は即時和了、アクション応答は 2 段階ディスパッチのため pending として戻す
    /// </summary>
    private async Task<PlayerResponse?> DispatchKanTsumoAsync(AdoptedPlayerResponse adopted)
    {
        if (adopted.Response is RinshanTsumoResponse)
        {
            await ResponseWinAsync([adopted.PlayerIndex], adopted.PlayerIndex, WinType.Rinshan);
            return null;
        }

        if (adopted.Response is not (KanTsumoDahaiResponse or KanTsumoAnkanResponse or KanTsumoKakanResponse))
        {
            throw new InvalidOperationException($"嶺上ツモフェーズの応答として未対応です。実際:{adopted.Response.GetType().Name}");
        }

        // 2 段階ディスパッチ: ResponseOk で RoundStateAfterKanTsumo に遷移 → 呼び出し側が pending 応答を消費する
        await ResponseOkAsync();
        return adopted.Response;
    }

    /// <summary>
    /// 問い合わせ対象が 1 人のフェーズ (Tsumo/KanTsumo/AfterKanTsumo) で対象プレイヤーの応答を取り出す
    /// </summary>
    private static AdoptedPlayerResponse FindInquiredResponse(RoundInquirySpec spec, ImmutableArray<AdoptedPlayerResponse> adopted)
    {
        if (spec.InquiredPlayerIndices.Length != 1)
        {
            throw new InvalidOperationException(
                $"フェーズ {spec.Phase} では問い合わせ対象が 1 人であることを期待しますが {spec.InquiredPlayerIndices.Length} 人でした。"
            );
        }
        var targetIndex = spec.InquiredPlayerIndices[0];
        return adopted.FirstOrDefault(x => x.PlayerIndex == targetIndex)
            ?? throw new InvalidOperationException($"問い合わせ対象 {targetIndex.Value} からの応答が見つかりません。");
    }

    /// <summary>
    /// 問い合わせ対象が複数いるフェーズ (Dahai/Kan) で対象プレイヤーの応答のみを抽出する
    /// </summary>
    private static ImmutableArray<AdoptedPlayerResponse> FilterInquiredResponses(RoundInquirySpec spec, ImmutableArray<AdoptedPlayerResponse> adopted)
    {
        var inquiredSet = spec.InquiredPlayerIndices.ToHashSet();
        return [.. adopted.Where(x => inquiredSet.Contains(x.PlayerIndex))];
    }

    private Tile GetDiscardedTile(PlayerIndex discarderIndex)
    {
        return Round.RiverArray[discarderIndex].Last();
    }
}
