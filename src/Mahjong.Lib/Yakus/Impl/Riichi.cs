using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 立直
/// </summary>
public record Riichi : Yaku
{
    public override int Number => 1;
    public override string Name => "立直";
    public override int HanOpen => 0;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Riichi() { }

    public static bool Valid(WinSituation winSituation, CallList callList)
    {
        return winSituation.IsRiichi && !winSituation.IsDoubleRiichi && !callList.HasOpen;
    }
}
