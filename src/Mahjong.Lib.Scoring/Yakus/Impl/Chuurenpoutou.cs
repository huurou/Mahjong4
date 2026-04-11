using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 九蓮宝燈
/// </summary>
public record Chuurenpoutou : Yaku
{
    public override int Number => 45;
    public override string Name => "九蓮宝燈";
    public override int HanOpen => 0;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Chuurenpoutou() { }

    public static bool Valid(Hand hand)
    {
        if (!new TileKindList(hand.SelectMany(x => x)).IsAllSameSuit) { return false; }
        var values = hand.SelectMany(x => x.Where(y => y.IsNumber), (_, x) => x.Number).ToList();
        // valuesは 1112345678999+1～9のいずれかになっているはず
        foreach (var n in new[] { 1, 1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 9, 9 })
        {
            if (!values.Remove(n)) { return false; }
        }
        return values.Count == 1;
    }
}
