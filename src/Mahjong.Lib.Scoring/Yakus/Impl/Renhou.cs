using Mahjong.Lib.Scoring.Games;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 人和 満貫扱いの人和
/// </summary>
public record Renhou : Yaku
{
    /// <summary>
    /// 天鳳にはなかった
    /// </summary>
    public override int Number => 36;
    public override string Name => "人和";
    public override int HanOpen => 0;
    public override int HanClosed => 5;
    public override bool IsYakuman => false;

    internal Renhou() { }

    public static bool Valid(WinSituation winSituation, GameRules gameRules)
    {
        return winSituation.IsRenhou && !gameRules.RenhouAsYakumanEnabled;
    }
}
