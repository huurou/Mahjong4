using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 既定の応答ディスパッチ実装。
/// フェーズ毎に採用応答を <see cref="RoundStateContext"/> の ResponseXxxAsync イベント発火に変換する
/// </summary>
public sealed class ResponseDispatcher : IResponseDispatcher
{
    public async Task<PlayerResponse?> DispatchAsync(
        RoundStateContext context,
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
                await context.ResponseOkAsync();
                return null;

            case RoundInquiryPhase.Tsumo:
                await DispatchTsumoAsync(context, FindInquiredResponse(spec, adopted));
                return null;

            case RoundInquiryPhase.Dahai:
                await DispatchDahaiAsync(context, FilterInquiredResponses(spec, adopted), spec.LoserIndex);
                return null;

            case RoundInquiryPhase.Kan:
                await DispatchKanAsync(context, FilterInquiredResponses(spec, adopted), spec.LoserIndex);
                return null;

            case RoundInquiryPhase.KanTsumo:
                return await DispatchKanTsumoAsync(context, FindInquiredResponse(spec, adopted));

            case RoundInquiryPhase.AfterKanTsumo:
                await DispatchAfterKanTsumoAsync(context, FindInquiredResponse(spec, adopted).Response);
                return null;

            default:
                throw new InvalidOperationException($"未対応のフェーズです。実際:{spec.Phase}");
        }
    }

    public async Task DispatchAfterKanTsumoAsync(RoundStateContext context, PlayerResponse pendingResponse)
    {
        switch (pendingResponse)
        {
            case KanTsumoDahaiResponse d:
                await context.ResponseDahaiAsync(d.Tile, d.IsRiichi);
                break;

            case KanTsumoAnkanResponse a:
                await context.ResponseKanAsync(CallType.Ankan, a.Tile);
                break;

            case KanTsumoKakanResponse k:
                await context.ResponseKanAsync(CallType.Kakan, k.Tile);
                break;

            default:
                throw new InvalidOperationException($"嶺上ツモ後フェーズの応答として未対応です。実際:{pendingResponse.GetType().Name}");
        }
    }

    private static async Task DispatchTsumoAsync(RoundStateContext context, AdoptedPlayerResponse adopted)
    {
        switch (adopted.Response)
        {
            case DahaiResponse d:
                await context.ResponseDahaiAsync(d.Tile, d.IsRiichi);
                break;

            case AnkanResponse a:
                await context.ResponseKanAsync(CallType.Ankan, a.Tile);
                break;

            case KakanResponse k:
                await context.ResponseKanAsync(CallType.Kakan, k.Tile);
                break;

            case TsumoAgariResponse:
                await context.ResponseWinAsync([adopted.PlayerIndex], adopted.PlayerIndex, WinType.Tsumo);
                break;

            case KyuushuKyuuhaiResponse:
                await context.ResponseRyuukyokuAsync(RyuukyokuType.KyuushuKyuuhai, []);
                break;

            default:
                throw new InvalidOperationException($"ツモフェーズの応答として未対応です。実際:{adopted.Response.GetType().Name}");
        }
    }

    private static async Task DispatchDahaiAsync(
        RoundStateContext context,
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
                await context.ResponseWinAsync(winners, loserIndex, WinType.Ron);
                break;

            case ChiResponse chi:
                await context.ResponseCallAsync(adopted[0].PlayerIndex, CallType.Chi, [.. chi.HandTiles], GetDiscardedTile(context, loserIndex));
                break;

            case PonResponse pon:
                await context.ResponseCallAsync(adopted[0].PlayerIndex, CallType.Pon, [.. pon.HandTiles], GetDiscardedTile(context, loserIndex));
                break;

            case DaiminkanResponse daiminkan:
                await context.ResponseCallAsync(adopted[0].PlayerIndex, CallType.Daiminkan, [.. daiminkan.HandTiles], GetDiscardedTile(context, loserIndex));
                break;

            case OkResponse:
                await context.ResponseOkAsync();
                break;

            default:
                throw new InvalidOperationException($"打牌フェーズの応答として未対応です。実際:{primary.GetType().Name}");
        }
    }

    private static async Task DispatchKanAsync(
        RoundStateContext context,
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
                await context.ResponseWinAsync(winners, loserIndex, WinType.Chankan);
                break;

            case OkResponse:
                await context.ResponseOkAsync();
                break;

            default:
                throw new InvalidOperationException($"槓フェーズの応答として未対応です。実際:{primary.GetType().Name}");
        }
    }

    /// <summary>
    /// KanTsumo 分岐: RinshanTsumoResponse は即時和了、アクション応答は 2 段階ディスパッチのため pending として戻す
    /// </summary>
    private static async Task<PlayerResponse?> DispatchKanTsumoAsync(RoundStateContext context, AdoptedPlayerResponse adopted)
    {
        if (adopted.Response is RinshanTsumoResponse)
        {
            await context.ResponseWinAsync([adopted.PlayerIndex], adopted.PlayerIndex, WinType.Rinshan);
            return null;
        }

        if (adopted.Response is not (KanTsumoDahaiResponse or KanTsumoAnkanResponse or KanTsumoKakanResponse))
        {
            throw new InvalidOperationException($"嶺上ツモフェーズの応答として未対応です。実際:{adopted.Response.GetType().Name}");
        }

        // 2 段階ディスパッチ: ResponseOk で RoundStateAfterKanTsumo に遷移 → 呼び出し側が pending 応答を消費する
        await context.ResponseOkAsync();
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

    private static Tile GetDiscardedTile(RoundStateContext context, PlayerIndex discarderIndex)
    {
        return context.Round.RiverArray[discarderIndex].Last();
    }
}
