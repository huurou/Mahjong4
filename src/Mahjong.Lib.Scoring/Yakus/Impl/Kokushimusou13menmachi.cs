using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 国士無双十三面待ち
/// </summary>
public record Kokushimusou13menmachi : Yaku
{
    public override int Number => 48;
    public override string Name => "国士無双十三面待ち";
    public override int HanOpen => 0;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Kokushimusou13menmachi() { }

    public static bool Valid(TileKindList tileKindList, TileKind winTileKind)
    {
        return Kokushimusou.Valid(tileKindList) && tileKindList.Count(x => x == winTileKind) == 2;
    }
}
