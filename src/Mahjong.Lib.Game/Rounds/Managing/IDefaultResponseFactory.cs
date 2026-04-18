using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// プレイヤー応答タイムアウトまたは例外時に注入する既定応答を生成する
/// </summary>
public interface IDefaultResponseFactory
{
    /// <summary>
    /// フェーズと候補リストから安全な既定応答を生成する
    /// </summary>
    PlayerResponse CreateDefault(PlayerDecisionSpec spec, RoundDecisionPhase phase);
}
