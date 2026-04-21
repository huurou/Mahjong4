using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 場風牌・西
/// </summary>
public record RoundWindWest : Yaku
{
    public override int Number => 16;
    public override string Name => "場風 西";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal RoundWindWest() { }

    public static bool Valid(Hand hand, CallList callList, WinSituation winSituation)
    {
        return winSituation.RoundWind == Wind.West &&
            hand.CombineFuuro(callList).IncludesKoutsu(TileKind.Sha);
    }
}
