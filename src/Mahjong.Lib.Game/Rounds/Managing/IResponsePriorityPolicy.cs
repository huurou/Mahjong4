using Mahjong.Lib.Game.Decisions;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 応答集約時の優先順位解決ポリシー
/// </summary>
public interface IResponsePriorityPolicy
{
    /// <summary>
    /// フェーズ別に収集された応答群から優先順位を適用して採用応答を決定する
    /// ダブロン時は複数応答を返す (採用順)
    /// </summary>
    ImmutableArray<ResolvedPlayerResponse> Resolve(
        RoundDecisionSpec spec,
        ImmutableArray<ResolvedPlayerResponse> responses
    );
}
