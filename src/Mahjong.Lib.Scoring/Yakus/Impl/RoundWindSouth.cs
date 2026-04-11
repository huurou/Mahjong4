using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 場風牌・南
/// </summary>
public record RoundWindSouth : Yaku
{
    public override int Number => 15;
    public override string Name => "場風牌・南";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal RoundWindSouth() { }

    public static bool Valid(Hand hand, CallList callList, WinSituation winSituation)
    {
        return winSituation.RoundWind == Wind.South &&
            hand.CombineFuuro(callList).IncludesKoutsu(TileKind.Nan);
    }
}
