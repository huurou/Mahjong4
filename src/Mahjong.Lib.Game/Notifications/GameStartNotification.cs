using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 対局開始通知
/// </summary>
/// <param name="PlayerList">プレイヤー席のリスト</param>
/// <param name="Rules">対局ルール</param>
/// <param name="RecipientIndex">通知先プレイヤーのインデックス</param>
public record GameStartNotification(
    PlayerList PlayerList,
    GameRules Rules,
    PlayerIndex RecipientIndex
) : GameNotification;
