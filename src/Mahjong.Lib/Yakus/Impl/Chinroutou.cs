using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 清老頭
/// </summary>
public record Chinroutou : Yaku
{
    public override int Number => 44;
    public override string Name => "清老頭";
    public override int HanOpen => 13;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Chinroutou() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        return hand.CombineFuuro(callList).All(x => x.All(y => y.IsRoutou));
    }
}
