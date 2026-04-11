using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.HandCalculating.HandDividing;

/// <summary>
/// 手牌を分解するクラス
/// </summary>
public static class HandDivider
{
    /// <summary>
    /// 牌リストを実現可能な組み合わせに分解し、組み合わされた手牌のリストを返します
    /// </summary>
    /// <param name="tileKindList">分解する牌リスト</param>
    /// <returns>可能な手牌の組み合わせのリスト</returns>
    public static List<Hand> Divide(TileKindList tileKindList)
    {
        // 必要な面子数を計算
        var requiredMentsuCount = tileKindList.Count / 3 + 1;
        var toitsuTiles = FindToitsuTiles(tileKindList);
        // 通常の面子構成の手牌を取得
        var hands = GenerateNormalHands(tileKindList, toitsuTiles, requiredMentsuCount);
        var resultHands = new HashSet<Hand>();
        foreach (var h in hands)
        {
            var sortedHand = new Hand([.. h.OrderBy(x => x)]);
            resultHands.Add(sortedHand);
        }
        // 七対子の判定
        if (toitsuTiles.Count == 7)
        {
            resultHands.Add([.. toitsuTiles.Select(x => new TileKindList(Enumerable.Repeat(x, 2)))]);
        }
        // ソートして結果を返す
        var sortedResults = resultHands.OrderBy(x => x).ToList();
        return sortedResults;
    }

    /// <summary>
    /// 通常の面子構成（4面子1雀頭）の手牌リストを生成します
    /// </summary>
    /// <param name="tileKindList">元の牌リスト</param>
    /// <param name="toitsuTiles">雀頭候補の牌リスト</param>
    /// <param name="requiredMentsuCount">必要な面子数</param>
    /// <returns>生成された手牌のリスト</returns>
    private static List<Hand> GenerateNormalHands(TileKindList tileKindList, TileKindList toitsuTiles, int requiredMentsuCount)
    {
        var hands = new List<Hand>();
        // 各雀頭候補について手牌の組み合わせを検索
        foreach (var toitsuTile in toitsuTiles)
        {
            // 雀頭として使用する牌を2枚除去
            var withoutHeadList = tileKindList.Remove(toitsuTile, 2);
            // 面子候補の組み合わせを生成
            var suits = GenerateMentsuCombinations(withoutHeadList, toitsuTile);
            // 各色の面子候補の直積を計算して全ての組み合わせを生成
            foreach (var prod in Product(suits))
            {
                var h = prod.SelectMany(x => x);
                // 必要な面子数と一致する場合のみ有効な手牌として追加
                if (h.Count() == requiredMentsuCount)
                {
                    h = h.OrderBy(x => x[0]);
                    hands.Add([.. h]);
                }
            }
        }
        return hands;
    }

    /// <summary>
    /// 雀頭候補となる牌のリストを取得する
    /// </summary>
    /// <param name="tileKindList">牌の集合</param>
    /// <returns>雀頭候補となる牌のリスト</returns>
    private static TileKindList FindToitsuTiles(TileKindList tileKindList)
    {
        // 字牌の同種は2枚のときのみ対子になり得る
        return [.. tileKindList.Distinct().Where(x => x.IsHonor && tileKindList.CountOf(x) == 2 || !x.IsHonor && tileKindList.CountOf(x) >= 2)];
    }

    /// <summary>
    /// 面子候補の組み合わせを生成します
    /// </summary>
    /// <param name="tileKindList">雀頭を除いた牌リスト</param>
    /// <param name="toitsuTile">雀頭の牌</param>
    /// <returns>面子候補の組み合わせリスト</returns>
    private static List<List<TileKindListList>> GenerateMentsuCombinations(TileKindList tileKindList, TileKind toitsuTile)
    {
        // 萬子・筒子・索子の面子候補を取得
        var man = FindValidCombinationsMan(tileKindList);
        var pin = FindValidCombinationsPin(tileKindList);
        var sou = FindValidCombinationsSou(tileKindList);
        // 字牌の刻子候補を取得
        TileKindListList honor = [.. TileKind.Honors.Where(x => tileKindList.CountOf(x) == 3).Select(x => new TileKindList(Enumerable.Repeat(x, 3)))];
        // 雀頭と各色の面子候補を組み合わせリストに追加
        List<List<TileKindListList>> suits = [[[[.. Enumerable.Repeat(toitsuTile, 2)]]]];
        if (man.Count != 0) { suits.Add(man); }
        if (pin.Count != 0) { suits.Add(pin); }
        if (sou.Count != 0) { suits.Add(sou); }
        if (honor.Count != 0) { suits.Add([honor]); }
        return suits;
    }

    /// <summary>
    /// 指定された牌種類の有効な面子の組み合わせを探す
    /// </summary>
    /// <param name="tileKinds">雀頭候補を除いた牌リスト</param>
    /// <returns>有効な面子の組み合わせのリスト</returns>
    private static List<TileKindListList> FindValidCombinations(IEnumerable<TileKind> tileKinds)
    {
        var tileList = tileKinds.ToList();
        if (tileList.Count == 0 || tileList.Count % 3 != 0) { return []; }

        var memo = new Dictionary<string, List<TileKindListList>>();
        var result = FindCombinationsRecursive(tileList, memo);
        return result;
    }

