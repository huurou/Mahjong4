using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// ツモ
/// </summary>
public record Tsumo : Yaku
{
    public override int Number => 0;
    public override string Name => "門前清自摸和";
    public override int HanOpen => 0;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Tsumo() { }

    public static bool Valid(CallList callList, WinSituation winSituation)
    {
        return !callList.HasOpen && winSituation.IsTsumo;
    }
}
