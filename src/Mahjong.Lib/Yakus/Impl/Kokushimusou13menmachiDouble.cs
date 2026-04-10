using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 国士無双十三面待ちダブル役満
/// </summary>
public record Kokushimusou13menmachiDouble : Yaku
{
    public override int Number => 48;
    public override string Name => "国士無双十三面待ち";
    public override int HanOpen => 0;
    public override int HanClosed => 26;
    public override bool IsYakuman => true;

    internal Kokushimusou13menmachiDouble() { }

    public static bool Valid(TileKindList tileKindList, TileKind winTileKind, GameRules gameRules)
    {
        return gameRules.DoubleYakumanEnabled && Kokushimusou13menmachi.Valid(tileKindList, winTileKind);
    }
}
