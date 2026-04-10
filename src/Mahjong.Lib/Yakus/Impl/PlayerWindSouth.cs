using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 自風牌・南
/// </summary>
public record PlayerWindSouth : Yaku
{
    public override int Number => 11;
    public override string Name => "自風牌・南";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal PlayerWindSouth() { }

    public static bool Valid(Hand hand, CallList callList, WinSituation winSituation)
    {
        return winSituation.PlayerWind == Wind.South &&
            hand.CombineFuuro(callList).IncludesKoutsu(TileKind.Nan);
    }
}
