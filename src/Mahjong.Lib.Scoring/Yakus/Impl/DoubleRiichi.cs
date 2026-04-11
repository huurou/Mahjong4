using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// ダブル立直
/// </summary>
public record DoubleRiichi : Yaku
{
    public override int Number => 21;
    public override string Name => "ダブル立直";
    public override int HanOpen => 0;
    public override int HanClosed => 2;
    public override bool IsYakuman => false;

    internal DoubleRiichi() { }

    public static bool Valid(WinSituation winSituation, CallList callList)
    {
        return winSituation.IsDoubleRiichi && !callList.HasOpen;
    }
}
