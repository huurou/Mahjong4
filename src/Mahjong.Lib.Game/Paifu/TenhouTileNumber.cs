using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Paifu;

/// <summary>
/// <see cref="Tile"/> (0-135) を天鳳 JSON 牌譜の牌番号に変換する
/// </summary>
/// <remarks>
/// 牌番号体系:
/// <list type="bullet">
/// <item>11-19: 1m-9m</item>
/// <item>21-29: 1p-9p</item>
/// <item>31-39: 1s-9s</item>
/// <item>41-47: 東/南/西/北/白/發/中</item>
/// <item>51: 赤 5m / 52: 赤 5p / 53: 赤 5s (赤ドラルール有効時のみ)</item>
/// </list>
/// 赤ドラは <see cref="GameRules.RedDoraTiles"/> に含まれる Tile を 51/52/53 にマッピングする。
/// ルール OFF (集合に含まれない) なら通常の 15/25/35 として扱う
/// </remarks>
public static class TenhouTileNumber
{
    /// <summary>
    /// <paramref name="tile"/> を天鳳牌番号に変換する
    /// </summary>
    /// <param name="tile">変換対象の牌</param>
    /// <param name="rules">対局ルール (赤ドラ判定用)</param>
    public static int Convert(Tile tile, GameRules rules)
    {
        if (rules.RedDoraTiles.Contains(tile))
        {
            var kind = tile.Id / 4;
            // 赤5m = kind 4, 赤5p = kind 13, 赤5s = kind 22 (Tile.Id 16/52/88)
            return kind switch
            {
                4 => 51,
                13 => 52,
                22 => 53,
                _ => ConvertNormal(tile),
            };
        }
        return ConvertNormal(tile);
    }

    private static int ConvertNormal(Tile tile)
    {
        var kind = tile.Id / 4;
        if (kind >= 27)
        {
            // 字牌 27-33 → 41-47
            return 41 + (kind - 27);
        }
        var suit = kind / 9; // 0=m, 1=p, 2=s
        var rank = kind % 9 + 1; // 1-9
        return (suit + 1) * 10 + rank;
    }
}
