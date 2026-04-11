using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 三暗刻
/// </summary>
public record Sanankou : Yaku
{
    public override int Number => 29;
    public override string Name => "三暗刻";
    public override int HanOpen => 2;
    public override int HanClosed => 2;
    public override bool IsYakuman => false;

    internal Sanankou() { }

    public static bool Valid(Hand hand, TileKindList winGroup, CallList callList, WinSituation winSituation)
    {
        var ankoTiles = winSituation.IsTsumo ? hand.Where(x => x.IsKoutsu) : hand.Where(x => x.IsKoutsu && x != winGroup);
        var ankanTiles = callList.Where(x => x.IsAnkan).Select(x => x.TileKindList);
        return ankoTiles.Count() + ankanTiles.Count() == 3;
    }
}
