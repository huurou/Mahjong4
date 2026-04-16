using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 局の状態が返す「何を誰に聞くか」の仕様
/// RoundState が生成し RoundManager が通知・応答に変換する
/// </summary>
/// <param name="Phase">意思決定フェーズ</param>
/// <param name="PlayerSpecs">各プレイヤーへの決定仕様</param>
public record RoundDecisionSpec(
    RoundDecisionPhase Phase,
    ImmutableList<PlayerDecisionSpec> PlayerSpecs
);
