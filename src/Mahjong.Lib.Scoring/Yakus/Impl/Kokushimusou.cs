using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 国士無双
/// </summary>
public record Kokushimusou : Yaku
{
    public override int Number => 47;
    public override string Name => "国士無双";
    public override int HanOpen => 0;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Kokushimusou() { }

    public static bool Valid(TileKindList tileKindList)
    {
        var tiles = tileKindList.ToList();
        foreach (var yaochuu in TileKind.Yaochus)
        {
            if (!tiles.Remove(yaochuu)) { return false; }
        }
        return tiles.Count == 1 && tiles[0].IsYaochu;
    }
}
