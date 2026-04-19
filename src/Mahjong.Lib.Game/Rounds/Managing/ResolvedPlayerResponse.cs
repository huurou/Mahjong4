using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 優先順位適用後の採用応答 (誰の応答が採用されたか)
/// </summary>
/// <param name="PlayerIndex">応答者</param>
/// <param name="Response">採用された応答</param>
public record AdoptedPlayerResponse(PlayerIndex PlayerIndex, PlayerResponse Response);
