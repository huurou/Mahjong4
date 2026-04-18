using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Tests.Players;

// テストシナリオ記述用の疑似プレイヤー実装
// 各通知に対する応答を 同期デリゲート (OnXxx) または 非同期デリゲート (OnXxxHandler) で事前設定でき
// 受信通知を記録する
// 優先順位: Async > Sync > 既定応答
// 同期デリゲート名 OnXxxAsync は override メソッド名と衝突するため async 版は Handler サフィックス付き
internal sealed class FakePlayer(PlayerId playerId, string displayName, PlayerIndex playerIndex)
    : Player(playerId, displayName, playerIndex)
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
    public Func<DahaiNotification, CancellationToken, PlayerResponse>? OnDahai { get; init; }
    public Func<KanNotification, CancellationToken, PlayerResponse>? OnKan { get; init; }
    public Func<KanTsumoNotification, CancellationToken, AfterKanTsumoResponse>? OnKanTsumo { get; init; }

    public Func<GameStartNotification, CancellationToken, Task<OkResponse>>? OnGameStartHandler { get; init; }
    public Func<RoundStartNotification, CancellationToken, Task<OkResponse>>? OnRoundStartHandler { get; init; }
    public Func<RoundEndNotification, CancellationToken, Task<OkResponse>>? OnRoundEndHandler { get; init; }
    public Func<GameEndNotification, CancellationToken, Task<OkResponse>>? OnGameEndHandler { get; init; }
    public Func<HaipaiNotification, CancellationToken, Task<OkResponse>>? OnHaipaiHandler { get; init; }
    public Func<OtherPlayerTsumoNotification, CancellationToken, Task<OkResponse>>? OnOtherPlayerTsumoHandler { get; init; }
    public Func<CallNotification, CancellationToken, Task<OkResponse>>? OnCallHandler { get; init; }
    public Func<DoraRevealNotification, CancellationToken, Task<OkResponse>>? OnDoraRevealHandler { get; init; }
    public Func<WinNotification, CancellationToken, Task<OkResponse>>? OnWinHandler { get; init; }
    public Func<RyuukyokuNotification, CancellationToken, Task<OkResponse>>? OnRyuukyokuHandler { get; init; }
    public Func<TsumoNotification, CancellationToken, Task<AfterTsumoResponse>>? OnTsumoHandler { get; init; }
    public Func<DahaiNotification, CancellationToken, Task<PlayerResponse>>? OnDahaiHandler { get; init; }
    public Func<KanNotification, CancellationToken, Task<PlayerResponse>>? OnKanHandler { get; init; }
    public Func<KanTsumoNotification, CancellationToken, Task<AfterKanTsumoResponse>>? OnKanTsumoHandler { get; init; }

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
        if (OnGameStartHandler is not null)
        {
            return OnGameStartHandler(notification, ct);
        }

        return Task.FromResult(OnGameStart?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnRoundStartAsync(RoundStartNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnRoundStartHandler is not null)
        {
            return OnRoundStartHandler(notification, ct);
        }

        return Task.FromResult(OnRoundStart?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnRoundEndAsync(RoundEndNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnRoundEndHandler is not null)
        {
            return OnRoundEndHandler(notification, ct);
        }

        return Task.FromResult(OnRoundEnd?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnGameEndAsync(GameEndNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnGameEndHandler is not null)
        {
            return OnGameEndHandler(notification, ct);
        }

        return Task.FromResult(OnGameEnd?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnHaipaiAsync(HaipaiNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnHaipaiHandler is not null)
        {
            return OnHaipaiHandler(notification, ct);
        }

        return Task.FromResult(OnHaipai?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnOtherPlayerTsumoAsync(OtherPlayerTsumoNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnOtherPlayerTsumoHandler is not null)
        {
            return OnOtherPlayerTsumoHandler(notification, ct);
        }

        return Task.FromResult(OnOtherPlayerTsumo?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnCallAsync(CallNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnCallHandler is not null)
        {
            return OnCallHandler(notification, ct);
        }

        return Task.FromResult(OnCall?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnDoraRevealAsync(DoraRevealNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnDoraRevealHandler is not null)
        {
            return OnDoraRevealHandler(notification, ct);
        }

        return Task.FromResult(OnDoraReveal?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnWinAsync(WinNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnWinHandler is not null)
        {
            return OnWinHandler(notification, ct);
        }

        return Task.FromResult(OnWin?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<OkResponse> OnRyuukyokuAsync(RyuukyokuNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnRyuukyokuHandler is not null)
        {
            return OnRyuukyokuHandler(notification, ct);
        }

        return Task.FromResult(OnRyuukyoku?.Invoke(notification, ct) ?? new OkResponse());
    }

    public override Task<AfterTsumoResponse> OnTsumoAsync(TsumoNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnTsumoHandler is not null)
        {
            return OnTsumoHandler(notification, ct);
        }

        return Task.FromResult(OnTsumo?.Invoke(notification, ct) ?? DefaultTsumoResponse(notification));
    }

    public override Task<PlayerResponse> OnDahaiAsync(DahaiNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnDahaiHandler is not null)
        {
            return OnDahaiHandler(notification, ct);
        }

        return Task.FromResult(OnDahai?.Invoke(notification, ct) ?? (PlayerResponse)new OkResponse());
    }

    public override Task<PlayerResponse> OnKanAsync(KanNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnKanHandler is not null)
        {
            return OnKanHandler(notification, ct);
        }

        return Task.FromResult(OnKan?.Invoke(notification, ct) ?? (PlayerResponse)new OkResponse());
    }

    public override Task<AfterKanTsumoResponse> OnKanTsumoAsync(KanTsumoNotification notification, CancellationToken ct = default)
    {
        Record(notification);
        if (OnKanTsumoHandler is not null)
        {
            return OnKanTsumoHandler(notification, ct);
        }

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
        return new FakePlayer(PlayerId.NewId(), $"F{index}", new PlayerIndex(index));
    }
}
