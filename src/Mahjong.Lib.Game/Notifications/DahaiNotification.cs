using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 打牌通知
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="DiscardedTile">打牌された牌</param>
/// <param name="DiscarderIndex">打牌者</param>
/// <param name="CandidateList">合法応答候補 (チー/ポン/大明槓/ロン/OK)</param>
public record DahaiNotification(
    PlayerRoundView View,
    Tile DiscardedTile,
    PlayerIndex DiscarderIndex,
    CandidateList CandidateList
) : RoundNotification(View);
