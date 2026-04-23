using Mahjong.Lib.Game.Calls;

namespace Mahjong.Lib.Game.Players.Impl;

/// <summary>
/// 副露リストを 64 bit FNV-1a ハッシュにまとめたキャッシュキー。
/// <see cref="CallList"/> の record 構造等価比較は内部で <see cref="System.Collections.Immutable.ImmutableList{T}.SequenceEqual"/>
/// を呼ぶため Dictionary の key に直接使うと比較コストが高い。
/// 副露は <see cref="CallType"/> と手内の牌種列だけで区別できるため、それらをハッシュ化して固定 8 byte の key として使う。
/// ハッシュ衝突 (2^-64) は実用上許容する。
/// </summary>
internal readonly struct CallsSignature : IEquatable<CallsSignature>
{
    private const ulong FNV_OFFSET = 14695981039346656037UL;
    private const ulong FNV_PRIME = 1099511628211UL;

    private readonly ulong hash_;

    private CallsSignature(ulong hash)
    {
        hash_ = hash;
    }

    /// <summary>
    /// 空の副露リストを示す既定値 (ハッシュは FNV 初期値)。
    /// </summary>
    public static CallsSignature Empty { get; } = new(FNV_OFFSET);

    /// <summary>
    /// <see cref="CallList"/> からシグネチャを生成する。
    /// Call 順序も含めてハッシュ化する (同一副露を重複追加することは通常ないため順序違いは構造違い扱い)。
    /// </summary>
    public static CallsSignature FromCalls(CallList calls)
    {
        var h = FNV_OFFSET;
        foreach (var call in calls)
        {
            h ^= (ulong)(byte)call.Type;
            h *= FNV_PRIME;
            foreach (var tile in call.Tiles)
            {
                h ^= (ulong)(uint)tile.Kind.Value;
                h *= FNV_PRIME;
            }
            // 区切り (異なる副露間の境界を区別)
            h ^= 0xFFUL;
            h *= FNV_PRIME;
        }
        return new CallsSignature(h);
    }

    public bool Equals(CallsSignature other)
    {
        return hash_ == other.hash_;
    }

    public override bool Equals(object? obj)
    {
        return obj is CallsSignature other && Equals(other);
    }

    public override int GetHashCode()
    {
        return hash_.GetHashCode();
    }

    public static bool operator ==(CallsSignature left, CallsSignature right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CallsSignature left, CallsSignature right)
    {
        return !left.Equals(right);
    }
}
