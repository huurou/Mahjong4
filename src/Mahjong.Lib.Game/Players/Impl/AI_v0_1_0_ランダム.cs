using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Players.Impl;

/// <summary>
/// ver.0.1.0 ランダム — 最小実装の AI プレイヤー。
/// 和了候補があれば必ず和了し、打牌はランダム選択。副露・立直・暗槓・加槓・九種九牌は選択しない。
/// 新バージョンを追加する場合は本ファイルをコピーして <c>AI_vX_Y_Z_名前.cs</c> として差し替える
/// </summary>
public sealed class AI_v0_1_0_ランダム(PlayerId playerId, PlayerIndex playerIndex, Random rng)
    : Player(playerId, DISPLAY_NAME, playerIndex)
{
    /// <summary>
    /// 表示名 (ファイル内の定数として定義し、<see cref="AI_v0_1_0_ランダムFactory"/> から参照する)
    /// </summary>
    public const string DISPLAY_NAME = "ver.0.1.0 ランダム";

    private readonly Random rng_ = rng;

    public override Task<OkResponse> OnGameStartAsync(GameStartNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnRoundStartAsync(RoundStartNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnRoundEndAsync(RoundEndNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnGameEndAsync(GameEndNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnHaipaiAsync(HaipaiNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnOtherPlayerTsumoAsync(OtherPlayerTsumoNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnCallAsync(CallNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnDoraRevealAsync(DoraRevealNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnWinAsync(WinNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnRyuukyokuAsync(RyuukyokuNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnOtherPlayerKanTsumoAsync(OtherPlayerKanTsumoNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<AfterTsumoResponse> OnTsumoAsync(TsumoNotification notification, CancellationToken ct = default)
    {
        var candidates = notification.CandidateList;
        if (candidates.HasCandidate<TsumoAgariCandidate>())
        {
            return Task.FromResult<AfterTsumoResponse>(new TsumoAgariResponse());
        }

        var dahai = candidates.GetCandidates<DahaiCandidate>().FirstOrDefault()
            ?? throw new InvalidOperationException("ツモフェーズに DahaiCandidate が提示されませんでした。");
        var option = PickRandomOption(dahai);
        return Task.FromResult<AfterTsumoResponse>(new DahaiResponse(option.Tile));
    }

    public override Task<PlayerResponse> OnDahaiAsync(DahaiNotification notification, CancellationToken ct = default)
    {
        var candidates = notification.CandidateList;
        if (candidates.HasCandidate<RonCandidate>())
        {
            return Task.FromResult<PlayerResponse>(new RonResponse());
        }
        return Task.FromResult<PlayerResponse>(new OkResponse());
    }

    public override Task<PlayerResponse> OnKanAsync(KanNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult<PlayerResponse>(new OkResponse());
    }

    public override Task<AfterKanTsumoResponse> OnKanTsumoAsync(KanTsumoNotification notification, CancellationToken ct = default)
    {
        var candidates = notification.CandidateList;
        if (candidates.HasCandidate<RinshanTsumoAgariCandidate>())
        {
            return Task.FromResult<AfterKanTsumoResponse>(new RinshanTsumoResponse());
        }

        var dahai = candidates.GetCandidates<DahaiCandidate>().FirstOrDefault()
            ?? throw new InvalidOperationException("嶺上ツモフェーズに DahaiCandidate が提示されませんでした。");
        var option = PickRandomOption(dahai);
        return Task.FromResult<AfterKanTsumoResponse>(new KanTsumoDahaiResponse(option.Tile));
    }

    private DahaiOption PickRandomOption(DahaiCandidate candidate)
    {
        var options = candidate.DahaiOptionList;
        if (options.Count == 0)
        {
            throw new InvalidOperationException("DahaiCandidate の選択肢が空です。");
        }
        return options[rng_.Next(options.Count)];
    }
}

/// <summary>
/// <see cref="AI_v0_1_0_ランダム"/> を席順ごとに生成する <see cref="IPlayerFactory"/>。
/// 各席には seed と index を Fibonacci hashing (Knuth multiplicative) で決定的に合成した値で Random を初期化し、
/// 同一 seed に対する再現性を保ちつつ席間の内部状態が近接しないようにする。
/// <see cref="HashCode.Combine{T1, T2}(T1, T2)"/> はプロセス起動時のランダム salt を含むため再現性を壊すので使わない
/// </summary>
public sealed class AI_v0_1_0_ランダムFactory(int seed) : IPlayerFactory
{
    public string DisplayName => AI_v0_1_0_ランダム.DISPLAY_NAME;

    public Player Create(PlayerIndex index, PlayerId id)
    {
        return new AI_v0_1_0_ランダム(id, index, new Random(DeriveSeed(seed, index.Value)));
    }

    private static int DeriveSeed(int seed, int index)
    {
        unchecked
        {
            return (int)((uint)seed * 0x9E3779B9u ^ (uint)index);
        }
    }
}
