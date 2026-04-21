using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Scoring.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds;

/// <summary>
/// 局内でのプレイヤーごとの状態
/// 合法候補列挙・和了判定・流局精算で参照する
/// </summary>
/// <param name="IsMenzen">門前 (鳴きをしていない)</param>
/// <param name="IsRiichi">立直宣言済み</param>
/// <param name="IsDoubleRiichi">ダブル立直 (第一打で立直)</param>
/// <param name="IsIppatsu">一発可否 (立直宣言直後かつ鳴きが入っていない、自分の次のツモ時に消える)</param>
/// <param name="IsFirstTurnBeforeDiscard">第一打前 (天和・地和・人和判定用、鳴きが入ると全員 false)</param>
/// <param name="IsRinshan">嶺上ツモ直後 (次打牌まで true)</param>
/// <param name="IsNagashiMangan">流し満貫条件 (鳴かれた or 幺九牌以外を捨てると false)</param>
/// <param name="IsTemporaryFuriten">同巡フリテン (Phase 5 のロン見逃し検出で駆動、Phase 2 では器のみ)</param>
/// <param name="IsFuriten">フリテン (自分の待ち牌が自分の河もしくは鳴かれた牌にある状態。ツモ時に再評価)</param>
/// <param name="IsPendingRiichi">立直宣言を保留中 (打牌〜ロン応答待ちの間に true。全員が和了応答でなければ確定、ロン応答が来れば不成立)</param>
/// <param name="IsPendingRiichiDouble">保留中の立直がダブル立直として確定するか (打牌時点のスナップショット。打牌後は IsFirstTurnBeforeDiscard が落ちるため、ダブリー判定は保留時に確定させる)</param>
/// <param name="TilesCalledFromRiver">自分の打牌のうち鳴かれた牌。フリテン判定で河と合わせて見る</param>
/// <param name="SafeKindsAgainstRiichi">自分が立直宣言者のとき、「他家視点で自分のアタリにならない牌種集合」。立直前は null、<see cref="Round.ConfirmRiichi"/> で自分の河全体の牌種で初期化し、以後 <see cref="Round.Dahai"/> で他家打牌の牌種を追加していく。副露で河から消えた牌も保持される (守備型 AI の現物判定に使用)</param>
public record PlayerRoundStatus(
    bool IsMenzen = true,
    bool IsRiichi = false,
    bool IsDoubleRiichi = false,
    bool IsIppatsu = false,
    bool IsFirstTurnBeforeDiscard = true,
    bool IsRinshan = false,
    bool IsNagashiMangan = true,
    bool IsTemporaryFuriten = false,
    bool IsFuriten = false,
    bool IsPendingRiichi = false,
    bool IsPendingRiichiDouble = false,
    ImmutableList<Tile>? TilesCalledFromRiver = null,
    ImmutableHashSet<TileKind>? SafeKindsAgainstRiichi = null
)
{
    /// <summary>
    /// 自分の打牌のうち鳴かれた牌。null を空リストへ正規化する
    /// </summary>
    public ImmutableList<Tile> TilesCalledFromRiver { get; init; } = TilesCalledFromRiver ?? [];
}
