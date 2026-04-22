using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 対局進行のイベントトレース
/// 観測用の副作用のみを持ち、通知・応答フローに影響を与えない
/// </summary>
public interface IGameTracer
{
    /// <summary>
    /// true の場合、<see cref="CompositeGameTracer"/> から呼び出されたときの例外を握り潰さず再 throw する。
    /// データ整合性が重要な tracer (牌譜の IO 出力など) では true にして fail-fast を強制する
    /// </summary>
    bool IsCritical => false;

    void OnNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, RoundNotification notification);

    void OnGameNotificationSent(NotificationId notificationId, PlayerIndex recipientIndex, GameNotification notification);

    void OnResponseReceived(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse response);

    void OnResponseTimeout(NotificationId notificationId, PlayerIndex recipientIndex);

    void OnResponseException(NotificationId notificationId, PlayerIndex recipientIndex, Exception ex);

    /// <summary>
    /// プレイヤー応答が提示済み候補に含まれない候補外応答であった場合に呼ばれる。
    /// RoundStateContext の通知・応答集約ループは <see cref="IDefaultResponseFactory"/> のフォールバック応答に差し替えて進行を継続する
    /// </summary>
    void OnInvalidResponse(NotificationId notificationId, PlayerIndex senderIndex, PlayerResponse invalidResponse, CandidateList presentedCandidates);

    void OnAdoptedAction(RoundInquiryPhase phase, AdoptedPlayerResponse adopted);

    void OnRoundStarted(Round round);

    void OnRoundEnded(AdoptedRoundAction action);

    /// <summary>
    /// ツモ牌が確定したタイミングで発火する (通常ツモ / 嶺上ツモ 双方)。
    /// 天鳳牌譜の <c>&lt;T{id}/&gt;〜&lt;W{id}/&gt;</c> タグ生成用
    /// </summary>
    void OnTsumoDrawn(PlayerIndex turn, Tile drawnTile, bool isRinshan);

    /// <summary>
    /// 新ドラ表示牌がめくられたタイミングで発火する (局開始の初期ドラ / 暗槓即乗り / 加槓・大明槓後のカンドラ)。
    /// 天鳳牌譜の <c>&lt;DORA hai="..."/&gt;</c> タグ生成用
    /// </summary>
    void OnDoraRevealed(Tile newIndicator);

    /// <summary>
    /// 立直宣言・成立タイミングで発火する。
    /// step=1: リーチ宣言 (打牌タグの直前に発火)、
    /// step=2: ロンされず成立した時点 (打牌タグ・供託加算後に発火)
    /// </summary>
    void OnRiichiDeclared(PlayerIndex player, int step);

    /// <summary>
    /// 副露 (チー/ポン/大明槓/暗槓/加槓) が確定したタイミングで発火する。
    /// 天鳳 JSON 牌譜の副露文字列生成用
    /// </summary>
    /// <param name="caller">副露したプレイヤー</param>
    /// <param name="call">副露内容 (CallType / Tiles / From / CalledTile)</param>
    void OnCallExecuted(PlayerIndex caller, Call call);
}
