using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 場風牌・北
/// </summary>
public record RoundWindNorth : Yaku
{
    public override int Number => 17;
    public override string Name => "場風 北";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal RoundWindNorth() { }

    public static bool Valid(Hand hand, CallList callList, WinSituation winSituation)
    {
        return winSituation.RoundWind == Wind.North &&
            hand.CombineFuuro(callList).IncludesKoutsu(TileKind.Pei);
    }
}
