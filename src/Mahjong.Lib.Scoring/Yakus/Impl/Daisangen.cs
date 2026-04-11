using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 大三元
/// </summary>
public record Daisangen : Yaku
{
    public override int Number => 39;
    public override string Name => "大三元";
    public override int HanOpen => 13;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Daisangen() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        var combined = hand.CombineFuuro(callList);
        return combined.Count(x => (x.IsKoutsu || x.IsKantsu) && x.All(x => x.IsDragon)) == 3;
    }
}
