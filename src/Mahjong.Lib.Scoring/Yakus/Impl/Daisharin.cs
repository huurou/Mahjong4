using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 大車輪
/// </summary>
public record Daisharin : Yaku
{
    public override int Number => 56;
    public override string Name => "大車輪";
    public override int HanOpen => 0;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Daisharin() { }
    public static bool Valid(Hand hand, GameRules gameRules)
    {
        return gameRules.DaisharinEnabled &&
            new TileKindListList(hand) == [new(pin: "22"), new(pin: "33"), new(pin: "44"), new(pin: "55"), new(pin: "66"), new(pin: "77"), new(pin: "88")];
    }
}