    /// <summary>
    /// 萬子の有効な面子の組み合わせを探します
    /// </summary>
    private static List<TileKindListList> FindValidCombinationsMan(TileKindList tileKindList)
    {
        return FindValidCombinations(tileKindList.Where(x => x.IsMan));
    }

    /// <summary>
    /// 筒子の有効な面子の組み合わせを探します
    /// </summary>
    private static List<TileKindListList> FindValidCombinationsPin(TileKindList tileKindList)
    {
        return FindValidCombinations(tileKindList.Where(x => x.IsPin));
    }

    /// <summary>
    /// 索子の有効な面子の組み合わせを探します
    /// </summary>
    private static List<TileKindListList> FindValidCombinationsSou(TileKindList tileKindList)
    {
        return FindValidCombinations(tileKindList.Where(x => x.IsSou));
    }

    /// <summary>
    /// 再帰的に面子の組み合わせを探す
    /// </summary>
    /// <param name="remainingTiles">残りの牌リスト</param>
    /// <param name="memo">メモ化用辞書</param>
    /// <returns>有効な面子の組み合わせのリスト</returns>
    private static List<TileKindListList> FindCombinationsRecursive(List<TileKind> remainingTiles, Dictionary<string, List<TileKindListList>> memo)
    {
        if (remainingTiles.Count == 0) { return [[]]; }

        var key = string.Join(",", remainingTiles.OrderBy(x => x));
        if (memo.TryGetValue(key, out var cachedResult)) { return cachedResult; }

        var combinations = new List<TileKindListList>();
        var tileCounts = GetTileCounts(remainingTiles);
        var firstTile = remainingTiles[0];
        // 刻子を試す
        if (tileCounts[firstTile] >= 3)
        {
            var newRemaining = RemoveTiles(remainingTiles, firstTile, 3);
            var subCombinations = FindCombinationsRecursive(newRemaining, memo);
            foreach (var subComb in subCombinations)
            {
                var koutsu = new TileKindList([firstTile, firstTile, firstTile]);
                combinations.Add([koutsu, .. subComb]);
            }
        }
        // 順子を試す（数牌のみ）
        if (!firstTile.IsHonor)
        {
            if (firstTile.TryGetAtDistance(1, out var nextTile) &&
                firstTile.TryGetAtDistance(2, out var nextNextTile) &&
                tileCounts.GetValueOrDefault(nextTile, 0) >= 1 &&
                tileCounts.GetValueOrDefault(nextNextTile, 0) >= 1)
            {
                var newRemaining = RemoveTiles(RemoveTiles(RemoveTiles(remainingTiles, firstTile, 1), nextTile, 1), nextNextTile, 1);
                var subCombinations = FindCombinationsRecursive(newRemaining, memo);
                foreach (var subComb in subCombinations)
                {
                    var shuntsu = new TileKindList([firstTile, nextTile, nextNextTile]);
                    combinations.Add([shuntsu, .. subComb]);
                }
            }
        }
        memo[key] = combinations;
        return combinations;
    }

    /// <summary>
    /// 牌の出現回数をカウントする
    /// </summary>
    private static Dictionary<TileKind, int> GetTileCounts(List<TileKind> tiles)
    {
        return tiles.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
    }

    /// <summary>
    /// 指定した牌を指定数だけ除去する
    /// </summary>
    private static List<TileKind> RemoveTiles(List<TileKind> tiles, TileKind tile, int count)
    {
        var result = new List<TileKind>(tiles);
        var removed = 0;
        for (var i = result.Count - 1; i >= 0 && removed < count; i--)
        {
            if (result[i] == tile)
            {
                result.RemoveAt(i);
                removed++;
            }
        }
        return result;
    }

    /// <summary>
    /// 複数のリストの直積を計算する
    /// </summary>
    private static List<List<TileKindListList>> Product(List<List<TileKindListList>> suits)
    {
        return suits.Count switch
        {
            1 => suits,
            2 => [.. suits[0].SelectMany(s0 => suits[1].Select(s1 => new[] { s0, s1 }.ToList()))],
            3 => [.. suits[0].SelectMany(s0 => suits[1].SelectMany(s1 => suits[2].Select(s2 => new[] { s0, s1, s2 }.ToList())))],
            4 => [.. suits[0].SelectMany(s0 => suits[1].SelectMany(s1 => suits[2].SelectMany(s2 => suits[3].Select(s3 => new[] { s0, s1, s2, s3 }.ToList()))))],
            // 雀頭 + 萬子 + 筒子 + 索子 + 字牌 で最大 5。想定外の件数にはならない
            _ => [.. suits[0].SelectMany(s0 => suits[1].SelectMany(s1 => suits[2].SelectMany(s2 => suits[3].SelectMany(s3 => suits[4].Select(s4 => new[] { s0, s1, s2, s3, s4 }.ToList())))))],
        };
    }
}
