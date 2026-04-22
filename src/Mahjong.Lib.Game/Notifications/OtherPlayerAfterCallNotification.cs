using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Notifications;

/// <summary>
/// 他家副露後打牌要求通知 (非手番プレイヤー用)。
/// 副露者が打牌選択中であることを他家へ伝える観測通知。応答は OK のみ。
/// </summary>
/// <param name="View">プレイヤー視点フィルタ済み卓情報</param>
/// <param name="CallerIndex">副露した (= 打牌選択中の) プレイヤー</param>
/// <param name="InquiredPlayerIndices">問い合わせ対象プレイヤー (副露者 1 人)。
/// 本通知は非副露者への配信用なので受信者自身は問い合わせ対象外だが、副露者は
/// <see cref="AfterCallNotification"/> 側で問い合わせ対象として含まれる</param>
public record OtherPlayerAfterCallNotification(
    PlayerRoundView View,
    PlayerIndex CallerIndex,
    ImmutableArray<PlayerIndex> InquiredPlayerIndices
) : RoundNotification(View, InquiredPlayerIndices);
