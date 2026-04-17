using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Tests.Players;

// テストシナリオ記述用の疑似プレイヤー実装
// 各通知に対する応答をデリゲートで事前設定でき 受信通知を記録する
// デリゲート未設定時は安全な既定応答 (OK / 先頭候補の打牌 等) を返す
internal sealed class FakePlayer(PlayerId playerId, string displayName)
    : Player(playerId, displayName)
{
    private readonly List<object> receivedNotifications_ = [];
    private readonly Lock lock_ = new();

    public Func<GameStartNotification, CancellationToken, OkResponse>? OnGameStart { get; init; }
    public Func<RoundStartNotification, CancellationToken, OkResponse>? OnRoundStart { get; init; }
    public Func<RoundEndNotification, CancellationToken, OkResponse>? OnRoundEnd { get; init; }
    public Func<GameEndNotification, CancellationToken, OkResponse>? OnGameEnd { get; init; }
    public Func<HaipaiNotification, CancellationToken, OkResponse>? OnHaipai { get; init; }
    public Func<OtherPlayerTsumoNotification, CancellationToken, OkResponse>? OnOtherPlayerTsumo { get; init; }
    public Func<CallNotification, CancellationToken, OkResponse>? OnCall { get; init; }
    public Func<DoraRevealNotification, CancellationToken, OkResponse>? OnDoraReveal { get; init; }
    public Func<WinNotification, CancellationToken, OkResponse>? OnWin { get; init; }
    public Func<RyuukyokuNotification, CancellationToken, OkResponse>? OnRyuukyoku { get; init; }
    public Func<TsumoNotification, CancellationToken, AfterTsumoResponse>? OnTsumo { get; init; }
    public Func<DahaiNotification, CancellationToken, AfterDahaiResponse>? OnDahai { get; init; }
    public Func<KanNotification, CancellationToken, AfterKanResponse>? OnKan { get; init; }
    public Func<KanTsumoNotification, CancellationToken, AfterKanTsumoResponse>? OnKanTsumo { get; init; }

    // 受信した通知の記録 (テスト assertion 用)
    public IReadOnlyList<object> ReceivedNotifications
    {
        get
        {
            lock (lock_)
            {
                return [.. receivedNotifications_];
            }
        }
    }

    public override Task<OkResponse> OnGameStartAsync(GameStartNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult(OnGameStart?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnRoundStartAsync(RoundStartNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult(OnRoundStart?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnRoundEndAsync(RoundEndNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult(OnRoundEnd?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnGameEndAsync(GameEndNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult(OnGameEnd?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnHaipaiAsync(HaipaiNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult(OnHaipai?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnOtherPlayerTsumoAsync(OtherPlayerTsumoNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult(OnOtherPlayerTsumo?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnCallAsync(CallNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult(OnCall?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnDoraRevealAsync(DoraRevealNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult(OnDoraReveal?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnWinAsync(WinNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult(OnWin?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnRyuukyokuAsync(RyuukyokuNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult(OnRyuukyoku?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<AfterTsumoResponse> OnTsumoAsync(TsumoNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult(OnTsumo?.Invoke(notification, ct) ?? DefaultTsumoResponse(notification));
    }

    public override Task<AfterDahaiResponse> OnDahaiAsync(DahaiNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult<AfterDahaiResponse>(OnDahai?.Invoke(notification, ct) ?? new PassResponse());
    }

    public override Task<AfterKanResponse> OnKanAsync(KanNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult<AfterKanResponse>(OnKan?.Invoke(notification, ct) ?? new KanPassResponse());
    }

    public override Task<AfterKanTsumoResponse> OnKanTsumoAsync(KanTsumoNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        return Task.FromResult(OnKanTsumo?.Invoke(notification, ct) ?? DefaultKanTsumoResponse(notification));
    }

    private void Record(object notification)
    {
        lock (lock_)
        {
            receivedNotifications_.Add(notification);
        }
    }

    private static DahaiResponse DefaultTsumoResponse(TsumoNotification notification)
    {
        var dahai = notification.CandidateList.GetCandidates<DahaiCandidate>().FirstOrDefault();
        return dahai is not null && dahai.DahaiOptionList.Count != 0
            ? new DahaiResponse(dahai.DahaiOptionList[0].Tile)
            : new DahaiResponse(notification.TsumoTile);
    }

    private static KanTsumoDahaiResponse DefaultKanTsumoResponse(KanTsumoNotification notification)
    {
        var dahai = notification.CandidateList.GetCandidates<DahaiCandidate>().FirstOrDefault();
        return dahai is not null && dahai.DahaiOptionList.Count != 0
            ? new KanTsumoDahaiResponse(dahai.DahaiOptionList[0].Tile)
            : new KanTsumoDahaiResponse(notification.DrawnTile);
    }

    public static FakePlayer Create(int index)
    {
        return new FakePlayer(PlayerId.NewId(), $"F{index}");
    }
}
