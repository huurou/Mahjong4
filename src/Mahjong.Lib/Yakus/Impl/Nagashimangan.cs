using Mahjong.Lib.Games;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 流し満貫
/// </summary>
public record Nagashimangan : Yaku
{
    /// <summary>
    /// 天鳳にはなかった
    /// </summary>
    public override int Number => 55;
    public override string Name => "流し満貫";
    public override int HanOpen => 5;
    public override int HanClosed => 5;
    public override bool IsYakuman => false;

    internal Nagashimangan() { }

    public static bool Valid(WinSituation winSituation)
    {
        return winSituation.IsNagashimangan;
    }
}
