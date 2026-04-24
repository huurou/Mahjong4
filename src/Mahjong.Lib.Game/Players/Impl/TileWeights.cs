using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Scoring.Tiles;
using Hand = Mahjong.Lib.Game.Hands.Hand;

namespace Mahjong.Lib.Game.Players.Impl;

/// <summary>
/// 0305 の一色手・大三元・四喜和狙いで用いる牌種別重み。
/// 34 牌種 × 整数倍率 (既定値 1)。染め色+字牌 ≥ 10 枚 / 三元牌 ≥ 3 枚 / 風牌 ≥ 2 枚 で条件を満たすと、
/// 対応する牌種の倍率を ×4〜×8 に上げる (書籍 0305 make_paijia 準拠)。
/// v0.6.1 以降は HandShapeEvaluator の低シャンテン再帰 (EvaluateHand13 / EvaluateBacktrack) にも乗算する。
/// </summary>
internal sealed class TileWeights
{
    private readonly int[] weights_;

    /// <summary>
    /// 全ての牌種に倍率 1 を与える既定値。染め判定が未成立の局面で使用する。
    /// </summary>
    public static TileWeights AllOne { get; } = CreateAllOne();

    private TileWeights(int[] weights)
    {
        if (weights.Length != 34)
        {
            throw new ArgumentException($"weights は 34 要素である必要があります。実際: {weights.Length}", nameof(weights));
        }
        weights_ = weights;
    }

    /// <summary>
    /// 指定牌種の倍率を返す。
    /// </summary>
    public int Of(TileKind kind)
    {
        return weights_[kind.Value];
    }

    /// <summary>
    /// 34 要素の倍率配列からインスタンスを生成する。
    /// </summary>
    internal static TileWeights FromArray(int[] weights)
    {
        return new TileWeights(weights);
    }

    /// <summary>
    /// 手牌と副露から 0305 の染め/三元/四喜狙い重みを構築する (書籍 make_paijia 準拠)。
    /// 条件:
    ///   - 特定色 (萬子/筒子/索子) + 字牌 ≥ 10 枚 → その色の数牌 ×4 (字牌は既定値 1 のまま)
    ///   - 三元牌 ≥ 3 枚 → 三元牌 ×8
    ///   - 風牌 ≥ 2 枚 → 風牌 ×4
    /// 複数条件が同時成立すると倍率は乗算される (例: 染めと三元が両立すれば三元牌 ×8)。
    /// </summary>
    public static TileWeights Build(Hand hand, CallList calls)
    {
        Span<int> kindCounts = stackalloc int[34];
        foreach (var tile in hand)
        {
            kindCounts[tile.Kind.Value]++;
        }
        foreach (var call in calls)
        {
            foreach (var tile in call.Tiles)
            {
                kindCounts[tile.Kind.Value]++;
            }
        }

        // スート別合計 (0:萬子, 1:筒子, 2:索子, 3:字牌)
        Span<int> suitCount = stackalloc int[4];
        for (var i = 0; i < 34; i++)
        {
            var suit = i < 27 ? i / 9 : 3;
            suitCount[suit] += kindCounts[i];
        }
        var dragonCount = kindCounts[31] + kindCounts[32] + kindCounts[33];
        var windCount = kindCounts[27] + kindCounts[28] + kindCounts[29] + kindCounts[30];

        var weights = new int[34];
        Array.Fill(weights, 1);

        // 染め狙い: 特定色 + 字牌 ≥ 10 枚 (書籍準拠、字牌は既定値 1 のまま)
        for (var suit = 0; suit < 3; suit++)
        {
            if (suitCount[suit] + suitCount[3] < 10) { continue; }
            // その色の数牌 ×4
            for (var i = suit * 9; i < suit * 9 + 9; i++)
            {
                weights[i] *= 4;
            }
        }

        // 三元牌 ≥ 3 枚 → 三元牌 ×8 (書籍準拠)
        if (dragonCount >= 3)
        {
            for (var i = 31; i < 34; i++)
            {
                weights[i] *= 8;
            }
        }

        // 風牌 ≥ 2 枚 → 風牌 ×4 (書籍準拠)
        if (windCount >= 2)
        {
            for (var i = 27; i < 31; i++)
            {
                weights[i] *= 4;
            }
        }

        return new TileWeights(weights);
    }

    private static TileWeights CreateAllOne()
    {
        var arr = new int[34];
        Array.Fill(arr, 1);
        return new TileWeights(arr);
    }
}
