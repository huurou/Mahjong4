using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 他家ツモ通知 (非手番プレイヤー用)
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="TsumoPlayerIndex">ツモしたプレイヤー</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー (ツモした手番プレイヤー 1 人)。
/// 本通知は非手番プレイヤーへの配信用なので受信者自身は問い合わせ対象外だが、手番 (= ツモした本人) は
/// <see cref="TsumoNotification"/> 側で問い合わせ対象として含まれる。全プレイヤー通知で同じ値を共有し
/// 「誰の判断待ちか」をクライアントから識別可能にするため、ここにも手番インデックスを載せる</param>
public record OtherPlayerTsumoNotification(
    PlayerRoundView View,
    PlayerIndex TsumoPlayerIndex,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices
) : RoundNotification(View, InquiredPlayerIndices);
