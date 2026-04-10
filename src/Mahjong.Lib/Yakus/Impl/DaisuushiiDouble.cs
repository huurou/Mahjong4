using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;
/// <summary>
/// 大四喜ダブル
/// </summary>
public record DaisuushiiDouble : Yaku
{
    public override int Number => 49;
    public override string Name => "大四喜";
    public override int HanOpen => 26;
    public override int HanClosed => 26;
    public override bool IsYakuman => true;

    internal DaisuushiiDouble() { }

    public static bool Valid(Hand hand, CallList callList, GameRules gameRules)
    {
        return gameRules.DoubleYakumanEnabled && Daisuushii.Valid(hand, callList);
    }
}
