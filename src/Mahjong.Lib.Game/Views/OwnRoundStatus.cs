namespace Mahjong.Lib.Game.Views;

/// <summary>
/// 自分だけが見られる局内状態 フリテン等の非公開情報を含む
/// </summary>
/// <param name="IsRiichi">立直中か</param>
/// <param name="IsDoubleRiichi">ダブル立直か</param>
/// <param name="IsIppatsu">一発可能か</param>
/// <param name="IsMenzen">門前か</param>
/// <param name="IsFuriten">永久フリテンか</param>
/// <param name="IsTemporaryFuriten">同巡フリテンか</param>
/// <param name="IsNagashiMangan">流し満貫資格があるか</param>
public record OwnRoundStatus(
    bool IsRiichi,
    bool IsDoubleRiichi,
    bool IsIppatsu,
    bool IsMenzen,
    bool IsFuriten,
    bool IsTemporaryFuriten,
    bool IsNagashiMangan
);
