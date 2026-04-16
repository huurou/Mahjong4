using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Views;

/// <summary>
/// 他家にも見える局内状態 公開情報のみを含む
/// </summary>
/// <param name="PlayerIndex">対象プレイヤー</param>
/// <param name="IsRiichi">立直中か</param>
/// <param name="IsDoubleRiichi">ダブル立直か</param>
/// <param name="IsMenzen">門前か</param>
public record VisiblePlayerRoundStatus(
    PlayerIndex PlayerIndex,
    bool IsRiichi,
    bool IsDoubleRiichi,
    bool IsMenzen
);
