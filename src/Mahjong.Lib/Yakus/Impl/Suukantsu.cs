using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 四槓子
/// </summary>
public record Suukantsu : Yaku
{
    public override int Number => 51;
    public override string Name => "四槓子";
    public override int HanOpen => 13;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Suukantsu() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        return hand.CombineFuuro(callList).Count(x => x.IsKantsu) == 4;
    }
}
