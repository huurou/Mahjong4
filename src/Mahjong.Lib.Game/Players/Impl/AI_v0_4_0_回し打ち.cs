using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using Mahjong.Lib.Scoring.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Players.Impl;

/// <summary>
/// ver.0.4.0 回し打ち — v0.3.0 評価値ロジックに加えて、他家リーチ時のみ「シャンテン数と切りたい牌の危険度」で
/// 押し引きを判定する書籍AI 0102アルゴリズム準拠の守備型 AI プレイヤー。
/// リーチ者 0 人時は v0.3.0 と同一挙動。リーチ者がいる場合:
///   テンパイ → 押し (リーチ可能ならリーチ)
///   1 シャンテン → 攻撃打牌の危険度 ≤ 5 なら回し打ち、そうでなければベタオリ
///   2 シャンテン以上 → ベタオリ
/// </summary>
public sealed class AI_v0_4_0_回し打ち(
    PlayerId playerId,
    PlayerIndex playerIndex,
    Random rng
) : Player(playerId, DISPLAY_NAME, playerIndex)
{
    /// <summary>
    /// 表示名 (ファイル内の定数として定義し、<see cref="AI_v0_4_0_回し打ちFactory"/> から参照する)
    /// </summary>
    public const string DISPLAY_NAME = "ver.0.4.0 回し打ち";

    /// <summary>
    /// 1 シャンテン時に回し打ちするかベタオリするかの閾値。
    /// 攻撃打牌の危険度がこの値以下なら回し打ち、超えるならベタオリ
    /// </summary>
    private const int MAWASHI_DANGER_THRESHOLD = 5;

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
        var chosen = SelectDahai(notification.View, dahai.DahaiOptionList);
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
        var chosen = SelectDahai(notification.View, dahai.DahaiOptionList);
        return Task.FromResult<AfterKanTsumoResponse>(new KanTsumoDahaiResponse(chosen.Tile, chosen.RiichiAvailable));
    }

    /// <summary>
    /// 回し打ちアルゴリズムで打牌を決定する。
    /// リーチ者がいない局面や自分がテンパイの場合は攻撃打牌 (v0.3.0 相当)、
    /// 1シャンテンで切りたい牌の危険度5なら回し打ち、それ以外はベタオリ。
    /// </summary>
    private DahaiOption SelectDahai(PlayerRoundView view, DahaiOptionList options)
    {
        if (options.Count == 0)
        {
            throw new InvalidOperationException("DahaiCandidate の選択肢が空です。");
        }

        var hasActiveRiichi = false;
        foreach (var status in view.OtherPlayerStatuses)
        {
            if (status.IsRiichi && status.SafeKindsAgainstRiichi is not null)
            {
                hasActiveRiichi = true;
                break;
            }
        }

        if (!hasActiveRiichi)
        {
            return SelectAttackDahai(view, options);
        }

        var shanten = ShantenHelper.CalcShanten(view.OwnHand);
        if (shanten <= 0)
        {
            // テンパイ (和了候補は呼び出し側で事前処理済) → 押し
            return SelectAttackDahai(view, options);
        }

        if (shanten == 1)
        {
            var attackDahai = SelectAttackDahai(view, options);
            var attackDanger = DangerEvaluator.CalcDanger(attackDahai.Tile, view);
            if (attackDanger <= MAWASHI_DANGER_THRESHOLD)
            {
                // 回し打ち (運よくテンパイすればリーチする)
                return attackDahai;
            }
        }

        // 2 シャンテン以上、または 1 シャンテンで切りたい牌が危険 → ベタオリ
        return SelectSafestDahai(view, options);
    }

    /// <summary>
    /// 全 DahaiOption から危険度最小の牌を選ぶ。同点はランダム。
    /// ベタオリ時はリーチしないため <see cref="DahaiOption.RiichiAvailable"/> を false に固定した option を返す。
    /// </summary>
    private DahaiOption SelectSafestDahai(PlayerRoundView view, DahaiOptionList options)
    {
        var evaluated = options
            .Select(x => (Option: x, Danger: DangerEvaluator.CalcDanger(x.Tile, view)))
            .ToList();
        var minDanger = evaluated.Min(x => x.Danger);
        var safest = evaluated
            .Where(x => x.Danger == minDanger)
            .Select(x => x.Option)
            .ToList();
        var chosen = safest[rng_.Next(safest.Count)];
        return chosen with { RiichiAvailable = false };
    }

    /// <summary>
    /// v0.3.0 評価値 AI の打牌選択ロジックをそのまま利用して、攻撃時の最適打牌を決定する。
    /// (将来の AI バージョンとの独立性のため継承ではなくコピー実装)
    /// </summary>
    private DahaiOption SelectAttackDahai(PlayerRoundView view, DahaiOptionList options)
    {
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
/// リーチ者に対する牌の危険度判定。
/// テストから直接呼べるよう internal で公開する。
/// </summary>
internal static class DangerEvaluator
{
    /// <summary>
    /// 全リーチ者に対する危険度の最大値を返す。リーチ者がいない場合は 0。
    /// </summary>
    public static int CalcDanger(Tile tile, PlayerRoundView view)
    {
        var max = 0;
        foreach (var status in view.OtherPlayerStatuses)
        {
            if (!status.IsRiichi || status.SafeKindsAgainstRiichi is not { } safeKinds) { continue; }

            var danger = CalcDangerAgainst(tile, safeKinds, view);
            max = Math.Max(max, danger);
        }
        return max;
    }

    /// <summary>
    /// 特定リーチ者視点での危険度を返す。<paramref name="safeKinds"/> はそのリーチ者の現物・スジ判定のベースになる牌種集合
    /// (Round が <see cref="PlayerRoundStatus.SafeKindsAgainstRiichi"/> として管理する)。
    /// </summary>
    public static int CalcDangerAgainst(Tile tile, ImmutableHashSet<TileKind> safeKinds, PlayerRoundView view)
    {
        var k = tile.Kind;
        if (safeKinds.Contains(k)) { return 0; }

        if (k.IsHonor)
        {
            var unseen = VisibleTileCounter.CountUnseen(view, k);
            return Math.Min(unseen, 3);
        }

        var n = k.Number;
        var hasMinus3 = k.TryGetAtDistance(-3, out var minus3) && safeKinds.Contains(minus3);
        var hasPlus3 = k.TryGetAtDistance(+3, out var plus3) && safeKinds.Contains(plus3);

        return n switch
        {
            1 => hasPlus3 ? 3 : 6,
            2 => hasPlus3 ? 4 : 8,
            3 => hasPlus3 ? 5 : 8,
            7 => hasMinus3 ? 5 : 8,
            8 => hasMinus3 ? 4 : 8,
            9 => hasMinus3 ? 3 : 6,
            _ => (hasMinus3, hasPlus3) switch   // 4, 5, 6
            {
                (true, true) => 4,
                (true, false) or (false, true) => 8,
                _ => 12,
            },
        };
    }
}

/// <summary>
/// <see cref="AI_v0_4_0_回し打ち"/> を席順ごとに生成する <see cref="IPlayerFactory"/>。
/// 共通ロジック (Fibonacci hashing による決定的シード派生) は <see cref="AiPlayerFactoryBase{TPlayer}"/> に委譲する
/// </summary>
public sealed class AI_v0_4_0_回し打ちFactory(int seed)
    : AiPlayerFactoryBase<AI_v0_4_0_回し打ち>(seed, AI_v0_4_0_回し打ち.DISPLAY_NAME)
{
    protected override AI_v0_4_0_回し打ち CreatePlayer(PlayerId id, PlayerIndex index, Random rng)
    {
        return new(id, index, rng);
    }
}
