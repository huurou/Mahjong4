using Mahjong.Lib.Scoring.Games;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 海底摸月
/// </summary>
public record Haitei : Yaku
{
    public override int Number => 5;
    public override string Name => "海底摸月";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Haitei() { }

    public static bool Valid(WinSituation winSituation)
    {
        return winSituation.IsHaitei;
    }
}
