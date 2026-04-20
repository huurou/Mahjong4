using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Game.Scoring.Conversions;

internal static class TileKindConverter
{
    public static TileKind ToTileKind(this Tile tile)
    {
        return TileKind.All[tile.Kind];
    }

    public static TileKind FromKind(int kind)
    {
        return TileKind.All[kind];
    }

    public static TileKindList ToTileKindList(this IEnumerable<Tile> tiles)
    {
        return [.. tiles.Select(ToTileKind)];
    }
}
