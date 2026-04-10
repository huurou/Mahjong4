using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 場風牌・東
/// </summary>
public record RoundWindEast : Yaku
{
    public override int Number => 14;
    public override string Name => "場風牌・東";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal RoundWindEast() { }

    public static bool Valid(Hand hand, CallList callList, WinSituation winSituation)
    {
        return winSituation.RoundWind == Wind.East &&
            hand.CombineFuuro(callList).IncludesKoutsu(TileKind.Ton);
    }
}
