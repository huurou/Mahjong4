using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 自風牌・北
/// </summary>
public record PlayerWindNorth : Yaku
{
    public override int Number => 13;
    public override string Name => "自風牌・北";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal PlayerWindNorth() { }

    public static bool Valid(Hand hand, CallList callList, WinSituation winSituation)
    {
        return winSituation.PlayerWind == Wind.North &&
            hand.CombineFuuro(callList).IncludesKoutsu(TileKind.Pei);
    }
}
