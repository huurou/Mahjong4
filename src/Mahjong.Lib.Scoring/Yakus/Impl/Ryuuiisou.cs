using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 緑一色
/// </summary>
public record Ryuuiisou : Yaku
{
    public override int Number => 43;
    public override string Name => "緑一色";
    public override int HanOpen => 13;
    public override int HanClosed => 13;
    public override bool IsYakuman => true;

    internal Ryuuiisou() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        var greens = new TileKindList(sou: "23468", honor: "r");
        return hand.CombineFuuro(callList).SelectMany(x => x).All(x => greens.Contains(x));
    }
}
