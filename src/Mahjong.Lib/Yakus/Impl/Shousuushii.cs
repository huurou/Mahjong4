using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 小四喜
/// </summary>
public record Shousuushii : Yaku
{
    public override int Number => 50;
    public override string Name => "小四喜";
    public override int HanOpen => 13;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Shousuushii() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        var combined = hand.CombineFuuro(callList);
        return combined.Where(x => x.IsKoutsu || x.IsKantsu).Count(x => x[0].IsWind) == 3 &&
            combined.Where(x => x.IsToitsu).Count(x => x[0].IsWind) == 1;
    }
}
