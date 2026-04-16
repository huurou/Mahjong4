namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 通知種別 (Wire DTO 用)
/// </summary>
public enum NotificationType
{
    GameStart,
    RoundStart,
    Haipai,
    Tsumo,
    OtherPlayerTsumo,
    Dahai,
    Call,
    Kan,
    KanTsumo,
    DoraReveal,
    Win,
    Ryuukyoku,
    RoundEnd,
    GameEnd,
}
