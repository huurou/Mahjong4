using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 四暗刻単騎待ち
/// </summary>
public record SuuankouTanki : Yaku
{
    public override int Number => 41;
    public override string Name => "四暗刻単騎待ち";
    public override int HanOpen => 0;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal SuuankouTanki() { }

    public static bool Valid(Hand hand, TileKindList winGroup, TileKind winTileKind, CallList callList, WinSituation winSituation)
    {
        var jantou = hand.Where(x => x.IsToitsu).First();
        return Suuankou.Valid(hand, winGroup, callList, winSituation) && jantou[0] == winTileKind;
    }
}
