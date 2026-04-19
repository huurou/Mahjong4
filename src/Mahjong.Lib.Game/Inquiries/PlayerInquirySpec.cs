using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Inquiries;

/// <summary>
/// 個別プレイヤーへの問い合わせ仕様 (対象プレイヤーと選択可能な応答候補)
/// </summary>
/// <param name="PlayerIndex">対象プレイヤー</param>
/// <param name="CandidateList">選択可能な応答候補</param>
public record PlayerInquirySpec(
    PlayerIndex PlayerIndex,
    CandidateList CandidateList
);
