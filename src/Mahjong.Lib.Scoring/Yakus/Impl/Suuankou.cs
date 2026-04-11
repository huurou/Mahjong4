using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 四暗刻
/// </summary>
public record Suuankou : Yaku
{
    public override int Number => 40;
    public override string Name => "四暗刻";
    public override int HanOpen => 0;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Suuankou() { }

    public static bool Valid(Hand hand, TileKindList winGroup, CallList callList, WinSituation winSituation)
    {
        var ankoTiles = winSituation.IsTsumo ? hand.Where(x => x.IsKoutsu) : hand.Where(x => x.IsKoutsu && x != winGroup);
        var ankanTiles = callList.Where(x => x.IsAnkan).Select(x => x.TileKindList);
        return ankoTiles.Count() + ankanTiles.Count() == 4;
    }
}
