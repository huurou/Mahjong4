using Mahjong.Lib.Game.Players;
using System.Collections;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds;

/// <summary>
/// 各プレイヤーの局内状態の配列
/// </summary>
public record PlayerRoundStatusArray : IEnumerable<PlayerRoundStatus>
{
    private readonly ImmutableArray<PlayerRoundStatus> statuses_;

    public PlayerRoundStatusArray()
    {
        statuses_ = [.. Enumerable.Range(0, PlayerIndex.PLAYER_COUNT).Select(_ => new PlayerRoundStatus())];
    }

    private PlayerRoundStatusArray(ImmutableArray<PlayerRoundStatus> statuses)
    {
        statuses_ = statuses;
    }

    public PlayerRoundStatus this[PlayerIndex index] => statuses_[index.Value];

    /// <summary>
    /// 指定のプレイヤーの局内状態を置き換えた新しい PlayerRoundStatusArray を返します
    /// </summary>
    public PlayerRoundStatusArray SetStatus(PlayerIndex index, PlayerRoundStatus status)
    {
        var builder = statuses_.ToBuilder();
        builder[index.Value] = status;
        return new PlayerRoundStatusArray(builder.ToImmutable());
    }

    public virtual bool Equals(PlayerRoundStatusArray? other)
    {
        return other is PlayerRoundStatusArray array && statuses_.SequenceEqual(array.statuses_);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var status in statuses_)
        {
            hash.Add(status);
        }
        return hash.ToHashCode();
    }

    public IEnumerator<PlayerRoundStatus> GetEnumerator()
    {
        return ((IEnumerable<PlayerRoundStatus>)statuses_).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)statuses_).GetEnumerator();
    }
}
