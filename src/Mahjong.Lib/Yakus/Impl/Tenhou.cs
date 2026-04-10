using Mahjong.Lib.Games;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 天和
/// </summary>
public record Tenhou : Yaku
{
    public override int Number => 37;
    public override string Name => "天和";
    public override int HanOpen => 0;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Tenhou() { }

    public static bool Valid(WinSituation winSituation)
    {
        return winSituation.IsTenhou;
    }
}
