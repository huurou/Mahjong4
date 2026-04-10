using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 中
/// </summary>
public record Chun : Yaku
{
    public override int Number => 20;
    public override string Name => "中";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Chun() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        return hand.CombineFuuro(callList).IncludesKoutsu(TileKind.Chun);
    }
}
