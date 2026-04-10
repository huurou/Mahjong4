using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 純正九蓮宝燈
/// </summary>
public record JunseiChuurenpoutou : Yaku
{
    public override int Number => 46;
    public override string Name => "純正九蓮宝燈";
    public override int HanOpen => 0;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal JunseiChuurenpoutou() { }

    public static bool Valid(Hand hand, TileKind winTileKind)
    {
        if (!Chuurenpoutou.Valid(hand)) { return false; }
        var values = hand.SelectMany(x => x.Where(x => x.IsNumber), (_, x) => x.Number).ToList();
        // 純正九蓮宝燈は和了牌以外が1112345678999で和了牌が1～9のいずれかが条件
        if (values.Count(x => x == winTileKind.Number) is not 2 and not 4) { return false; }
        values.Remove(1);
        values.Remove(1);
        values.Remove(9);
        values.Remove(9);
        foreach (var n in Enumerable.Range(1, 9))
        {
            values.Remove(n);
        }
        return values.Count == 1;
    }
}
