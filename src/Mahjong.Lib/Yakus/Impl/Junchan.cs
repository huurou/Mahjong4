using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 純全帯么九
/// </summary>
public record Junchan : Yaku
{
    public override int Number => 33;
    public override string Name => "純全帯么九";
    public override int HanOpen => 2;
    public override int HanClosed => 3;
    public override bool IsYakuman => false;

    internal Junchan() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        var combined = hand.CombineFuuro(callList);
        var shuntsus = combined.Count(x => x.IsShuntsu);
        var routous = combined.Count(x => x.Any(y => y.IsRoutou));
        return shuntsus != 0 && routous == 5;
    }
}
