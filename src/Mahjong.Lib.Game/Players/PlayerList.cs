using System.Collections;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Players;

/// <summary>
/// プレイヤー席のリスト
/// index 0 が起家となる仕様 起家決定・並び替えは呼び出し側の責務
/// </summary>
public class PlayerList : IEnumerable<Player>
{
    private readonly ImmutableArray<Player> players_;

    public PlayerList(IEnumerable<Player> players)
    {
        var array = players.ToImmutableArray();
        if (array.Length != PlayerIndex.PLAYER_COUNT)
        {
            throw new ArgumentException($"プレイヤー数は {PlayerIndex.PLAYER_COUNT} 人である必要があります。", nameof(players));
        }
        for (var i = 0; i < array.Length; i++)
        {
            if (array[i].PlayerIndex.Value != i)
            {
                throw new ArgumentException(
                    $"プレイヤーの PlayerIndex は PlayerList 内の位置と一致する必要があります。位置:{i} PlayerIndex:{array[i].PlayerIndex.Value}",
                    nameof(players)
                );
            }
        }
        players_ = array;
    }

    public Player this[PlayerIndex index] => players_[index.Value];

    public int Count => players_.Length;

    public IEnumerator<Player> GetEnumerator()
    {
        return ((IEnumerable<Player>)players_).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)players_).GetEnumerator();
    }
}
