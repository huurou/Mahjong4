using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using Mahjong.Lib.Scoring.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Players.Impl;

/// <summary>
/// ver.0.3.0 評価値 — 有効牌枚数同点時のタイブレーカーとして、対象牌を孤立牌と見立てた面子完成ポテンシャル (評価値) が低い牌を優先して切る AI プレイヤー。
/// v0.2.0 と同じシャンテン・有効牌ロジックに加え、評価値最小の候補群に絞った上でランダム選択する。
/// 評価値 = Σ (くっつき候補の「使える枚数」×くっつき先表ドラ倍率) × (対象牌自身の表ドラ倍率×赤ドラ倍率×役牌倍率)
/// </summary>
public sealed class AI_v0_3_0_評価値(
    PlayerId playerId,
    PlayerIndex playerIndex,
    Random rng
) : Player(playerId, DISPLAY_NAME, playerIndex)
{
    /// <summary>
    /// 表示名 (ファイル内の定数として定義し、<see cref="AI_v0_3_0_評価値Factory"/> から参照する)
    /// </summary>
    public const string DISPLAY_NAME = "ver.0.3.0 評価値";

    private readonly Random rng_ = rng;
    private GameRules? rules_;

    public override Task<OkResponse> OnGameStartAsync(GameStartNotification notification, CancellationToken ct = default)
    {
        rules_ = notification.Rules;
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

    public override Task<DahaiResponse> OnAfterCallAsync(AfterCallNotification notification, CancellationToken ct = default)
    {
        var candidates = notification.CandidateList;
        var dahai = candidates.GetCandidates<DahaiCandidate>().FirstOrDefault()
            ?? throw new InvalidOperationException("副露後フェーズに DahaiCandidate が提示されませんでした。");
        var chosen = SelectBestDahai(notification.View, dahai.DahaiOptionList);
        // 副露後は非門前のため立直不可
        return Task.FromResult(new DahaiResponse(chosen.Tile));
    }

    public override Task<OkResponse> OnOtherPlayerAfterCallAsync(OtherPlayerAfterCallNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
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
        // 同一 Kind の打牌候補は Hand13 が同形のため結果が一致し、ShantenCalculator 呼び出しを抑制できる
        var shantenCache = new Dictionary<TileKind, int>();
        var usefulCache = new Dictionary<TileKind, ImmutableHashSet<TileKind>>();
        var visibleCache = new Dictionary<TileKind, int>();

        int GetUnseen(TileKind kind)
        {
            if (!visibleCache.TryGetValue(kind, out var visible))
            {
                visible = VisibleTileCounter.CountUnseen(view, kind);
                visibleCache[kind] = visible;
            }
            return visible;
        }

        int GetShanten(Hands.Hand hand13, TileKind kind)
        {
            if (!shantenCache.TryGetValue(kind, out var shanten))
            {
                shanten = ShantenHelper.CalcShanten(hand13);
                shantenCache[kind] = shanten;
            }
            return shanten;
        }

        int GetUsefulScore(Hands.Hand hand13, TileKind kind, int shanten)
        {
            if (!usefulCache.TryGetValue(kind, out var useful))
            {
                useful = ShantenHelper.EnumerateUsefulTileKinds(hand13, shanten);
                usefulCache[kind] = useful;
            }
            var score = 0;
            foreach (var usefulKind in useful)
            {
                score += GetUnseen(usefulKind);
            }
            return score;
        }

        // フェーズ 1: 最小シャンテン × 最大有効牌スコアで finalists を決定 (v0.2.0 と同一)
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
            .Select(x => (x.Option, Score: GetUsefulScore(x.Hand13, x.Option.Tile.Kind, x.Shanten)))
            .ToList();
        var maxScore = scored.Max(x => x.Score);
        var finalists = scored
            .Where(x => x.Score == maxScore)
            .Select(x => x.Option)
            .ToList();

        if (finalists.Count == 1)
        {
            return finalists[0];
        }

        // フェーズ 2: 評価値タイブレーカー
        var doraIndicatorCounts = BuildDoraIndicatorCounts(view);
        var yakuhaiKinds = BuildYakuhaiKinds(view);
        var baseEvaluationCache = new Dictionary<TileKind, int>();

        int GetBaseEvaluation(TileKind kind)
        {
            if (baseEvaluationCache.TryGetValue(kind, out var cached)) { return cached; }
            var termSum = 0;
            foreach (var (x, d) in EnumerateAdjacents(kind))
            {
                var useful = CalcUseful(x, kind, d, GetUnseen);
                var termMult = x == kind ? 1 : 1 << doraIndicatorCounts.GetValueOrDefault(x);
                termSum += useful * termMult;
            }
            var outer = 1 << doraIndicatorCounts.GetValueOrDefault(kind);
            if (yakuhaiKinds.Contains(kind)) { outer *= 2; }
            var result = termSum * outer;
            baseEvaluationCache[kind] = result;
            return result;
        }

        int CalcEvaluation(Tile tile)
        {
            // OnGameStartAsync で rules_ が設定される前提。未設定なら赤ドラ評価が silent に抜けるのを防ぐため明示的に fail する
            if (rules_ is null)
            {
                throw new InvalidOperationException("OnGameStartAsync 未受信のまま評価値計算が呼ばれました。");
            }
            var value = GetBaseEvaluation(tile.Kind);
            if (rules_.IsRedDora(tile)) { value *= 2; }
            return value;
        }

        var withEvaluation = finalists
            .Select(x => (Option: x, Evaluation: CalcEvaluation(x.Tile)))
            .ToList();
        var minEvaluation = withEvaluation.Min(x => x.Evaluation);
        var bottomFinalists = withEvaluation
            .Where(x => x.Evaluation == minEvaluation)
            .Select(x => x.Option)
            .ToList();

        return bottomFinalists[rng_.Next(bottomFinalists.Count)];
    }

    private static Dictionary<TileKind, int> BuildDoraIndicatorCounts(PlayerRoundView view)
    {
        var counts = new Dictionary<TileKind, int>();
        foreach (var indicator in view.DoraIndicators)
        {
            var doraKind = TileKind.GetActualDora(indicator.Kind);
            counts[doraKind] = counts.GetValueOrDefault(doraKind) + 1;
        }
        return counts;
    }

    private static ImmutableHashSet<TileKind> BuildYakuhaiKinds(PlayerRoundView view)
    {
        var builder = ImmutableHashSet.CreateBuilder<TileKind>();
        foreach (var dragon in TileKind.Dragons)
        {
            builder.Add(dragon);
        }
        builder.Add(TileKind.Winds[view.RoundWind.Value]);
        var dealerIndex = view.RoundNumber.ToDealer();
        var seatWind = (view.ViewerIndex.Value - dealerIndex.Value + PlayerIndex.PLAYER_COUNT) % PlayerIndex.PLAYER_COUNT;
        builder.Add(TileKind.Winds[seatWind]);
        return builder.ToImmutable();
    }

    private static IEnumerable<(TileKind X, int Distance)> EnumerateAdjacents(TileKind kind)
    {
        yield return (kind, 0);
        if (kind.IsHonor) { yield break; }
        foreach (var d in new[] { -2, -1, 1, 2 })
        {
            if (kind.TryGetAtDistance(d, out var k)) { yield return (k, d); }
        }
    }

    private static int CalcUseful(TileKind x, TileKind kind, int d, Func<TileKind, int> getUnseen)
    {
        var unseenX = getUnseen(x);

        return d switch
        {
            0 => unseenX,
            -2 => Math.Min(unseenX, neighbor(-1, kind, getUnseen)),
            +2 => Math.Min(unseenX, neighbor(+1, kind, getUnseen)),
            -1 => Math.Min(unseenX, Math.Max(neighbor(-2, kind, getUnseen), neighbor(+1, kind, getUnseen))),
            +1 => Math.Min(unseenX, Math.Max(neighbor(-1, kind, getUnseen), neighbor(+2, kind, getUnseen))),
            _ => 0,
        };

        static int neighbor(int distance, TileKind kind, Func<TileKind, int> getUnseen)
        {
            return kind.TryGetAtDistance(distance, out var k) ? getUnseen(k) : 0;
        }
    }
}

/// <summary>
/// <see cref="AI_v0_3_0_評価値"/> を席順ごとに生成する <see cref="IPlayerFactory"/>。
/// 共通ロジック (Fibonacci hashing による決定的シード派生) は <see cref="AiPlayerFactoryBase{TPlayer}"/> に委譲する
/// </summary>
public sealed class AI_v0_3_0_評価値Factory(int seed)
    : AiPlayerFactoryBase<AI_v0_3_0_評価値>(seed, AI_v0_3_0_評価値.DISPLAY_NAME)
{
    protected override AI_v0_3_0_評価値 CreatePlayer(PlayerId id, PlayerIndex index, Random rng)
    {
        return new(id, index, rng);
    }
}
