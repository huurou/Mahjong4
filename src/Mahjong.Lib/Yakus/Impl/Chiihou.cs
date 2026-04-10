using Mahjong.Lib.Games;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 地和
/// </summary>
public record Chiihou : Yaku
{
    public override int Number => 38;
    public override string Name => "地和";
    public override int HanOpen => 0;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Chiihou() { }

    public static bool Valid(WinSituation winSituation)
    {
        return winSituation.IsChiihou;
    }
}
