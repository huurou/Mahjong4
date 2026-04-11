using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 大四喜
/// </summary>
public record Daisuushii : Yaku
{
    public override int Number => 49;
    public override string Name => "大四喜";
    public override int HanOpen => 13;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Daisuushii() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        var combined = hand.CombineFuuro(callList);
        return combined.Count(x => (x.IsKoutsu || x.IsKantsu) && x.IsAllWind) == 4;
    }
}
