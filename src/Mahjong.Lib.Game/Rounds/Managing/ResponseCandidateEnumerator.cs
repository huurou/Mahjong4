using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// Round と意思決定フェーズから合法応答候補を列挙する既定実装
/// 本実装では簡略化として立直中は槓候補を一切提示しない (待ち不変の暗槓のみ許可する精緻化は別タスク)
/// </summary>
public sealed class ResponseCandidateEnumerator(
    ITenpaiChecker tenpaiChecker,
    GameRules rules
) : IResponseCandidateEnumerator
{
    private readonly ITenpaiChecker tenpaiChecker_ = tenpaiChecker ?? throw new ArgumentNullException(nameof(tenpaiChecker));
    private readonly GameRules rules_ = rules ?? throw new ArgumentNullException(nameof(rules));

    private const int RIICHI_POINT_MIN = 1000;
    private const int RIICHI_WALL_MIN = 4;
    private const int KYUUSHU_KIND_MIN = 9;

    public CandidateList EnumerateForTsumo(Round round, PlayerIndex turnPlayerIndex)
    {
        ArgumentNullException.ThrowIfNull(round);
        ArgumentNullException.ThrowIfNull(turnPlayerIndex);

        var builder = ImmutableList.CreateBuilder<ResponseCandidate>();
        var hand = round.HandArray[turnPlayerIndex];
        var callList = round.CallListArray[turnPlayerIndex];
        var status = round.PlayerRoundStatusArray[turnPlayerIndex];

        builder.Add(BuildDahaiCandidate(round, turnPlayerIndex, hand, callList, status));

        if (TryBuildTsumoAgariCandidate(hand, callList, status) is { } tsumoAgari)
        {
            builder.Add(tsumoAgari);
        }

        if (!status.IsRiichi)
        {
            builder.AddRange(BuildAnkanCandidates(round, hand));
            builder.AddRange(BuildKakanCandidates(round, hand, callList));
        }

        if (status.IsFirstTurnBeforeDiscard && hand.Select(x => x.Kind).Where(IsYaochuuKind).Distinct().Count() >= KYUUSHU_KIND_MIN)
        {
            builder.Add(new KyuushuKyuuhaiCandidate());
        }

        return new CandidateList(builder.ToImmutable());
    }

    public CandidateList EnumerateForDahai(Round round, PlayerIndex responderIndex, Tile discardedTile)
    {
        ArgumentNullException.ThrowIfNull(round);
        ArgumentNullException.ThrowIfNull(responderIndex);
        ArgumentNullException.ThrowIfNull(discardedTile);

        var builder = ImmutableList.CreateBuilder<ResponseCandidate>();
        var hand = round.HandArray[responderIndex];
        var callList = round.CallListArray[responderIndex];
        var status = round.PlayerRoundStatusArray[responderIndex];

        if (TryBuildRonCandidate(hand, callList, status, discardedTile) is { } ron)
        {
            builder.Add(ron);
        }

        if (!status.IsRiichi)
        {
            builder.AddRange(BuildChiCandidates(round, responderIndex, hand, discardedTile));
            builder.AddRange(BuildPonCandidates(hand, discardedTile));
            builder.AddRange(BuildDaiminkanCandidates(round, hand, discardedTile));
        }

        builder.Add(new OkCandidate());
        return new CandidateList(builder.ToImmutable());
    }

    public CandidateList EnumerateForKan(
        Round round,
        PlayerIndex responderIndex,
        ImmutableArray<Tile> kanTiles,
        CallType kanType
    )
    {
        ArgumentNullException.ThrowIfNull(round);
        ArgumentNullException.ThrowIfNull(responderIndex);

        if (kanTiles.IsDefaultOrEmpty)
        {
            throw new ArgumentException("kanTiles が空です。", nameof(kanTiles));
        }

        var builder = ImmutableList.CreateBuilder<ResponseCandidate>();
        if (kanType is CallType.Kakan)
        {
            var hand = round.HandArray[responderIndex];
            var callList = round.CallListArray[responderIndex];
            var status = round.PlayerRoundStatusArray[responderIndex];
            if (TryBuildRonCandidate(hand, callList, status, kanTiles[0]) is not null)
            {
                builder.Add(new ChankanRonCandidate());
            }
        }
        else if (kanType is CallType.Ankan && rules_.AllowAnkanChankanForKokushi)
        {
            // 暗槓チャンカンは国士無双のみ成立。手牌13枚と暗槓牌種がすべて幺九牌である場合に候補を提示する
            // (役制限は ScoreCalculator 側で国士無双以外を不成立として担保する)
            var hand = round.HandArray[responderIndex];
            var callList = round.CallListArray[responderIndex];
            var status = round.PlayerRoundStatusArray[responderIndex];
            var ankanKind = kanTiles[0].Kind;
            if (IsYaochuuKind(ankanKind) &&
                hand.All(x => IsYaochuuKind(x.Kind)) &&
                TryBuildRonCandidate(hand, callList, status, kanTiles[0]) is not null)
            {
                builder.Add(new ChankanRonCandidate());
            }
        }

        builder.Add(new OkCandidate());
        return new CandidateList(builder.ToImmutable());
    }

    public CandidateList EnumerateForKanTsumo(Round round, PlayerIndex turnPlayerIndex)
    {
        ArgumentNullException.ThrowIfNull(round);
        ArgumentNullException.ThrowIfNull(turnPlayerIndex);

        var builder = ImmutableList.CreateBuilder<ResponseCandidate>();
        var hand = round.HandArray[turnPlayerIndex];
        var callList = round.CallListArray[turnPlayerIndex];
        var status = round.PlayerRoundStatusArray[turnPlayerIndex];

        builder.Add(BuildDahaiCandidate(round, turnPlayerIndex, hand, callList, status));
        if (TryBuildTsumoAgariCandidate(hand, callList, status) is not null)
        {
            builder.Add(new RinshanTsumoAgariCandidate());
        }
        if (!status.IsRiichi)
        {
            builder.AddRange(BuildAnkanCandidates(round, hand));
            builder.AddRange(BuildKakanCandidates(round, hand, callList));
        }

        return new CandidateList(builder.ToImmutable());
    }

    public CandidateList EnumerateForAfterKanTsumo(Round round, PlayerIndex turnPlayerIndex)
    {
        ArgumentNullException.ThrowIfNull(round);
        ArgumentNullException.ThrowIfNull(turnPlayerIndex);

        var builder = ImmutableList.CreateBuilder<ResponseCandidate>();
        var hand = round.HandArray[turnPlayerIndex];
        var callList = round.CallListArray[turnPlayerIndex];
        var status = round.PlayerRoundStatusArray[turnPlayerIndex];

        builder.Add(BuildDahaiCandidate(round, turnPlayerIndex, hand, callList, status));
        if (!status.IsRiichi)
        {
            builder.AddRange(BuildAnkanCandidates(round, hand));
            builder.AddRange(BuildKakanCandidates(round, hand, callList));
        }

        return new CandidateList(builder.ToImmutable());
    }

    private DahaiCandidate BuildDahaiCandidate(
        Round round,
        PlayerIndex turnPlayerIndex,
        Hand hand,
        CallList callList,
        PlayerRoundStatus status
    )
    {
        var riichiBase = !status.IsRiichi &&
            status.IsMenzen &&
            round.PointArray[turnPlayerIndex].Value >= RIICHI_POINT_MIN &&
            round.Wall.LiveRemaining >= RIICHI_WALL_MIN;
        var tsumoTile = hand.Last();
        var candidateTiles = status.IsRiichi ? (IEnumerable<Tile>)[tsumoTile] : hand;
        var options = ImmutableList.CreateBuilder<DahaiOption>();
        foreach (var tile in candidateTiles)
        {
            var remaining = new Hand(RemoveFirst(hand, tile));
            var riichiAvailable = riichiBase && tenpaiChecker_.IsTenpai(remaining, callList);
            options.Add(new DahaiOption(tile, riichiAvailable));
        }

        return new DahaiCandidate(new DahaiOptionList(options.ToImmutable()));
    }

    private TsumoAgariCandidate? TryBuildTsumoAgariCandidate(Hand hand, CallList callList, PlayerRoundStatus status)
    {
        // フリテンはロン/チャンカンのみ禁止で、ツモ和了は可能
        if (hand.Count() < 2)
        {
            return null;
        }

        var tsumoTile = hand.Last();
        var remaining = new Hand(RemoveFirst(hand, tsumoTile));
        var waits = tenpaiChecker_.EnumerateWaitTileKinds(remaining, callList);
        return waits.Contains(tsumoTile.Kind) ? new TsumoAgariCandidate() : null;
    }

    private RonCandidate? TryBuildRonCandidate(
        Hand hand,
        CallList callList,
        PlayerRoundStatus status,
        Tile targetTile
    )
    {
        if (status.IsFuriten || status.IsTemporaryFuriten)
        {
            return null;
        }

        var waits = tenpaiChecker_.EnumerateWaitTileKinds(hand, callList);
        return waits.Contains(targetTile.Kind) ? new RonCandidate() : null;
    }

    /// <summary>
    /// 指定牌種の手牌から pickCount 枚を選ぶ全ての赤ドラ枚数バリエーションを列挙します。
    /// 赤ドラ k 枚 + 非赤ドラ (pickCount - k) 枚の組合せを、k が取りうる範囲で yield します。
    /// 同色の非赤ドラ同士・赤ドラ同士はスコア的に交換可能であるため、同一 k に対する順列は代表 1 件のみ返します。
    /// ルールで赤ドラが複数枚許容される場合 (例: 赤5萬×2) でも、k=0..min(redCount, pickCount) の範囲で正しく列挙します。
    /// </summary>
    private IEnumerable<ImmutableArray<Tile>> EnumerateRedCountVariants(IEnumerable<Tile> tilesOfKind, int pickCount)
    {
        var reds = tilesOfKind.Where(rules_.IsRedDora).ToList();
        var nonReds = tilesOfKind.Where(x => !rules_.IsRedDora(x)).ToList();
        if (reds.Count + nonReds.Count < pickCount) { yield break; }

        var minReds = Math.Max(0, pickCount - nonReds.Count);
        var maxReds = Math.Min(reds.Count, pickCount);
        for (var k = minReds; k <= maxReds; k++)
        {
            var builder = ImmutableArray.CreateBuilder<Tile>(pickCount);
            builder.AddRange(nonReds.Take(pickCount - k));
            builder.AddRange(reds.Take(k));
            yield return builder.ToImmutable();
        }
    }

    private IEnumerable<ChiCandidate> BuildChiCandidates(
        Round round,
        PlayerIndex responderIndex,
        Hand hand,
        Tile discardedTile
    )
    {
        if (responderIndex != round.Turn.Next() || discardedTile.Kind >= 27) { yield break; }

        var suit = discardedTile.Kind / 9;
        var tilesByKind = hand.GroupBy(x => x.Kind).ToDictionary(x => x.Key, x => x.ToList());
        var patterns = new (int KindA, int KindB)[]
        {
            (discardedTile.Kind - 2, discardedTile.Kind - 1),
            (discardedTile.Kind - 1, discardedTile.Kind + 1),
            (discardedTile.Kind + 1, discardedTile.Kind + 2),
        };
        foreach (var (kindA, kindB) in patterns)
        {
            if (kindA < 0 || kindB < 0 || kindA >= 27 || kindB >= 27 ||
                kindA / 9 != suit || kindB / 9 != suit ||
                !tilesByKind.TryGetValue(kindA, out var tilesA) ||
                !tilesByKind.TryGetValue(kindB, out var tilesB))
            {
                continue;
            }

            // 順子はそれぞれの牌種から 1 枚ずつ、赤ドラ採用パターンの組合せを列挙
            foreach (var variantA in EnumerateRedCountVariants(tilesA, 1))
            {
                foreach (var variantB in EnumerateRedCountVariants(tilesB, 1))
                {
                    yield return new ChiCandidate([variantA[0], variantB[0]]);
                }
            }
        }
    }

    private IEnumerable<PonCandidate> BuildPonCandidates(Hand hand, Tile discardedTile)
    {
        var sameKind = hand.Where(x => x.Kind == discardedTile.Kind).ToList();
        foreach (var variant in EnumerateRedCountVariants(sameKind, 2))
        {
            yield return new PonCandidate(variant);
        }
    }

    private IEnumerable<DaiminkanCandidate> BuildDaiminkanCandidates(Round round, Hand hand, Tile discardedTile)
    {
        if (!round.Wall.CanKan) { yield break; }

        var sameKind = hand.Where(x => x.Kind == discardedTile.Kind).ToList();
        foreach (var variant in EnumerateRedCountVariants(sameKind, 3))
        {
            yield return new DaiminkanCandidate(variant);
        }
    }

    private static IEnumerable<AnkanCandidate> BuildAnkanCandidates(Round round, Hand hand)
    {
        if (!round.Wall.CanKan) { yield break; }

        // 暗槓は手牌の同種 4 枚全てを使用するため、赤ドラ有無で選択肢は生じない
        foreach (var group in hand.GroupBy(x => x.Kind).Where(x => x.Count() >= 4))
        {
            yield return new AnkanCandidate([.. group.Take(4)]);
        }
    }

    private IEnumerable<KakanCandidate> BuildKakanCandidates(Round round, Hand hand, CallList callList)
    {
        if (!round.Wall.CanKan) { yield break; }

        var ponKinds = callList.Where(x => x.Type == CallType.Pon).Select(x => x.Tiles[0].Kind).ToImmutableHashSet();
        var handByKind = hand.GroupBy(x => x.Kind).ToDictionary(x => x.Key, x => x.ToList());
        foreach (var kind in ponKinds)
        {
            if (!handByKind.TryGetValue(kind, out var tilesOfKind)) { continue; }
            foreach (var variant in EnumerateRedCountVariants(tilesOfKind, 1))
            {
                yield return new KakanCandidate(variant[0]);
            }
        }
    }

    private static IEnumerable<Tile> RemoveFirst(Hand hand, Tile target)
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

    private static bool IsYaochuuKind(int kind)
    {
        return kind is 0 or 8 or 9 or 17 or 18 or 26 or >= 27;
    }
}
