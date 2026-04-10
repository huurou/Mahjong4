using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 字一色
/// </summary>
public record Tsuuiisou : Yaku
{
    public override int Number => 42;
    public override string Name => "字一色";
    public override int HanOpen => 13;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Tsuuiisou() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        return hand.CombineFuuro(callList).SelectMany(x => x).All(x => x.IsHonor);
    }
}
