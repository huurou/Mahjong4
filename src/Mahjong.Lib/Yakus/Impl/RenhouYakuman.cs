using Mahjong.Lib.Games;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 人和（役満）
/// </summary>
public record RenhouYakuman : Yaku
{
    public override int Number => 36;
    public override string Name => "人和";
    public override int HanOpen => 0;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal RenhouYakuman() { }

    public static bool Valid(WinSituation winSituation, GameRules gameRules)
    {
        return winSituation.IsRenhou && gameRules.RenhouAsYakumanEnabled;
    }
}
