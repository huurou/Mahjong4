using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 他家嶺上ツモ通知 (非手番プレイヤー用)
/// 嶺上ツモ牌そのものは手番プレイヤー固有の私的情報のため本通知には含まない
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="KanTsumoPlayerIndex">嶺上ツモしたプレイヤー</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー (嶺上ツモした手番プレイヤー 1 人)。
/// 本通知は非手番プレイヤーへの配信用なので受信者自身は問い合わせ対象外だが、手番 (= 嶺上ツモした本人) は
/// <see cref="KanTsumoNotification"/> 側で問い合わせ対象として含まれる。全プレイヤー通知で同じ値を共有し
/// 「誰の判断待ちか」をクライアントから識別可能にするため、ここにも手番インデックスを載せる</param>
public record OtherPlayerKanTsumoNotification(
    PlayerRoundView View,
    PlayerIndex KanTsumoPlayerIndex,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices
) : RoundNotification(View, InquiredPlayerIndices);
