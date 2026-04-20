using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Views;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Players.Impl;

/// <summary>
/// ver.0.2.0 有効牌 — シャンテン数と有効牌ベースで打牌する AI プレイヤー。
/// シャンテン数が減らない牌の中で、切った後の未見有効牌枚数が最多のものを選択する。
/// 同点時はランダム。リーチ可能なら必ずリーチ。ロン/ツモ和了/槍槓は常に行う。副露/暗槓/加槓/九種九牌は選択しない
/// </summary>
public sealed class AI_v0_2_0_有効牌(
    PlayerId playerId,
    PlayerIndex playerIndex,
    Random rng,
    IShantenEvaluator evaluator
) : Player(playerId, DISPLAY_NAME, playerIndex)
{
    /// <summary>
    /// 表示名 (ファイル内の定数として定義し、<see cref="AI_v0_2_0_有効牌Factory"/> から参照する)
    /// </summary>
    public const string DISPLAY_NAME = "ver.0.2.0 有効牌";

    private readonly Random rng_ = rng;
    private readonly IShantenEvaluator evaluator_ = evaluator;

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
        var chosen = SelectBestDahai(notification.View, dahai.DahaiOptionList);
        return Task.FromResult<AfterTsumoResponse>(new DahaiResponse(chosen.Tile, chosen.RiichiAvailable));
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
        var candidates = notification.CandidateList;
        if (candidates.HasCandidate<RonCandidate>())
        {
            return Task.FromResult<PlayerResponse>(new RonResponse());
        }
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
        var chosen = SelectBestDahai(notification.View, dahai.DahaiOptionList);
        return Task.FromResult<AfterKanTsumoResponse>(new KanTsumoDahaiResponse(chosen.Tile, chosen.RiichiAvailable));
    }

    private DahaiOption SelectBestDahai(PlayerRoundView view, DahaiOptionList options)
    {
        if (options.Count == 0)
        {
            throw new InvalidOperationException("DahaiCandidate の選択肢が空です。");
        }

        var hand14 = view.OwnHand;
        var callList = view.CallListArray[view.ViewerIndex];

        // Tile.Kind をキーに Shanten / 有効牌 / 未見枚数をメモ化する。
        // 同一 Kind の打牌候補 (例: 萬1 を 3 枚持つ場合の各牌) は Hand13 が同形になるため結果が一致する。
        // 最悪で Kind 種数ぶんしか ShantenCalculator を呼ばないため、34 牌種ループの重複計算を抑制できる
        var shantenCache = new Dictionary<int, int>();
        var usefulCache = new Dictionary<int, ImmutableHashSet<int>>();
        var visibleCache = new Dictionary<int, int>();

        int GetShanten(Hands.Hand hand13, int kind)
        {
            if (!shantenCache.TryGetValue(kind, out var shanten))
            {
                shanten = evaluator_.CalcShanten(hand13, callList);
                shantenCache[kind] = shanten;
            }
            return shanten;
        }

        int GetScore(Hands.Hand hand13, int kind)
        {
            if (!usefulCache.TryGetValue(kind, out var useful))
            {
                useful = evaluator_.EnumerateUsefulTileKinds(hand13, callList);
                usefulCache[kind] = useful;
            }
            var score = 0;
            foreach (var usefulKind in useful)
            {
                if (!visibleCache.TryGetValue(usefulKind, out var visible))
                {
                    visible = VisibleTileCounter.CountUnseen(view, usefulKind);
                    visibleCache[usefulKind] = visible;
                }
                score += visible;
            }
            return score;
        }

        var evaluated = options
            .Select(x => (Option: x, Hand13: hand14.RemoveTile(x.Tile)))
            .Select(x => (x.Option, x.Hand13, Shanten: GetShanten(x.Hand13, x.Option.Tile.Kind)))
            .ToList();
        var minShanten = evaluated.Min(x => x.Shanten);
        var scored = evaluated
            .Where(x => x.Shanten == minShanten)
            .Select(x => (x.Option, Score: GetScore(x.Hand13, x.Option.Tile.Kind)))
            .ToList();
        var maxScore = scored.Max(x => x.Score);
        var finalists = scored
            .Where(x => x.Score == maxScore)
            .Select(x => x.Option)
            .ToList();
        return finalists[rng_.Next(finalists.Count)];
    }
}

/// <summary>
/// <see cref="AI_v0_2_0_有効牌"/> を席順ごとに生成する <see cref="IPlayerFactory"/>。
/// 各席には seed と index を <see cref="HashCode.Combine{T1, T2}(T1, T2)"/> で合成した値で Random を初期化し、
/// 同一 seed に対する再現性を保ちつつ席間の内部状態が近接しないようにする
/// </summary>
public sealed class AI_v0_2_0_有効牌Factory(int seed, IShantenEvaluator evaluator) : IPlayerFactory
{
    public string DisplayName => AI_v0_2_0_有効牌.DISPLAY_NAME;

    public Player Create(PlayerIndex index, PlayerId id)
    {
        return new AI_v0_2_0_有効牌(id, index, new Random(HashCode.Combine(seed, index.Value)), evaluator);
    }
}
