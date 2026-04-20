using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// プレイヤー応答の 2 段構え検証を行う
/// - <see cref="IsResponseInCandidates"/>: 1 段目 (候補リスト membership)
/// - <see cref="ValidateSemantic"/>: 2 段目 (手牌整合・フリテン違反・立直条件等の意味的検証)
/// 1 段目失敗時は `RoundStateContext` の通知・応答集約ループで `defaultFactory.CreateDefault` によるフォールバックに差し替えられる。
/// 2 段目失敗時はクライアント契約違反として `InvalidOperationException` が throw され進行を停止する
/// </summary>
internal static class ResponseValidator
{
    private const int RIICHI_POINT_MIN = 1000;
    private const int RIICHI_WALL_MIN = 4;
    private const int KYUUSHU_KIND_MIN = 9;

    /// <summary>
    /// 指定のプレイヤー応答が <paramref name="candidates"/> に含まれる合法応答かを返します。
    /// 牌単位の一致は以下の規約で判定します:
    /// - 打牌 / 嶺上ツモ打牌: DahaiCandidate.DahaiOptionList 内の Tile と完全一致 (Tile.Id 一致)
    /// - 立直宣言 (IsRiichi=true): 対応 DahaiOption.RiichiAvailable=true が必要
    /// - 暗槓 / 嶺上暗槓: 候補の先頭牌と Tile.Kind 一致 (暗槓は同種4枚全てを使うため赤ドラ有無で選択肢は生じない)
    /// - 加槓 / 嶺上加槓: 候補の Tile と完全一致 (Tile.Id 一致)。赤ドラ採用/非採用は別候補として列挙される
    /// - チー / ポン / 大明槓: HandTiles が SequenceEqual
    /// - その他 (Ok/Ron/Tsumo/Chankan/Rinshan/Kyuushu): 対応候補が候補リストに存在するだけで可
    /// </summary>
    public static bool IsResponseInCandidates(PlayerResponse response, CandidateList candidates)
    {
        return response switch
        {
            OkResponse => candidates.HasCandidate<OkCandidate>(),
            RonResponse => candidates.HasCandidate<RonCandidate>(),
            TsumoAgariResponse => candidates.HasCandidate<TsumoAgariCandidate>(),
            KyuushuKyuuhaiResponse => candidates.HasCandidate<KyuushuKyuuhaiCandidate>(),
            ChankanRonResponse => candidates.HasCandidate<ChankanRonCandidate>(),
            RinshanTsumoResponse => candidates.HasCandidate<RinshanTsumoAgariCandidate>(),
            DahaiResponse d => MatchesDahai(candidates, d.Tile, d.IsRiichi),
            KanTsumoDahaiResponse d => MatchesDahai(candidates, d.Tile, d.IsRiichi),
            AnkanResponse a => candidates.GetCandidates<AnkanCandidate>().Any(x => x.Tiles.Length > 0 && x.Tiles[0].Kind == a.Tile.Kind),
            KanTsumoAnkanResponse a => candidates.GetCandidates<AnkanCandidate>().Any(x => x.Tiles.Length > 0 && x.Tiles[0].Kind == a.Tile.Kind),
            KakanResponse k => candidates.GetCandidates<KakanCandidate>().Any(x => x.Tile.Equals(k.Tile)),
            KanTsumoKakanResponse k => candidates.GetCandidates<KakanCandidate>().Any(x => x.Tile.Equals(k.Tile)),
            ChiResponse c => candidates.GetCandidates<ChiCandidate>().Any(x => x.HandTiles.SequenceEqual(c.HandTiles)),
            PonResponse p => candidates.GetCandidates<PonCandidate>().Any(x => x.HandTiles.SequenceEqual(p.HandTiles)),
            DaiminkanResponse d => candidates.GetCandidates<DaiminkanCandidate>().Any(x => x.HandTiles.SequenceEqual(d.HandTiles)),
            _ => false,
        };
    }

    /// <summary>
    /// 応答が候補を通過した後に、Round / Hand / Status との意味的整合を検証する。
    /// 1 段目で候補 membership は担保されるが、クライアントが偽造応答を送ってきたケースに備えて
    /// 「応答が Round 状態と矛盾しないか」を再検証する。失敗時は呼び出し側 (`CollectSingleAsync`) が
    /// `InvalidOperationException` を throw して進行を停止する (3 段目 副作用防止は throw で Round 更新前に停止することで達成)
    /// </summary>
    public static SemanticValidationResult ValidateSemantic(
        PlayerResponse response,
        Round round,
        PlayerIndex playerIndex,
        RoundInquiryPhase phase
    )
    {
        return response switch
        {
            OkResponse => SemanticValidationResult.Ok,
            DahaiResponse d => ValidateDahai(round, playerIndex, d.Tile, d.IsRiichi),
            KanTsumoDahaiResponse d => ValidateDahai(round, playerIndex, d.Tile, d.IsRiichi),
            AnkanResponse a => ValidateAnkan(round, playerIndex, a.Tile),
            KanTsumoAnkanResponse a => ValidateAnkan(round, playerIndex, a.Tile),
            KakanResponse k => ValidateKakan(round, playerIndex, k.Tile),
            KanTsumoKakanResponse k => ValidateKakan(round, playerIndex, k.Tile),
            ChiResponse c => ValidateChi(round, playerIndex, c.HandTiles),
            PonResponse p => ValidatePon(round, playerIndex, p.HandTiles),
            DaiminkanResponse d => ValidateDaiminkan(round, playerIndex, d.HandTiles),
            RonResponse => ValidateRon(round, playerIndex),
            ChankanRonResponse => ValidateRon(round, playerIndex),
            TsumoAgariResponse => ValidateTsumoAgari(round, playerIndex),
            RinshanTsumoResponse => ValidateTsumoAgari(round, playerIndex),
            KyuushuKyuuhaiResponse => ValidateKyuushuKyuuhai(round, playerIndex),
            _ => SemanticValidationResult.Invalid($"未対応の応答型です: {response.GetType().Name}"),
        };
    }

