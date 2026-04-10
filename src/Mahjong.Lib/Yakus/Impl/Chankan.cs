using Mahjong.Lib.Games;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 槍槓
/// </summary>
public record Chankan : Yaku
{
    public override int Number => 3;
    public override string Name => "槍槓";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Chankan() { }

    public static bool Valid(WinSituation winSituation)
    {
        return winSituation.IsChankan;
    }
}
