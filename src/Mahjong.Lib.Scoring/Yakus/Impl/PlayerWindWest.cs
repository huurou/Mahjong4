using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 自風牌・西
/// </summary>
public record PlayerWindWest : Yaku
{
    public override int Number => 12;
    public override string Name => "自風 西";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal PlayerWindWest() { }

    public static bool Valid(Hand hand, CallList callList, WinSituation winSituation)
    {
        return winSituation.PlayerWind == Wind.West &&
            hand.CombineFuuro(callList).IncludesKoutsu(TileKind.Sha);
    }
}
