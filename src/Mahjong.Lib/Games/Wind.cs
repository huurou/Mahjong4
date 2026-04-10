using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Games;

/// <summary>
/// 風を表現する列挙型
/// </summary>
public enum Wind
{
    /// <summary>
    /// 東
    /// </summary>
    East,
    /// <summary>
    /// 南
    /// </summary>
    South,
    /// <summary>
    /// 西
    /// </summary>
    West,
    /// <summary>
    /// 北
    /// </summary>
    North,
}

/// <summary>
/// Windの拡張メソッド
/// </summary>
public static class WindExtensions
{
    public static TileKind ToTileKind(this Wind wind)
    {
        return wind switch
        {
            Wind.East => TileKind.Ton,
            Wind.South => TileKind.Nan,
            Wind.West => TileKind.Sha,
            Wind.North => TileKind.Pei,
            _ => throw new ArgumentOutOfRangeException(nameof(wind), wind, null)
        };
    }
}
