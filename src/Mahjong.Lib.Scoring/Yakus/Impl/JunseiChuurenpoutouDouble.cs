using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 純正九蓮宝燈ダブル役満
/// </summary>
public record JunseiChuurenpoutouDouble : Yaku
{
    public override int Number => 46;
    public override string Name => "純正九蓮宝燈";
    public override int HanOpen => 0;
    public override int HanClosed => 26;
    public override bool IsYakuman => true;

    internal JunseiChuurenpoutouDouble() { }

    public static bool Valid(Hand hand, TileKind winTileKind, GameRules gameRules)
    {
        return gameRules.DoubleYakumanEnabled && JunseiChuurenpoutou.Valid(hand, winTileKind);
    }
}
