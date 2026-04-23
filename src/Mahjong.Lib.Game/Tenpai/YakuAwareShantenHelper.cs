using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Scoring.Shantens;
using Mahjong.Lib.Scoring.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tenpai;

/// <summary>
/// 有効牌に対する副露マーク。評価値倍率の計算に用いる。
/// </summary>
internal enum CallMark
{
    /// <summary>鳴きでは進めない (ツモのみ想定)</summary>
    None,
    /// <summary>ポンで役ありシャンテンが進む (評価値 ×4)</summary>
    Pon,
    /// <summary>チーで役ありシャンテンが進む (評価値 ×2)</summary>
    Chi,
}

/// <summary>
/// 役に対するシャンテン数を計算するヘルパー。
/// 以下 7 経路の min を返す: 門前 / 翻牌 / 断么九 / 対々和 / 一色手 (萬/筒/索)。
/// いずれの経路も成立しえない場合は <see cref="INFINITE"/>。
/// </summary>
internal static class YakuAwareShantenHelper
{
    /// <summary>
    /// 対象役が成立不能なときに各経路関数が返す番兵値 (Infinity 相当)
    /// </summary>
    public const int INFINITE = 99;

    /// <summary>
    /// 役ありシャンテン数を計算する。
    /// </summary>
    /// <param name="hand">晒していない手牌 (副露相当分は含めない)</param>
    /// <param name="calls">現在の副露</param>
    /// <param name="rules">対局ルール (KuitanAllowed を参照)</param>
    /// <param name="roundWindIndex">場風 (0=東/1=南/2=西/3=北)</param>
    /// <param name="seatWindIndex">自風 (0=東/1=南/2=西/3=北)</param>
    public static int Calc(Hands.Hand hand, CallList calls, GameRules rules, int roundWindIndex, int seatWindIndex)
    {
        int[] shantens =
        [
            CalcMenzen(hand, calls),
            CalcYakuhai(hand, calls, roundWindIndex, seatWindIndex),
            CalcTanyao(hand, calls, rules),
            CalcToitoi(hand, calls),
            CalcIsshoku(hand, calls, suit: 0),
            CalcIsshoku(hand, calls, suit: 1),
            CalcIsshoku(hand, calls, suit: 2),
        ];
        return shantens.Min();
    }

    /// <summary>
    /// 門前経路。非暗槓の Call があれば INFINITE。
    /// 暗槓のみなら通常形 + 七対子 + 国士無双の min (暗槓ゼロのときのみ)。
    /// 暗槓 ≥ 1 なら七対子・国士は成立しないので通常形のみ。
    /// </summary>
    internal static int CalcMenzen(Hands.Hand hand, CallList calls)
    {
        var ankanCount = 0;
        foreach (var call in calls)
        {
            if (call.Type == CallType.Ankan)
            {
                ankanCount++;
            }
            else
            {
                return INFINITE;
            }
        }
        var allowSpecialForms = ankanCount == 0;
        return ShantenCalculator.Calc(
            hand.ToScoringTileKindList(),
            knownCallMeldCount: ankanCount,
            useChiitoitsu: allowSpecialForms,
            useKokushi: allowSpecialForms);
    }

    /// <summary>
    /// 翻牌経路。対象 kind (白/發/中 + 場風 + 自風) のいずれかで刻子が確定していれば通常シャンテン、
    /// 対子だけがあれば「ポン仮想 + 1 シャンテン補正」で最善 kind を採用、いずれもなければ INFINITE。
    /// </summary>
    internal static int CalcYakuhai(Hands.Hand hand, CallList calls, int roundWindIndex, int seatWindIndex)
    {
        var targets = ImmutableHashSet.CreateBuilder<TileKind>();
        targets.Add(TileKind.Haku);
        targets.Add(TileKind.Hatsu);
        targets.Add(TileKind.Chun);
        targets.Add(TileKind.Winds[roundWindIndex]);
        targets.Add(TileKind.Winds[seatWindIndex]);

        var tileKindList = hand.ToScoringTileKindList();
        var callMeldCount = calls.Count;

        // 先に刻子確定の有無だけ判定 (1 つでも確定していれば通常シャンテン即返し)
        // 翻牌は 4 面子 1 雀頭の通常形でのみ成立する役なので、七対子・国士経路は除外
        foreach (var kind in targets)
        {
            if (tileKindList.CountOf(kind) >= 3 || HasCallKoutsuOf(calls, kind))
            {
                return ShantenCalculator.Calc(tileKindList, knownCallMeldCount: callMeldCount, useChiitoitsu: false, useKokushi: false);
            }
        }

        // 対子候補を全て試して min をとる
        var best = INFINITE;
        foreach (var kind in targets)
        {
            if (tileKindList.CountOf(kind) == 2)
            {
                var removed = tileKindList.Remove(kind, 2);
                var shanten = ShantenCalculator.Calc(removed, knownCallMeldCount: callMeldCount + 1, useChiitoitsu: false, useKokushi: false);
                best = Math.Min(best, shanten + 1);
            }
        }
        return best;
    }

