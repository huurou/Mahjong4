using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Games.Scoring;

/// <summary>
/// 点数計算リクエスト
/// </summary>
/// <param name="Round">和了発生時の局状態</param>
/// <param name="WinnerIndex">和了者</param>
/// <param name="LoserIndex">放銃者 ロン/槍槓では打牌者/加槓宣言者、ツモ/嶺上では <paramref name="WinnerIndex"/> と同値 (WinType で判別する)</param>
/// <param name="WinType">和了種別</param>
/// <param name="WinTile">和了牌 (Ron=放銃牌 / Chankan=加槓追加牌 / Tsumo・Rinshan=ツモ牌)</param>
public record ScoreRequest(
    Round Round,
    PlayerIndex WinnerIndex,
    PlayerIndex LoserIndex,
    WinType WinType,
    Tile WinTile
);
