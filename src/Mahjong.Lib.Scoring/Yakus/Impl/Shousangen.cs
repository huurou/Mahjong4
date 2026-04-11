using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 小三元
/// </summary>
public record Shousangen : Yaku
{
    public override int Number => 30;
    public override string Name => "小三元";
    public override int HanOpen => 2;
    public override int HanClosed => 2;
    public override bool IsYakuman => false;

    internal Shousangen() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        var toitsu = hand.FirstOrDefault(x => x.IsToitsu);
        return toitsu is not null && toitsu[0].IsDragon &&
            hand.Where(x => x.IsKoutsu && x[0].IsDragon)
                .Concat(callList.Where(x => (x.IsPon || x.IsKan) && x.TileKindList[0].IsDragon).Select(x => x.TileKindList))
                .Count() == 2;
    }
}
