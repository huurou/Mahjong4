using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 混老頭
/// </summary>
public record Honroutou : Yaku
{
    public override int Number => 31;
    public override string Name => "混老頭";
    public override int HanOpen => 2;
    public override int HanClosed => 2;
    public override bool IsYakuman => false;

    internal Honroutou() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        return hand.CombineFuuro(callList).All(x => x.All(y => y.IsYaochu));
    }
}
