using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 対々和
/// </summary>
public record Toitoihou : Yaku
{
    public override int Number => 28;
    public override string Name => "対々和";
    public override int HanOpen => 2;
    public override int HanClosed => 2;
    public override bool IsYakuman => false;

    internal Toitoihou() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        return hand.CombineFuuro(callList).Count(x => x.IsKoutsu || x.IsKantsu) == 4;
    }
}
