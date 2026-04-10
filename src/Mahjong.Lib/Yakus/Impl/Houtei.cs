using Mahjong.Lib.Games;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 河底撈魚
/// </summary>
public record Houtei : Yaku
{
    public override int Number => 6;
    public override string Name => "河底撈魚";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Houtei() { }

    public static bool Valid(WinSituation winSituation)
    {
        return winSituation.IsHoutei;
    }
}
