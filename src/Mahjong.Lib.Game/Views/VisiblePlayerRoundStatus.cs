using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Scoring.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Views;

/// <summary>
/// 他家にも見える局内状態 公開情報のみを含む
/// </summary>
/// <param name="PlayerIndex">対象プレイヤー</param>
/// <param name="IsRiichi">立直中か</param>
/// <param name="IsDoubleRiichi">ダブル立直か</param>
/// <param name="IsMenzen">門前か</param>
/// <param name="SafeKindsAgainstRiichi">このプレイヤーが立直者のとき、「他家視点でこのプレイヤーのアタリにならない牌種集合」。立直前は null。副露で河から消えた牌も保持される</param>
public record VisiblePlayerRoundStatus(
    PlayerIndex PlayerIndex,
    bool IsRiichi,
    bool IsDoubleRiichi,
    bool IsMenzen,
    ImmutableHashSet<TileKind>? SafeKindsAgainstRiichi
);
