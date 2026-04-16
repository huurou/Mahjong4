using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 個別プレイヤーへの決定仕様
/// </summary>
/// <param name="PlayerIndex">対象プレイヤー</param>
/// <param name="CandidateList">選択可能な応答候補</param>
public record PlayerDecisionSpec(
    PlayerIndex PlayerIndex,
    CandidateList CandidateList
);
