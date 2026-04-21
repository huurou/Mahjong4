using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Text;

namespace Mahjong.Lib.Game.Paifu;

/// <summary>
/// <see cref="Call"/> を天鳳 JSON 牌譜の副露文字列 (c/p/m/a/k) にエンコードする
/// </summary>
/// <remarks>
/// tenhou.net/6 形式では、鳴き元の相対位置を符号文字の挿入位置で表現する:
/// <list type="bullet">
/// <item>チー: 必ず上家からなので <c>c{called}{t1}{t2}</c> 固定</item>
/// <item>ポン: 上家=<c>p{called}{t1}{t2}</c> / 対面=<c>{t1}p{called}{t2}</c> / 下家=<c>{t1}{t2}p{called}</c></item>
/// <item>大明槓: ポンと同じ配置規則で <c>m</c> を使用</item>
/// <item>暗槓: <c>{b}{b}{b}a{t}</c> (b=通常牌、赤ドラがあれば t を赤に)</item>
/// <item>加槓: 元ポンの <c>p</c> を <c>k</c> に置換、加槓追加牌を <c>k</c> の直後に挿入</item>
/// </list>
/// </remarks>
public static class TenhouMeldStringEncoder
{
    /// <summary>
    /// <paramref name="call"/> を副露文字列に変換する
    /// </summary>
    /// <param name="call">副露</param>
    /// <param name="callerIndex">副露した側のプレイヤー index</param>
    /// <param name="rules">対局ルール (赤ドラ判定用)</param>
    public static string Encode(Call call, PlayerIndex callerIndex, GameRules rules)
    {
        return call.Type switch
        {
            CallType.Chi => EncodeChi(call, rules),
            CallType.Pon => EncodePon(call, callerIndex, rules),
            CallType.Daiminkan => EncodeDaiminkan(call, callerIndex, rules),
            CallType.Ankan => EncodeAnkan(call, rules),
            CallType.Kakan => EncodeKakan(call, callerIndex, rules),
            _ => throw new ArgumentException($"不明な副露種別:{call.Type}", nameof(call)),
        };
    }

    private static string EncodeChi(Call call, GameRules rules)
    {
        // Tile は record のため ReferenceEquals では同値判定できない。
        // チーは 3 枚で calledTile と同 Id は 1 枚だけのはずなので、Id でフィルタして残る 2 枚を手牌部とする
        var calledTile = call.CalledTile ?? throw new InvalidOperationException("Chi は CalledTile が必須です。");
        var handTiles = call.Tiles.Where(x => x.Id != calledTile.Id).ToList();
        if (handTiles.Count != 2)
        {
            throw new InvalidOperationException("Chi の calledTile が Tiles に 1 枚だけ含まれている必要があります。");
        }
        handTiles.Sort((x, y) => x.Id.CompareTo(y.Id));
        var sb = new StringBuilder();
        sb.Append('c');
        sb.Append(TenhouTileNumber.Convert(calledTile, rules));
        sb.Append(TenhouTileNumber.Convert(handTiles[0], rules));
        sb.Append(TenhouTileNumber.Convert(handTiles[1], rules));
        return sb.ToString();
    }

    private static string EncodePon(Call call, PlayerIndex callerIndex, GameRules rules)
    {
        var calledTile = call.CalledTile ?? throw new InvalidOperationException("Pon は CalledTile が必須です。");
        var handTiles = RemoveOneById(call.Tiles, calledTile.Id);
        handTiles.Sort((x, y) => x.Id.CompareTo(y.Id));
        var called = TenhouTileNumber.Convert(calledTile, rules);
        var t1 = TenhouTileNumber.Convert(handTiles[0], rules);
        var t2 = TenhouTileNumber.Convert(handTiles[1], rules);
        return FromRel(call.From, callerIndex) switch
        {
            3 => $"p{called}{t1}{t2}",   // 上家
            2 => $"{t1}p{called}{t2}",   // 対面
            1 => $"{t1}{t2}p{called}",   // 下家
            _ => throw new InvalidOperationException("Pon で鳴き元が自身になることはありません。"),
        };
    }

