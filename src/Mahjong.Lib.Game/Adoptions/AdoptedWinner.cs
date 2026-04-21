using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Adoptions;

/// <summary>
/// 個別和了者の情報
/// </summary>
/// <param name="PlayerIndex">和了者</param>
/// <param name="WinTile">和了牌</param>
/// <param name="ScoreResult">点数計算結果 (翻/符/点数移動/役情報)</param>
/// <param name="PaoPlayerIndex">包 (責任払い) の責任者。包役 (大三元/大四喜/四槓子) が成立した場合のみ値を持ち、それ以外は null</param>
public record AdoptedWinner(
    PlayerIndex PlayerIndex,
    Tile WinTile,
    ScoreResult ScoreResult,
    PlayerIndex? PaoPlayerIndex = null
);
