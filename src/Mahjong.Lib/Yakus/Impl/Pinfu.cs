using Mahjong.Lib.Fus;
using Mahjong.Lib.Games;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 平和
/// </summary>
public record Pinfu : Yaku
{
    public override int Number => 7;
    public override string Name => "平和";
    public override int HanOpen => 0;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Pinfu() { }

    public static bool Valid(FuList fuList, WinSituation winSituation, GameRules gameRules)
    {
        if (winSituation.IsTsumo)
        {
            // ピンヅモありならツモでもツモ符はつかない
            // ピンヅモなしなら平和不成立
            return gameRules.PinzumoEnabled && fuList.Contains(Fu.Futei) && fuList.Count == 1;
        }
        else
        {
            // ロンなら副底と面前符だけ
            return fuList.Contains(Fu.Futei) && fuList.Contains(Fu.Menzen) && fuList.Count == 2;
        }
    }
}
