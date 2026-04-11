using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 四暗刻単騎待ちダブル役満
/// </summary>
public record SuuankouTankiDouble : Yaku
{
    public override int Number => 41;
    public override string Name => "四暗刻単騎待ち";
    public override int HanOpen => 0;
    public override int HanClosed => 26;
    public override bool IsYakuman => true;

    internal SuuankouTankiDouble() { }

    public static bool Valid(Hand hand, TileKindList winGroup, TileKind winTileKind, CallList callList, WinSituation winSituation, GameRules gameRules)
    {
        return gameRules.DoubleYakumanEnabled && SuuankouTanki.Valid(hand, winGroup, winTileKind, callList, winSituation);
    }
}