    private static bool MatchesDahai(CandidateList candidates, Tile tile, bool isRiichi)
    {
        return candidates.GetCandidates<DahaiCandidate>()
            .SelectMany(x => x.DahaiOptionList)
            .Any(x => x.Tile.Equals(tile) && (!isRiichi || x.RiichiAvailable));
    }

    private static SemanticValidationResult ValidateDahai(
        Round round,
        PlayerIndex playerIndex,
        Tile tile,
        bool isRiichi
    )
    {
        var hand = round.HandArray[playerIndex];
        if (!hand.Contains(tile))
        {
            return SemanticValidationResult.Invalid($"打牌する牌 {tile} が手牌に存在しません。");
        }

        if (isRiichi)
        {
            var status = round.PlayerRoundStatusArray[playerIndex];
            if (!status.IsMenzen)
            {
                return SemanticValidationResult.Invalid("門前ではないため立直宣言できません。");
            }

            if (status.IsRiichi)
            {
                return SemanticValidationResult.Invalid("既に立直宣言済みです。");
            }

            if (round.PointArray[playerIndex].Value < RIICHI_POINT_MIN)
            {
                return SemanticValidationResult.Invalid($"持ち点が {RIICHI_POINT_MIN} 点未満のため立直宣言できません。");
            }

            if (round.Wall.LiveRemaining < RIICHI_WALL_MIN)
            {
                return SemanticValidationResult.Invalid($"ツモ山残が {RIICHI_WALL_MIN} 枚未満のため立直宣言できません。");
            }

            var remaining = new Hands.Hand(RemoveFirst(hand, tile));
            if (!TenpaiHelper.IsTenpai(remaining))
            {
                return SemanticValidationResult.Invalid("打牌後テンパイしていないため立直宣言できません。");
            }
        }

        return SemanticValidationResult.Ok;
    }

    private static SemanticValidationResult ValidateAnkan(Round round, PlayerIndex playerIndex, Tile tile)
    {
        var hand = round.HandArray[playerIndex];
        if (hand.Count(x => x.Kind == tile.Kind) < 4)
        {
            return SemanticValidationResult.Invalid($"暗槓対象牌種 {tile.Kind} の 4 枚が手牌にありません。");
        }
        return SemanticValidationResult.Ok;
    }

    private static SemanticValidationResult ValidateKakan(Round round, PlayerIndex playerIndex, Tile tile)
    {
        var hand = round.HandArray[playerIndex];
        if (!hand.Contains(tile))
        {
            return SemanticValidationResult.Invalid($"加槓対象牌 {tile} が手牌に存在しません。");
        }
        var callList = round.CallListArray[playerIndex];
        if (!callList.Any(x => x.Type == CallType.Pon && x.Tiles.Count > 0 && x.Tiles[0].Kind == tile.Kind))
        {
            return SemanticValidationResult.Invalid($"加槓対象牌種 {tile.Kind} のポン副露がありません。");
        }
        return SemanticValidationResult.Ok;
    }

    private static SemanticValidationResult ValidateChi(
        Round round,
        PlayerIndex playerIndex,
        ImmutableArray<Tile> handTiles
    )
    {
        if (handTiles.Length != 2)
        {
            return SemanticValidationResult.Invalid($"チーの手牌使用枚数は 2 枚ですが {handTiles.Length} 枚でした。");
        }

        var hand = round.HandArray[playerIndex];
        if (!ContainsAll(hand, handTiles))
        {
            return SemanticValidationResult.Invalid("チーで使用する手牌が実際の手牌に存在しません。");
        }

        var discardedTile = round.RiverArray[round.Turn].LastOrDefault();
        if (discardedTile is null)
        {
            return SemanticValidationResult.Invalid("直前の打牌が見つかりません。");
        }

        var kinds = new[] { handTiles[0].Kind, handTiles[1].Kind, discardedTile.Kind }.OrderBy(x => x).ToArray();
        if (!kinds[0].IsNumber || !kinds[0].IsSameSuit(kinds[2]))
        {
            return SemanticValidationResult.Invalid("チーは同一数牌スートの連続 3 牌のみ可能です。");
        }

        if (!kinds[0].TryGetAtDistance(1, out var next1) || next1 != kinds[1] ||
            !kinds[0].TryGetAtDistance(2, out var next2) || next2 != kinds[2])
        {
            return SemanticValidationResult.Invalid($"チーの順子が連続 3 牌ではありません ({kinds[0]},{kinds[1]},{kinds[2]})。");
        }

        return SemanticValidationResult.Ok;
    }

