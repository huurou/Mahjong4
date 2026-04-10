using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 二盃口
/// </summary>
public record Ryanpeikou : Yaku
{
    public override int Number => 32;
    public override string Name => "二盃口";
    public override int HanOpen => 0;
    public override int HanClosed => 3;
    public override bool IsYakuman => false;

    internal Ryanpeikou() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        if (callList.HasOpen) { return false; }
        var shuntsus = hand.Where(x => x.IsShuntsu);
        var counts = shuntsus.Select(x => shuntsus.Count(x.Equals));
        return counts.Count(x => x >= 2) == 4;
    }
}
