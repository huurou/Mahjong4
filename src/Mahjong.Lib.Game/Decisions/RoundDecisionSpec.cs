using Mahjong.Lib.Game.Players;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 局の状態が返す「何を誰に聞くか」の仕様
/// RoundState が生成し RoundManager が通知・応答に変換する
/// </summary>
/// <param name="Phase">意思決定フェーズ</param>
/// <param name="PlayerSpecs">各プレイヤーへの決定仕様</param>
/// <param name="LoserIndex">放銃者候補 (打牌局面は打牌者 / 槓局面は加槓者 / それ以外は null)。
/// 優先順位ポリシーの並び順 (天鳳: 放銃者から見た上家優先) 判定に用いる</param>
public record RoundDecisionSpec(
    RoundDecisionPhase Phase,
    ImmutableList<PlayerDecisionSpec> PlayerSpecs,
    PlayerIndex? LoserIndex
);