    private static string EncodeDaiminkan(Call call, PlayerIndex callerIndex, GameRules rules)
    {
        var calledTile = call.CalledTile ?? throw new InvalidOperationException("Daiminkan は CalledTile が必須です。");
        var handTiles = RemoveOneById(call.Tiles, calledTile.Id);
        handTiles.Sort((x, y) => x.Id.CompareTo(y.Id));
        var called = TenhouTileNumber.Convert(calledTile, rules);
        var t1 = TenhouTileNumber.Convert(handTiles[0], rules);
        var t2 = TenhouTileNumber.Convert(handTiles[1], rules);
        var t3 = TenhouTileNumber.Convert(handTiles[2], rules);
        return FromRel(call.From, callerIndex) switch
        {
            3 => $"m{called}{t1}{t2}{t3}",   // 上家
            2 => $"{t1}m{called}{t2}{t3}",   // 対面
            1 => $"{t1}{t2}{t3}m{called}",   // 下家 (天鳳実装は末尾寄せ)
            _ => throw new InvalidOperationException("Daiminkan で鳴き元が自身になることはありません。"),
        };
    }

    private static string EncodeAnkan(Call call, GameRules rules)
    {
        // 天鳳暗槓表記 "{b}{b}{b}a{t}": b=通常牌、t=赤ドラ枚数を 1 枚だけ末尾 (tiles[3]) に寄せる。
        // 赤ドラが含まれなければ tiles[3] も通常牌として扱う。
        // 前提: RedDoraTiles は同一種につき 1 枚のみ。暗槓 1 組に赤が 2 枚以上含まれる構成はルール上生じないため、
        // 2 枚以上のケースは例外で検知する (黙って誤出力するより早期に気づける)
        var tiles = call.Tiles.OrderBy(x => rules.RedDoraTiles.Contains(x) ? 1 : 0).ThenBy(x => x.Id).ToList();
        var redCount = tiles.Count(rules.RedDoraTiles.Contains);
        if (redCount > 1)
        {
            throw new InvalidOperationException($"暗槓 1 組に赤ドラが複数枚含まれる構成は未対応です。redCount:{redCount}");
        }
        var b = TenhouTileNumber.Convert(tiles[0], rules);
        var t = TenhouTileNumber.Convert(tiles[3], rules);
        return $"{b}{b}{b}a{t}";
    }

    private static string EncodeKakan(Call call, PlayerIndex callerIndex, GameRules rules)
    {
        // Kakan Tiles は Round.Kakan の仕様で [元ポン 3 枚, 加槓追加牌] の順
        var calledTile = call.CalledTile ?? throw new InvalidOperationException("Kakan は CalledTile (元ポンの鳴き牌) が必須です。");
        var addedTile = call.Tiles[^1];
        var ponTiles = call.Tiles.Take(call.Tiles.Count - 1).ToList();
        var handTiles = RemoveOneById(ponTiles, calledTile.Id);
        handTiles.Sort((x, y) => x.Id.CompareTo(y.Id));
        var called = TenhouTileNumber.Convert(calledTile, rules);
        var added = TenhouTileNumber.Convert(addedTile, rules);
        var t1 = TenhouTileNumber.Convert(handTiles[0], rules);
        var t2 = TenhouTileNumber.Convert(handTiles[1], rules);
        return FromRel(call.From, callerIndex) switch
        {
            3 => $"k{called}{added}{t1}{t2}",  // 上家から (元ポン)
            2 => $"{t1}k{called}{added}{t2}",  // 対面から
            1 => $"{t1}{t2}k{called}{added}",  // 下家から
            _ => throw new InvalidOperationException("Kakan の元ポンで鳴き元が自身になることはありません。"),
        };
    }

    private static List<Tile> RemoveOneById(IEnumerable<Tile> tiles, int idToRemove)
    {
        var result = new List<Tile>();
        var removed = false;
        foreach (var tile in tiles)
        {
            if (!removed && tile.Id == idToRemove)
            {
                removed = true;
                continue;
            }
            result.Add(tile);
        }
        if (!removed)
        {
            throw new InvalidOperationException($"指定 Id の牌が見つかりません。id:{idToRemove}");
        }
        return result;
    }

    private static int FromRel(PlayerIndex fromIndex, PlayerIndex callerIndex)
    {
        return (fromIndex.Value - callerIndex.Value + PlayerIndex.PLAYER_COUNT) % PlayerIndex.PLAYER_COUNT;
    }
}
