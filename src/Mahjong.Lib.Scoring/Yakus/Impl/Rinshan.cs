using Mahjong.Lib.Scoring.Games;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 嶺上開花
/// </summary>
public record Rinshan : Yaku
{
    public override int Number => 4;
    public override string Name => "嶺上開花";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Rinshan() { }

    public static bool Valid(WinSituation winSituation)
    {
        return winSituation.IsRinshan;
    }
}
