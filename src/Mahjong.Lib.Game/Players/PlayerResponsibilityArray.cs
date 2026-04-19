using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Players;

/// <summary>
/// 各プレイヤーの包 (責任払い) 責任者の配列
/// index = 和了者 PlayerIndex, 値 = 責任者 PlayerIndex (未確定時は null)
/// </summary>
public record PlayerResponsibilityArray
{
    private readonly ImmutableArray<PlayerIndex?> responsibles_;

    public PlayerResponsibilityArray()
    {
        responsibles_ = [.. new PlayerIndex?[4]];
    }

    private PlayerResponsibilityArray(ImmutableArray<PlayerIndex?> responsibles)
    {
        responsibles_ = responsibles;
    }

    /// <summary>
    /// 指定の和了者に対する責任者 PlayerIndex を返します。未確定時は null。
    /// </summary>
    public PlayerIndex? this[PlayerIndex winnerIndex] => responsibles_[winnerIndex.Value];

    /// <summary>
    /// 指定の和了者に対する責任者を設定した新しい PlayerResponsibilityArray を返します。
    /// </summary>
    public PlayerResponsibilityArray SetResponsible(PlayerIndex winnerIndex, PlayerIndex responsibleIndex)
    {
        return new PlayerResponsibilityArray(responsibles_.SetItem(winnerIndex.Value, responsibleIndex));
    }

    public virtual bool Equals(PlayerResponsibilityArray? other)
    {
        return other is not null && responsibles_.SequenceEqual(other.responsibles_);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var r in responsibles_)
        {
            hash.Add(r);
        }
        return hash.ToHashCode();
    }
}
