using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 清一色
/// </summary>
public record Chinitsu : Yaku
{
    public override int Number => 35;
    public override string Name => "清一色";
    public override int HanOpen => 5;
    public override int HanClosed => 6;
    public override bool IsYakuman => false;

    internal Chinitsu() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        var combined = hand.CombineFuuro(callList);
        return new TileKindList(combined.SelectMany(x => x)).IsAllSameSuit;
    }
}
