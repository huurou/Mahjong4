using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications.Payloads;

/// <summary>
/// 対局開始通知のペイロード
/// </summary>
/// <param name="PlayerList">プレイヤー席のリスト</param>
/// <param name="Rules">対局ルール</param>
public record GameStartNotificationPayload(
    PlayerList PlayerList,
    GameRules Rules
) : NotificationPayload;
