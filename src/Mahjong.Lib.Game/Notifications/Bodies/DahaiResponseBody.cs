using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Notifications.Bodies;

/// <summary>
/// 打牌応答本体
/// </summary>
/// <param name="Tile">打牌する牌</param>
/// <param name="IsRiichi">立直宣言するか</param>
public record DahaiResponseBody(Tile Tile, bool IsRiichi = false) : ResponseBody;
