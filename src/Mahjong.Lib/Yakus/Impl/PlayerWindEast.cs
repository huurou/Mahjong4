using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 自風牌・東
/// </summary>
public record PlayerWindEast : Yaku
{
    public override int Number => 10;
    public override string Name => "自風牌・東";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal PlayerWindEast() { }

    public static bool Valid(Hand hand, CallList callList, WinSituation winSituation)
    {
        return winSituation.PlayerWind == Wind.East && 
            hand.CombineFuuro(callList).IncludesKoutsu(TileKind.Ton);
    }
}
