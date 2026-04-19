using System.Text.Json.Serialization;

namespace Mahjong.Lib.Game.Notifications.Bodies;

/// <summary>
/// Wire DTO 応答本体の基底型 (discriminated union 相当)
/// </summary>
[JsonDerivedType(typeof(OkResponseBody), nameof(OkResponseBody))]
[JsonDerivedType(typeof(DahaiResponseBody), nameof(DahaiResponseBody))]
[JsonDerivedType(typeof(CallResponseBody), nameof(CallResponseBody))]
[JsonDerivedType(typeof(KanResponseBody), nameof(KanResponseBody))]
[JsonDerivedType(typeof(WinResponseBody), nameof(WinResponseBody))]
[JsonDerivedType(typeof(RyuukyokuResponseBody), nameof(RyuukyokuResponseBody))]
public abstract record ResponseBody;
