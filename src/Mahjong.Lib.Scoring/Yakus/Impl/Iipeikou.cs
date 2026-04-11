using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 一盃口
/// </summary>
public record Iipeikou : Yaku
{
    public override int Number => 9;
    public override string Name => "一盃口";
    public override int HanOpen => 0;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Iipeikou() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        return !callList.HasOpen && hand.Where(x => x.IsShuntsu).GroupBy(x => x).Any(x => x.Count() >= 2);
    }
}
