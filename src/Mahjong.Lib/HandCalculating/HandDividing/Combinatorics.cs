namespace Mahjong.Lib.HandCalculating.HandDividing;

public static class Combinatorics
{
    /// <summary>
    /// source の要素から長さ k の順列 (k-permutations) を遅延生成します。
    /// </summary>
    /// <typeparam name="T">要素の型</typeparam>
    /// <param name="source">元となる要素のコレクション</param>
    /// <param name="k">生成する順列の長さ</param>
    /// <returns>長さ k の順列のコレクション</returns>
    public static IEnumerable<IEnumerable<T>> Permutations<T>(IEnumerable<T> source, int k) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        // ベースケース: k == 0 → 空の順列を 1 つ返す
        if (k == 0)
        {
            yield return [];
            yield break;
        }

        // 解なしケース: k が不適切 or 要素不足
        var list = source.ToList();
        if (k < 0 || k > list.Count) { yield break; }

        // 再帰ステップ：各要素を先頭に選んで残りで長さ k-1 の順列を生成
        for (var i = 0; i < list.Count; i++)
        {
            var head = list[i];
            // 残り要素を取得
            var tail = list.Take(i).Concat(list.Skip(i + 1));
            foreach (var perm in Permutations(tail, k - 1))
            {
                yield return [head, .. perm];
            }
        }
    }

    /// <summary>
    /// source の要素から長さ k の組み合わせ (k-combinations) を遅延生成します。
    /// </summary>
    /// <typeparam name="T">要素の型</typeparam>
    /// <param name="source">元となる要素のコレクション</param>
    /// <param name="k">生成する組み合わせの長さ</param>
    /// <returns>長さ k の組み合わせのコレクション</returns>
    public static IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> source, int k) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        // ベースケース: k == 0 → 空の組み合わせを 1 つ返す
        if (k == 0)
        {
            yield return []; yield break;
        }

        // 解なしケース: k が不適切 or 要素不足
        var list = source.ToList();
        if (k < 0 || k > list.Count) { yield break; }

        // 再帰ステップ：各要素を選ぶか選ばないかで組み合わせを生成
        for (var i = 0; i < list.Count; i++)
        {
            var head = list[i];
            var tail = list.Skip(i + 1);
            foreach (var combination in Combinations(tail, k - 1))
            {
                yield return [head, .. combination];
            }
        }
    }
}
