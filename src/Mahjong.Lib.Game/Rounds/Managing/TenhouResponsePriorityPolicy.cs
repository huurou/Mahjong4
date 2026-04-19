using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 天鳳ルール準拠の優先順位解決実装
/// Dahai: ロン > ポン/大明槓 > チー > OK (スルー)
/// Kan:   槍槓ロン > OK
/// ダブロン対応 (ロン応答が複数あれば放銃者基準の巡目順で全員採用)
/// 同順位衝突時は放銃者基準の巡目順で上家優先 (放銃者の下家 → 対面 → 上家)
/// </summary>
public sealed class TenhouResponsePriorityPolicy : IResponsePriorityPolicy
{
    public ImmutableArray<AdoptedPlayerResponse> Resolve(
        RoundInquirySpec spec,
        ImmutableArray<AdoptedPlayerResponse> responses
    )
    {
        ArgumentNullException.ThrowIfNull(spec);

        return spec.Phase switch
        {
            RoundInquiryPhase.Dahai => ResolveDahai(responses, RequireLoserIndex(spec)),
            RoundInquiryPhase.Kan => ResolveKan(responses, RequireLoserIndex(spec)),
            // Haipai: 全員 OK / Tsumo / KanTsumo / AfterKanTsumo: 単一プレイヤー応答なので優先順位解決不要
            _ => responses,
        };
    }

    private static ImmutableArray<AdoptedPlayerResponse> ResolveDahai(
        ImmutableArray<AdoptedPlayerResponse> responses,
        PlayerIndex loserIndex
    )
    {
        // ロン (ダブロン対応) - 全員採用、放銃者基準の巡目順 (下家 → 対面 → 上家)
        var ronResponders = responses
            .Where(x => x.Response is RonResponse)
            .OrderBy(x => TurnOrderFromLoser(x.PlayerIndex, loserIndex))
            .ToImmutableArray();
        if (ronResponders.Length > 0)
        {
            return ronResponders;
        }

        // ポン/大明槓 (優先) - 単一採用、放銃者基準で最も近い (= 下家) を優先
        var ponDaiminkan = responses
            .Where(x => x.Response is PonResponse or DaiminkanResponse)
            .OrderBy(x => TurnOrderFromLoser(x.PlayerIndex, loserIndex))
            .FirstOrDefault();
        if (ponDaiminkan is not null)
        {
            return [ponDaiminkan];
        }

        // チー - 単一採用 (通常は下家のみ提示されるが、念のため放銃者基準順でソート)
        var chi = responses
            .Where(x => x.Response is ChiResponse)
            .OrderBy(x => TurnOrderFromLoser(x.PlayerIndex, loserIndex))
            .FirstOrDefault();
        if (chi is not null)
        {
            return [chi];
        }

        // 全員スルー (OK)
        return responses;
    }

    private static ImmutableArray<AdoptedPlayerResponse> ResolveKan(
        ImmutableArray<AdoptedPlayerResponse> responses,
        PlayerIndex loserIndex
    )
    {
        // 槍槓ロン (ダブロン対応可) - 全員採用、放銃者基準の巡目順
        var chankanResponders = responses
            .Where(x => x.Response is ChankanRonResponse)
            .OrderBy(x => TurnOrderFromLoser(x.PlayerIndex, loserIndex))
            .ToImmutableArray();
        if (chankanResponders.Length > 0)
        {
            return chankanResponders;
        }

        // 全員スルー
        return responses; 
    }

    /// <summary>
    /// 放銃者基準の巡目距離 (下家=1 / 対面=2 / 上家=3)。自身は 0
    /// </summary>
    private static int TurnOrderFromLoser(PlayerIndex responder, PlayerIndex loser)
    {
        return (responder.Value - loser.Value + PlayerIndex.PLAYER_COUNT) % PlayerIndex.PLAYER_COUNT;
    }

    private static PlayerIndex RequireLoserIndex(RoundInquirySpec spec)
    {
        return spec.LoserIndex
            ?? throw new ArgumentException(
                $"フェーズ {spec.Phase} では RoundInquirySpec.LoserIndex が必須です。",
                nameof(spec)
            );
    }
}
