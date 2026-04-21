using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Adoptions;

/// <summary>
/// 流局採用結果
/// </summary>
/// <param name="Type">流局種別</param>
/// <param name="TenpaiPlayerIndices">テンパイ者 (荒牌平局時)</param>
/// <param name="NagashiManganPlayerIndices">流し満貫者 (荒牌平局時)</param>
/// <param name="PointDeltas">精算による点数移動 (index=プレイヤー, 値=精算後-精算前)。
/// 途中流局 (点数移動なし) は全要素 0 の PointArray</param>
/// <param name="DealerContinues">親続行か</param>
public record AdoptedRyuukyokuAction(
    RyuukyokuType Type,
    ImmutableList<PlayerIndex> TenpaiPlayerIndices,
    ImmutableList<PlayerIndex> NagashiManganPlayerIndices,
    PointArray PointDeltas,
    bool DealerContinues
) : AdoptedRoundAction
{
    public virtual bool Equals(AdoptedRyuukyokuAction? other)
    {
        return other is not null &&
            Type == other.Type &&
            TenpaiPlayerIndices.SequenceEqual(other.TenpaiPlayerIndices) &&
            NagashiManganPlayerIndices.SequenceEqual(other.NagashiManganPlayerIndices) &&
            PointDeltas == other.PointDeltas &&
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
        hash.Add(PointDeltas);
        hash.Add(DealerContinues);
        return hash.ToHashCode();
    }
}
