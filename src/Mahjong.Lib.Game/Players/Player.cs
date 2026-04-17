using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Players;

/// <summary>
/// プレイヤー
/// 対局/局の各通知を受け取り応答を返す抽象基底
/// 識別情報 (PlayerId / DisplayName) による値等価を実装する
/// </summary>
public abstract class Player
{
    protected Player(PlayerId playerId, string displayName)
    {
        ArgumentNullException.ThrowIfNull(playerId);
        ArgumentNullException.ThrowIfNull(displayName);

        PlayerId = playerId;
        DisplayName = displayName;
    }

    /// <summary>
    /// プレイヤー識別子
    /// </summary>
    public PlayerId PlayerId { get; }

    /// <summary>
    /// 表示名
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 対局開始通知 (各プレイヤー個別)
    /// </summary>
    public abstract Task<OkResponse> OnGameStartAsync(GameStartNotification notification, CancellationToken ct = default);

    /// <summary>
    /// 局開始通知 (全プレイヤー共通)
    /// </summary>
    public abstract Task<OkResponse> OnRoundStartAsync(RoundStartNotification notification, CancellationToken ct = default);

    /// <summary>
    /// 局終了通知
    /// </summary>
    public abstract Task<OkResponse> OnRoundEndAsync(RoundEndNotification notification, CancellationToken ct = default);

    /// <summary>
    /// 対局終了通知
    /// </summary>
    public abstract Task<OkResponse> OnGameEndAsync(GameEndNotification notification, CancellationToken ct = default);

    /// <summary>
    /// 配牌通知 (全プレイヤー)
    /// </summary>
    public abstract Task<OkResponse> OnHaipaiAsync(HaipaiNotification notification, CancellationToken ct = default);

    /// <summary>
    /// 他家ツモ通知 (非手番プレイヤー用)
    /// </summary>
    public abstract Task<OkResponse> OnOtherPlayerTsumoAsync(OtherPlayerTsumoNotification notification, CancellationToken ct = default);

    /// <summary>
    /// 副露成立通知 (全プレイヤー)
    /// </summary>
    public abstract Task<OkResponse> OnCallAsync(CallNotification notification, CancellationToken ct = default);

    /// <summary>
    /// ドラ表示通知 (全プレイヤー)
    /// </summary>
    public abstract Task<OkResponse> OnDoraRevealAsync(DoraRevealNotification notification, CancellationToken ct = default);

    /// <summary>
    /// 和了通知 (全プレイヤー)
    /// </summary>
    public abstract Task<OkResponse> OnWinAsync(WinNotification notification, CancellationToken ct = default);

    /// <summary>
    /// 流局通知 (全プレイヤー)
    /// </summary>
    public abstract Task<OkResponse> OnRyuukyokuAsync(RyuukyokuNotification notification, CancellationToken ct = default);

    /// <summary>
    /// ツモ通知 (手番プレイヤー) 打牌/暗槓/加槓/ツモ和了/九種九牌のいずれかを応答
    /// </summary>
    public abstract Task<AfterTsumoResponse> OnTsumoAsync(TsumoNotification notification, CancellationToken ct = default);

    /// <summary>
    /// 打牌通知 (非手番プレイヤー用) スルー/チー/ポン/大明槓/ロンのいずれかを応答
    /// </summary>
    public abstract Task<AfterDahaiResponse> OnDahaiAsync(DahaiNotification notification, CancellationToken ct = default);

    /// <summary>
    /// 加槓通知 (非手番プレイヤー用) スルー/槍槓ロンのいずれかを応答
    /// </summary>
    public abstract Task<AfterKanResponse> OnKanAsync(KanNotification notification, CancellationToken ct = default);

    /// <summary>
    /// 嶺上ツモ通知 (手番プレイヤー) 嶺上開花ツモ和了/打牌/暗槓/加槓のいずれかを応答
    /// </summary>
    public abstract Task<AfterKanTsumoResponse> OnKanTsumoAsync(KanTsumoNotification notification, CancellationToken ct = default);

    public sealed override bool Equals(object? obj)
    {
        return obj is Player other &&
            PlayerId.Equals(other.PlayerId) &&
            DisplayName == other.DisplayName;
    }

    public sealed override int GetHashCode()
    {
        return HashCode.Combine(PlayerId, DisplayName);
    }

    public static bool operator ==(Player? left, Player? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Player? left, Player? right)
    {
        return !Equals(left, right);
    }
}
