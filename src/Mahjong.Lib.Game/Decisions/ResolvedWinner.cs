using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 個別和了者の情報
/// </summary>
/// <param name="PlayerIndex">和了者</param>
/// <param name="WinTile">和了牌</param>
/// <param name="ScoreResult">点数計算結果 (翻/符/点数移動/役情報)</param>
public record ResolvedWinner(
    PlayerIndex PlayerIndex,
    Tile WinTile,
    ScoreResult ScoreResult
);
