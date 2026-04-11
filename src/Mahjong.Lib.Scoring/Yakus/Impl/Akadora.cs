using Mahjong.Lib.Scoring.Games;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 赤ドラ
/// </summary>
public record Akadora : Yaku
{
    public override int Number => 54;
    public override string Name => "赤ドラ";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Akadora() { }

    public static bool Valid(WinSituation winSituation)
    {
        return winSituation.AkadoraCount > 0;
    }
}
