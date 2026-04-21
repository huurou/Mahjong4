using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Views;
using Mahjong.Lib.Scoring.Tiles;
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
    Random rng
) : Player(playerId, DISPLAY_NAME, playerIndex)
{
    /// <summary>
    /// 表示名 (ファイル内の定数として定義し、<see cref="AI_v0_2_0_有効牌Factory"/> から参照する)
    /// </summary>
    public const string DISPLAY_NAME = "ver.0.2.0 有効牌";

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

        // Tile.Kind をキーに Shanten / 有効牌 / 未見枚数をメモ化する。
        // 同一 Kind の打牌候補 (例: 萬1 を 3枚持つ場合の各牌) は Hand13 が同形になるため結果が一致する。
        // 最悪で Kind 数しか ShantenCalculator を呼ばないため、34牌種ループの重複計算を抑制できる
        var shantenCache = new Dictionary<TileKind, int>();
        var usefulCache = new Dictionary<TileKind, ImmutableHashSet<TileKind>>();
        var visibleCache = new Dictionary<TileKind, int>();

        int GetShanten(Hands.Hand hand13, TileKind kind)
        {
            if (!shantenCache.TryGetValue(kind, out var shanten))
            {
                shanten = ShantenHelper.CalcShanten(hand13);
                shantenCache[kind] = shanten;
            }
            return shanten;
        }

        int GetScore(Hands.Hand hand13, TileKind kind, int shanten)
        {
            if (!usefulCache.TryGetValue(kind, out var useful))
            {
                useful = ShantenHelper.EnumerateUsefulTileKinds(hand13, shanten);
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
            .Select(x =>
            {
                var hand13 = hand14.RemoveTile(x.Tile);
                return (Option: x, Hand13: hand13, Shanten: GetShanten(hand13, x.Tile.Kind));
            })
            .ToList();
        var minShanten = evaluated.Min(x => x.Shanten);
        var scored = evaluated
            .Where(x => x.Shanten == minShanten)
            .Select(x => (x.Option, Score: GetScore(x.Hand13, x.Option.Tile.Kind, x.Shanten)))
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
/// 共通ロジック (Fibonacci hashing による決定的シード派生) は <see cref="AiPlayerFactoryBase{TPlayer}"/> に委譲する
/// </summary>
public sealed class AI_v0_2_0_有効牌Factory(int seed)
    : AiPlayerFactoryBase<AI_v0_2_0_有効牌>(seed, AI_v0_2_0_有効牌.DISPLAY_NAME)
{
    protected override AI_v0_2_0_有効牌 CreatePlayer(PlayerId id, PlayerIndex index, Random rng)
    {
        return new(id, index, rng);
    }
}
