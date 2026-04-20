using System.Collections.Immutable;
using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Scoring.Tests;

internal static class TestHelper
{
    public static Tile Tile(int kind, int copy = 0)
    {
        return new Tile(kind * 4 + copy);
    }

    public static Hand Hand(params int[] kinds)
    {
        var seen = new int[34];
        var tiles = new List<Tile>();
        foreach (var kind in kinds)
        {
            tiles.Add(Tile(kind, seen[kind]));
            seen[kind]++;
        }
        return new Hand(tiles);
    }

    public static CallList EmptyCallList()
    {
        return new CallList([]);
    }

    /// <summary>
    /// 指定牌種の Pon 副露を生成する (from=他家、called=tiles[0])
    /// </summary>
    public static Call Pon(int kind)
    {
        var tiles = ImmutableList.Create(Tile(kind, 0), Tile(kind, 1), Tile(kind, 2));
        return new Call(CallType.Pon, tiles, new PlayerIndex(1), tiles[0]);
    }

    /// <summary>
    /// 指定牌種の Ankan 副露を生成する
    /// </summary>
    public static Call Ankan(int kind)
    {
        var tiles = ImmutableList.Create(Tile(kind, 0), Tile(kind, 1), Tile(kind, 2), Tile(kind, 3));
        return new Call(CallType.Ankan, tiles, new PlayerIndex(0), calledTile: null);
    }

    /// <summary>
    /// 指定 3 牌種の Chi 副露を生成する (最小牌を called として扱う)
    /// </summary>
    public static Call Chi(int kind1, int kind2, int kind3)
    {
        var tiles = ImmutableList.Create(Tile(kind1, 0), Tile(kind2, 0), Tile(kind3, 0));
        return new Call(CallType.Chi, tiles, new PlayerIndex(1), tiles[0]);
    }

    public static CallList CallList(params Call[] calls)
    {
        return [.. calls];
    }
}
