using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.States.RoundStates;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// <see cref="RoundState"/> と <see cref="RoundInquirySpec"/> / <see cref="PlayerInquirySpec"/> から
/// 各プレイヤーへ送る <see cref="RoundNotification"/> を組み立てる抽象
/// </summary>
public interface IRoundNotificationBuilder
{
    /// <summary>
    /// 現在の状態と問い合わせ仕様に基づき、指定プレイヤーへ送る通知を組み立てる
    /// </summary>
    RoundNotification Build(
        RoundState state,
        Round round,
        RoundInquirySpec spec,
        PlayerInquirySpec playerSpec,
        IRoundViewProjector projector
    );
}
