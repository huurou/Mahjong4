using Mahjong.Lib.Game.Notifications.Bodies;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// プレイヤー応答 Wire DTO (シリアライズ/通信用の共通エンベロープ)
/// </summary>
/// <param name="NotificationId">対応する通知Id</param>
/// <param name="RoundRevision">対応する局Revision</param>
/// <param name="PlayerIndex">応答者 (なりすまし防止のため必須)</param>
/// <param name="Body">応答内容</param>
public record PlayerResponseEnvelope(
    NotificationId NotificationId,
    int RoundRevision,
    PlayerIndex PlayerIndex,
    ResponseBody Body
);
