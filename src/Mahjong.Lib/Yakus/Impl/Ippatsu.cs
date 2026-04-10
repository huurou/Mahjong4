using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 一発
/// </summary>
public record Ippatsu : Yaku
{
    public override int Number => 2;
    public override string Name => "一発";
    public override int HanOpen => 0;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Ippatsu() { }

    public static bool Valid(WinSituation winSituation, CallList callList)
    {
        return (Riichi.Valid(winSituation, callList) || DoubleRiichi.Valid(winSituation, callList)) &&
            winSituation.IsIppatsu;
    }
}