    private static bool HasCallKoutsuOf(CallList calls, TileKind kind)
    {
        foreach (var call in calls)
        {
            if (call.Type == CallType.Chi) { continue; }

            // ポン/大明槓/加槓/暗槓 はすべて 3 枚以上の同一牌種で構成される
            if (call.Tiles[0].Kind == kind)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 断么九経路。クイタンなしで非暗槓 Call あり / Call に么九含む / のいずれかで INFINITE。
    /// それ以外は手牌から么九を除去してシャンテン計算。
    /// 除去した么九牌は断么九では使えず最終的に必ず打牌する必要があるため、
    /// 除去枚数を下限ペナルティとしてシャンテンに加算する (実際のシャンテンを過小評価しない安全側補正)。
    /// </summary>
    internal static int CalcTanyao(Hands.Hand hand, CallList calls, GameRules rules)
    {
        foreach (var call in calls)
        {
            if (!rules.KuitanAllowed && call.Type != CallType.Ankan)
            {
                return INFINITE;
            }
            foreach (var tile in call.Tiles)
            {
                if (tile.Kind.IsYaochu)
                {
                    return INFINITE;
                }
            }
        }

        var handCount = hand.Count();
        var filtered = FilterTileKinds(hand, k => !k.IsYaochu);
        var removedCount = handCount - filtered.Count;
        // 断么九は通常形の中張牌縛りで成立する役なので、七対子・国士経路は除外
        var shanten = ShantenCalculator.Calc(filtered, knownCallMeldCount: calls.Count, useChiitoitsu: false, useKokushi: false);
        return shanten + removedCount;
    }

    /// <summary>
    /// 対々和経路 (直接公式)。Call に順子あり → INFINITE、それ以外は
    /// 8 - 2 * (刻子数) - (対子数)、ブロック数超過分を補正。
    /// </summary>
    internal static int CalcToitoi(Hands.Hand hand, CallList calls)
    {
        var callKoutsu = 0;
        foreach (var call in calls)
        {
            if (call.Type == CallType.Chi)
            {
                return INFINITE;
            }

            callKoutsu++;
        }

        var tileKindList = hand.ToScoringTileKindList();
        var handKoutsu = 0;
        var handToitsu = 0;
        foreach (var kind in TileKind.All)
        {
            var count = tileKindList.CountOf(kind);
            if (count >= 3)
            {
                handKoutsu++;
            }
            else if (count == 2)
            {
                handToitsu++;
            }
        }

        var koutsuCount = callKoutsu + handKoutsu;
        var toitsuCount = handToitsu;
        if (koutsuCount + toitsuCount > 5)
        {
            toitsuCount = 5 - koutsuCount;
        }
        return 8 - 2 * koutsuCount - toitsuCount;
    }

    /// <summary>
    /// 一色手経路 (萬子/筒子/索子)。Call に対象スートでも字牌でもない牌が含まれれば INFINITE。
    /// それ以外は手牌から他スート数牌を除去 (字牌は残す) してシャンテン計算。
    /// 除去した他スート牌は一色手では使えず最終的に必ず打牌する必要があるため、
    /// 除去枚数を下限ペナルティとしてシャンテンに加算する (実際のシャンテンを過小評価しない安全側補正)。
    /// </summary>
    /// <param name="suit">0=萬子 / 1=筒子 / 2=索子</param>
    internal static int CalcIsshoku(Hands.Hand hand, CallList calls, int suit)
    {
        foreach (var call in calls)
        {
            foreach (var tile in call.Tiles)
            {
                if (tile.Kind.IsHonor) { continue; }

                if (tile.Kind.Value / 9 != suit)
                {
                    return INFINITE;
                }
            }
        }

        var handCount = hand.Count();
        var filtered = FilterTileKinds(hand, k => k.IsHonor || k.Value / 9 == suit);
        var removedCount = handCount - filtered.Count;
        // 混一色・清一色は通常形の一色縛りで成立する役なので、七対子・国士経路は除外
        var shanten = ShantenCalculator.Calc(filtered, knownCallMeldCount: calls.Count, useChiitoitsu: false, useKokushi: false);
        return shanten + removedCount;
    }

    private static TileKindList FilterTileKinds(Hands.Hand hand, Func<TileKind, bool> predicate)
    {
        var builder = ImmutableList.CreateBuilder<TileKind>();
        foreach (var tile in hand)
        {
            if (predicate(tile.Kind))
            {
                builder.Add(tile.Kind);
            }
        }
        return [.. builder.ToImmutable()];
    }

    /// <summary>
    /// 役ありシャンテンを進める有効牌を副露マーク (<see cref="CallMark"/>) 付きで列挙する。
    /// 各 TileKind について、ツモ時のシャンテン減少を
    /// 確認し、さらに <em>ポンでさらに進む</em>なら <see cref="CallMark.Pon"/> を、
    /// そうでなければ <em>チーでさらに進む</em>なら <see cref="CallMark.Chi"/> を付与する。
    /// </summary>
    /// <param name="hand">現在の手牌 (概ね 13 枚)</param>
    /// <param name="calls">現在の副露</param>
    /// <param name="rules">対局ルール</param>
    /// <param name="roundWindIndex">場風インデックス (0-3)</param>
    /// <param name="seatWindIndex">自風インデックス (0-3)</param>
    public static ImmutableArray<(TileKind Kind, CallMark Mark)> EnumerateUsefulTileKindsWithCallMark(
        Hands.Hand hand, CallList calls, GameRules rules,
        int roundWindIndex, int seatWindIndex)
    {
        var currentShanten = Calc(hand, calls, rules, roundWindIndex, seatWindIndex);
        if (currentShanten is INFINITE or ShantenConstants.SHANTEN_AGARI)
        {
            return [];
        }

        var tileKindCounts = BuildKindCounts(hand);

        var builder = ImmutableArray.CreateBuilder<(TileKind Kind, CallMark Mark)>();
        foreach (var kind in TileKind.All)
        {
            if (tileKindCounts.GetValueOrDefault(kind) >= 4) { continue; }

            var handPlusKind = hand.AddTile(TileFromKind(kind, tileKindCounts));
            var shantenAfterDraw = Calc(handPlusKind, calls, rules, roundWindIndex, seatWindIndex);
            if (shantenAfterDraw >= currentShanten) { continue; }

            // 有効牌。テンパイ前のみ副露マークを検討 (テンパイなら副露はしない)
            var mark = CallMark.None;
            if (currentShanten > 0)
            {
                if (tileKindCounts.GetValueOrDefault(kind) >= 2 &&
                    TryPonReducesShanten(hand, calls, rules, kind, currentShanten, roundWindIndex, seatWindIndex, tileKindCounts))
                {
                    mark = CallMark.Pon;
                }
                else if (kind.IsNumber &&
                    TryChiReducesShanten(hand, calls, rules, kind, currentShanten, roundWindIndex, seatWindIndex, tileKindCounts))
                {
                    mark = CallMark.Chi;
                }
            }
            builder.Add((kind, mark));
        }
        return builder.ToImmutable();
    }

    /// <summary>
    /// ポン仮想後の役ありシャンテンが現状より進むかを判定する。
    /// </summary>
    private static bool TryPonReducesShanten(
        Hands.Hand hand, CallList calls, GameRules rules, TileKind kind,
        int currentShanten, int roundWindIndex, int seatWindIndex,
        Dictionary<TileKind, int> tileKindCounts)
    {
        var tilesOfKind = hand.Where(t => t.Kind == kind).Take(2).ToList();
        if (tilesOfKind.Count < 2) { return false; }

        var handAfter = hand.RemoveTile(tilesOfKind[0]).RemoveTile(tilesOfKind[1]);
        var virtualCall = BuildVirtualKoutsuCall(CallType.Pon, kind, tileKindCounts);
        var callsAfter = calls.Add(virtualCall);
        var shantenAfter = Calc(handAfter, callsAfter, rules, roundWindIndex, seatWindIndex);
        return shantenAfter < currentShanten;
    }

    /// <summary>
    /// チー仮想後の役ありシャンテンが現状より進むかを判定する。
    /// </summary>
    private static bool TryChiReducesShanten(
        Hands.Hand hand, CallList calls, GameRules rules, TileKind kind,
        int currentShanten, int roundWindIndex, int seatWindIndex,
        Dictionary<TileKind, int> tileKindCounts)
    {
        // kind を含む順子 3 パターン: (k-2,k-1,k) / (k-1,k,k+1) / (k,k+1,k+2)
        foreach (var (off1, off2) in new[] { (-2, -1), (-1, 1), (1, 2) })
        {
            if (!kind.TryGetAtDistance(off1, out var k1)) { continue; }
            if (!kind.TryGetAtDistance(off2, out var k2)) { continue; }
            // 順子構成には kind 以外の 2 牌種が手牌にそれぞれ 1 枚以上必要
            var need1 = hand.FirstOrDefault(t => t.Kind == k1);
            if (need1 is null) { continue; }
            var need2 = hand.FirstOrDefault(t => t.Kind == k2);
            if (need2 is null) { continue; }

            var handAfter = hand.RemoveTile(need1).RemoveTile(need2);
            var virtualCall = BuildVirtualShuntsuCall(kind, k1, k2, tileKindCounts);
            var callsAfter = calls.Add(virtualCall);
            var shantenAfter = Calc(handAfter, callsAfter, rules, roundWindIndex, seatWindIndex);
            if (shantenAfter < currentShanten) { return true; }
        }
        return false;
    }

    private static Dictionary<TileKind, int> BuildKindCounts(Hands.Hand hand)
    {
        var dict = new Dictionary<TileKind, int>();
        foreach (var tile in hand)
        {
            dict[tile.Kind] = dict.GetValueOrDefault(tile.Kind) + 1;
        }
        return dict;
    }

    /// <summary>
    /// 指定 kind の有効牌を仮想的に Tile 化する (手牌・副露にまだ使われていない id を返す)。
    /// 内部シャンテン計算では <see cref="Tile.Kind"/> のみ参照されるので id は一意であれば十分。
    /// </summary>
    private static Tile TileFromKind(TileKind kind, Dictionary<TileKind, int> tileKindCounts)
    {
        var used = tileKindCounts.GetValueOrDefault(kind);
        return new Tile(kind.Value * 4 + used);
    }

    /// <summary>
    /// 役ありシャンテン仮想計算専用の Call 生成。<see cref="ShantenCalculator"/> は <see cref="Tile.Kind"/>
    /// しか参照しないため Tile id の実体整合 (手牌・他の副露との id 衝突回避等) は保証しない。
    /// Round 実体の副露を構築する用途には絶対に使わないこと。
    /// </summary>
    private static Call BuildVirtualKoutsuCall(CallType type, TileKind kind, Dictionary<TileKind, int> tileKindCounts)
    {
        var used = tileKindCounts.GetValueOrDefault(kind);
        // ポン/大明槓 3-4 枚、暗槓 4 枚。ここではポン想定で 3 枚使用
        var count = type is CallType.Ankan or CallType.Daiminkan or CallType.Kakan ? 4 : 3;
        var tiles = new List<Tile>(count);
        for (var i = 0; i < count; i++)
        {
            tiles.Add(new Tile(kind.Value * 4 + (used + i) % 4));
        }
        var tileList = tiles.ToImmutableList();
        var calledTile = type == CallType.Ankan ? null : tileList[0];
        return new Call(type, tileList, new PlayerIndex(3), calledTile);
    }

    /// <summary>
    /// 役ありシャンテン仮想計算専用の Chi Call 生成。3 枚とも数牌かつ同スート・連続の Call
    /// バリデーションを満たす最小構成。<see cref="ShantenCalculator"/> は <see cref="Tile.Kind"/>
    /// しか参照しないため Tile id の実体整合 (手牌・他の副露との id 衝突回避等) は保証しない。
    /// Round 実体の副露を構築する用途には絶対に使わないこと。
    /// </summary>
    private static Call BuildVirtualShuntsuCall(TileKind calledKind, TileKind k1, TileKind k2, Dictionary<TileKind, int> tileKindCounts)
    {
        var usedCalled = tileKindCounts.GetValueOrDefault(calledKind);
        var calledTile = new Tile(calledKind.Value * 4 + usedCalled);
        var tile1 = new Tile(k1.Value * 4);
        var tile2 = new Tile(k2.Value * 4);
        var allTiles = new List<Tile> { calledTile, tile1, tile2 };
        return new Call(CallType.Chi, [.. allTiles], new PlayerIndex(3), calledTile);
    }
}
