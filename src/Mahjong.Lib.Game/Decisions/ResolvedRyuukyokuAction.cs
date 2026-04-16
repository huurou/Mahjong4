using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 流局採用結果
/// </summary>
/// <param name="Type">流局種別</param>
/// <param name="TenpaiPlayerIndices">テンパイ者 (荒牌平局時)</param>
/// <param name="NagashiManganPlayerIndices">流し満貫者 (荒牌平局時)</param>
/// <param name="DealerContinues">親続行か</param>
public record ResolvedRyuukyokuAction(
    RyuukyokuType Type,
    ImmutableList<PlayerIndex> TenpaiPlayerIndices,
    ImmutableList<PlayerIndex> NagashiManganPlayerIndices,
    bool DealerContinues
) : ResolvedRoundAction
{
    public virtual bool Equals(ResolvedRyuukyokuAction? other)
    {
        return other is not null &&
            Type == other.Type &&
            TenpaiPlayerIndices.SequenceEqual(other.TenpaiPlayerIndices) &&
            NagashiManganPlayerIndices.SequenceEqual(other.NagashiManganPlayerIndices) &&
            DealerContinues == other.DealerContinues;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Type);
        foreach (var index in TenpaiPlayerIndices)
        {
            hash.Add(index);
        }
        foreach (var index in NagashiManganPlayerIndices)
        {
            hash.Add(index);
        }
        hash.Add(DealerContinues);
        return hash.ToHashCode();
    }
}