    private static SemanticValidationResult ValidatePon(
        Round round,
        PlayerIndex playerIndex,
        ImmutableArray<Tile> handTiles
    )
    {
        if (handTiles.Length != 2)
        {
            return SemanticValidationResult.Invalid($"ポンの手牌使用枚数は 2 枚ですが {handTiles.Length} 枚でした。");
        }

        return ValidateSameKindCall(round, playerIndex, handTiles, callName: "ポン");
    }

    private static SemanticValidationResult ValidateDaiminkan(
        Round round,
        PlayerIndex playerIndex,
        ImmutableArray<Tile> handTiles
    )
    {
        if (handTiles.Length != 3)
        {
            return SemanticValidationResult.Invalid($"大明槓の手牌使用枚数は 3 枚ですが {handTiles.Length} 枚でした。");
        }

        return ValidateSameKindCall(round, playerIndex, handTiles, callName: "大明槓");
    }

    private static SemanticValidationResult ValidateSameKindCall(
        Round round,
        PlayerIndex playerIndex,
        ImmutableArray<Tile> handTiles,
        string callName
    )
    {
        var hand = round.HandArray[playerIndex];
        if (!ContainsAll(hand, handTiles))
        {
            return SemanticValidationResult.Invalid($"{callName}で使用する手牌が実際の手牌に存在しません。");
        }

        var discardedTile = round.RiverArray[round.Turn].LastOrDefault();
        if (discardedTile is null)
        {
            return SemanticValidationResult.Invalid("直前の打牌が見つかりません。");
        }

        if (handTiles.Any(x => x.Kind != discardedTile.Kind))
        {
            return SemanticValidationResult.Invalid($"{callName}は全牌が打牌と同種である必要があります。");
        }

        return SemanticValidationResult.Ok;
    }

    private static SemanticValidationResult ValidateRon(Round round, PlayerIndex playerIndex)
    {
        var status = round.PlayerRoundStatusArray[playerIndex];
        if (status.IsFuriten)
        {
            return SemanticValidationResult.Invalid("フリテン状態のためロン宣言できません。");
        }

        if (status.IsTemporaryFuriten)
        {
            return SemanticValidationResult.Invalid("同巡フリテン状態のためロン宣言できません。");
        }

        return SemanticValidationResult.Ok;
    }

    private static SemanticValidationResult ValidateTsumoAgari(Round round, PlayerIndex playerIndex)
    {
        // 副露含めて 14 枚相当 (暗槓/大明槓で手牌は 10-13 枚になるため副露込みで判定)
        var handCount = round.HandArray[playerIndex].Count();
        var callTileCount = round.CallListArray[playerIndex].Sum(x => x.Tiles.Count);
        var totalCount = handCount + callTileCount;
        if (totalCount < 14)
        {
            return SemanticValidationResult.Invalid($"ツモ和了は手牌 + 副露で 14 枚相当が必要ですが {totalCount} 枚でした。");
        }

        return SemanticValidationResult.Ok;
    }

    private static SemanticValidationResult ValidateKyuushuKyuuhai(Round round, PlayerIndex playerIndex)
    {
        var status = round.PlayerRoundStatusArray[playerIndex];
        if (!status.IsFirstTurnBeforeDiscard)
        {
            return SemanticValidationResult.Invalid("九種九牌は第一打前のみ宣言できます。");
        }

        var hand = round.HandArray[playerIndex];
        var yaochuuKinds = hand.Select(x => x.Kind).Where(x => x.IsYaochu).Distinct().Count();
        if (yaochuuKinds < KYUUSHU_KIND_MIN)
        {
            return SemanticValidationResult.Invalid($"九種九牌は幺九牌 {KYUUSHU_KIND_MIN} 種以上が必要ですが {yaochuuKinds} 種でした。");
        }

        return SemanticValidationResult.Ok;
    }

    private static bool ContainsAll(Hands.Hand hand, ImmutableArray<Tile> tiles)
    {
        var remaining = hand.ToList();
        foreach (var tile in tiles)
        {
            var index = remaining.FindIndex(x => x.Equals(tile));
            if (index < 0)
            {
                return false;
            }

            remaining.RemoveAt(index);
        }
        return true;
    }

    private static IEnumerable<Tile> RemoveFirst(Hands.Hand hand, Tile target)
    {
        var removed = false;
        foreach (var tile in hand)
        {
            if (!removed && tile.Equals(target))
            {
                removed = true;
                continue;
            }

            yield return tile;
        }
    }
}

/// <summary>
/// 意味的検証の結果
/// </summary>
internal record SemanticValidationResult(bool IsValid, string? Reason)
{
    public static SemanticValidationResult Ok { get; } = new(true, null);

    public static SemanticValidationResult Invalid(string reason)
    {
        return new(false, reason);
    }
}
