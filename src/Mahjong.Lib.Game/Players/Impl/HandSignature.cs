using Mahjong.Lib.Game.Hands;

namespace Mahjong.Lib.Game.Players.Impl;

/// <summary>
/// 手牌の牌種別枚数を 34 スロット × 3 bit にビットパックしたキャッシュキー。
/// 同一の牌種枚数を持つ手牌 (順序違い・赤黒違いを含む) は同じ署名になる。
/// ulong 2 本 (計 128 bit、内 102 bit を使用) で保持するため等価比較・ハッシュ計算が高速。
/// </summary>
internal readonly struct HandSignature : IEquatable<HandSignature>
{
    private readonly ulong lo_;   // slot[0..20] を 3 bit ずつパック (合計 63 bit)
    private readonly ulong hi_;   // slot[21..33] を 3 bit ずつパック (合計 39 bit)

    private HandSignature(ulong lo, ulong hi)
    {
        lo_ = lo;
        hi_ = hi;
    }

    /// <summary>
    /// Hand の牌を牌種別ごとに集計し、署名を生成する。
    /// </summary>
    public static HandSignature FromHand(Hand hand)
    {
        Span<int> counts = stackalloc int[34];
        foreach (var tile in hand)
        {
            counts[tile.Kind.Value]++;
        }
        return FromCounts(counts);
    }

    /// <summary>
    /// 34 要素の牌種別枚数配列から署名を生成する。各要素は 0-4 の範囲を想定 (3 bit で表現)。
    /// </summary>
    public static HandSignature FromCounts(ReadOnlySpan<int> counts)
    {
        if (counts.Length != 34)
        {
            throw new ArgumentException($"counts は 34 要素である必要があります。実際: {counts.Length}", nameof(counts));
        }

        ulong lo = 0;
        ulong hi = 0;
        for (var i = 0; i < 21; i++)
        {
            lo |= (ulong)(counts[i] & 0x7) << (i * 3);
        }
        for (var i = 21; i < 34; i++)
        {
            hi |= (ulong)(counts[i] & 0x7) << ((i - 21) * 3);
        }
        return new HandSignature(lo, hi);
    }

    public bool Equals(HandSignature other)
    {
        return lo_ == other.lo_ && hi_ == other.hi_;
    }

    public override bool Equals(object? obj)
    {
        return obj is HandSignature other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(lo_, hi_);
    }

    public static bool operator ==(HandSignature left, HandSignature right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HandSignature left, HandSignature right)
    {
        return !left.Equals(right);
    }
}
