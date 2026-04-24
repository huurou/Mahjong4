using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Views;
using Mahjong.Lib.Scoring.Tiles;
using System.Collections.Immutable;
using Wind = Mahjong.Lib.Scoring.Games.Wind;

namespace Mahjong.Lib.Game.Players.Impl;

/// <summary>
/// ver.0.6.1 手作り — v0.6.0 の和了率激減 (25.85%→8.75%) を書籍準拠の修正で回復する改訂版。
/// ロジック自体は v0.6.0 と同一。共通評価器 <see cref="HandShapeEvaluator"/> と <see cref="TileWeights"/>
/// の以下 3 点を書籍 (kobalab/majiang-ai 0301〜0305) 準拠に修正したことで評価値の挙動が変わる:
///   A. <see cref="HandShapeEvaluator"/>.EvaluateFulouPostDahaiTerminal を 1 シャンテンまで拡張。
///      副露後 1 シャンテン手が全て評価値 0 になる不具合を修正し、副露率の回復を狙う。
///   B1+B2. <see cref="TileWeights"/> の閾値・乗数を書籍 make_paijia に合わせる
///      (染め ×4 / 字既定値 / 三元 ≥3 で ×8 / 風 ≥2 で ×4)。
///   B3. <see cref="HandShapeEvaluator"/>.EvaluateHand13 / EvaluateBacktrack が TileWeights を乗算する。
///      v0.6.0 はフォールバック経路のみで乗算していた。
/// <b>注意</b>: 変更は共有評価器 (<see cref="HandShapeEvaluator"/> / <see cref="TileWeights"/>) に入っているため、
/// <see cref="AI_v0_6_0_手作り"/> も同じ評価器を参照し暗黙的に書籍準拠化される。
/// そのため <c>AutoPlay</c> での v0.6.0 vs v0.6.1 比較は、純粋な旧 v0.6.0 (書籍乖離実装) との比較ではなく、
/// 「書籍準拠評価器を共有した v0.6.0 コード vs v0.6.1 コード」の比較になる (plan の設計意図)。
/// </summary>
public sealed class AI_v0_6_1_手作り(
    PlayerId playerId,
    PlayerIndex playerIndex,
    Random rng
) : Player(playerId, DISPLAY_NAME, playerIndex)
{
    public const string DISPLAY_NAME = "ver.0.6.1 手作り";

    /// <summary>
    /// 1 シャンテン時に回し打ちするかベタオリするかの閾値 (v0.4.0 と同じ)
    /// </summary>
    private const int MAWASHI_DANGER_THRESHOLD = 5;

    /// <summary>
    /// 評価値ベースの打牌選択 / 副露判断に切り替える役ありシャンテンの閾値。
    /// </summary>
    private const int EVALUATION_SHANTEN_THRESHOLD = 2;

    private readonly Random rng_ = rng;
    private readonly HandShapeEvaluator evaluator_ = new();
    private GameRules? rules_;

    internal static int SlowDecisionLogThresholdMs = 0;

    public override Task<OkResponse> OnGameStartAsync(GameStartNotification notification, CancellationToken ct = default)
    {
        rules_ = notification.Rules;
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnRoundStartAsync(RoundStartNotification notification, CancellationToken ct = default)
    {
        evaluator_.ClearBetweenRounds();
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnRoundEndAsync(RoundEndNotification notification, CancellationToken ct = default)
    {
        evaluator_.ClearBetweenRounds();
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnGameEndAsync(GameEndNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnHaipaiAsync(HaipaiNotification notification, CancellationToken ct = default)
    {
        evaluator_.ClearBetweenRounds();
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnOtherPlayerTsumoAsync(OtherPlayerTsumoNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnCallAsync(CallNotification notification, CancellationToken ct = default)
    {
        evaluator_.ClearAll();
        return Task.FromResult(new OkResponse());
    }

    public override Task<DahaiResponse> OnAfterCallAsync(AfterCallNotification notification, CancellationToken ct = default)
    {
        EnsureRules();
        var candidates = notification.CandidateList;
        var dahai = candidates.GetCandidates<DahaiCandidate>().FirstOrDefault()
            ?? throw new InvalidOperationException("副露後フェーズに DahaiCandidate が提示されませんでした。");
        var chosen = SelectDahai(notification.View, dahai.DahaiOptionList);
        return Task.FromResult(new DahaiResponse(chosen.Tile));
    }

    public override Task<OkResponse> OnOtherPlayerAfterCallAsync(OtherPlayerAfterCallNotification notification, CancellationToken ct = default)
    {
        evaluator_.ClearEvalOnly();
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnDoraRevealAsync(DoraRevealNotification notification, CancellationToken ct = default)
    {
        evaluator_.ClearAll();
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
        EnsureRules();
        evaluator_.ClearEvalOnly();
        var candidates = notification.CandidateList;
        if (candidates.HasCandidate<TsumoAgariCandidate>())
        {
            return Task.FromResult<AfterTsumoResponse>(new TsumoAgariResponse());
        }

        var view = notification.View;
        var currentShanten = CalcYakuAwareShanten(view.OwnHand, GetCalls(view), view);

        if (!ShouldSkipKanDueToRiichi(view, currentShanten))
        {
            var ankanCall = TryFindAnkanSame(view, candidates, currentShanten);
            if (ankanCall is not null)
            {
                return Task.FromResult<AfterTsumoResponse>(new AnkanResponse(ankanCall));
            }

            var kakanCall = TryFindKakanSame(view, candidates, currentShanten);
            if (kakanCall is not null)
            {
                return Task.FromResult<AfterTsumoResponse>(new KakanResponse(kakanCall));
            }
        }

        var dahaiCandidate = candidates.GetCandidates<DahaiCandidate>().FirstOrDefault()
            ?? throw new InvalidOperationException("ツモフェーズに DahaiCandidate が提示されませんでした。");
        var chosen = SelectDahai(view, dahaiCandidate.DahaiOptionList);
        return Task.FromResult<AfterTsumoResponse>(new DahaiResponse(chosen.Tile, chosen.RiichiAvailable));
    }

    public override Task<PlayerResponse> OnDahaiAsync(DahaiNotification notification, CancellationToken ct = default)
    {
        EnsureRules();
        evaluator_.ClearEvalOnly();
        var candidates = notification.CandidateList;
        if (candidates.HasCandidate<RonCandidate>())
        {
            return Task.FromResult<PlayerResponse>(new RonResponse());
        }

        var view = notification.View;
        var currentShanten = CalcYakuAwareShanten(view.OwnHand, GetCalls(view), view);

        if (currentShanten <= 0)
        {
            return Task.FromResult<PlayerResponse>(new OkResponse());
        }

        if (HasActiveRiichiOpponent(view) && currentShanten > 1)
        {
            return Task.FromResult<PlayerResponse>(new OkResponse());
        }

        var calls = GetCalls(view);

        if (currentShanten <= EVALUATION_SHANTEN_THRESHOLD)
        {
            var evalResponse = SelectFulouByEvaluation(view, candidates, notification, calls);
            if (evalResponse is not null)
            {
                return Task.FromResult(evalResponse);
            }
            return Task.FromResult<PlayerResponse>(new OkResponse());
        }

        foreach (var daiminkan in candidates.GetCandidates<DaiminkanCandidate>())
        {
            var postShanten = CalcCallSimulatedShanten(view, calls, CallType.Daiminkan, daiminkan.HandTiles, notification.DiscardedTile, notification.DiscarderIndex);
            if (postShanten == currentShanten)
            {
                return Task.FromResult<PlayerResponse>(new DaiminkanResponse(daiminkan.HandTiles));
            }
        }

        foreach (var pon in candidates.GetCandidates<PonCandidate>())
        {
            var postShanten = CalcCallSimulatedShanten(view, calls, CallType.Pon, pon.HandTiles, notification.DiscardedTile, notification.DiscarderIndex);
            if (postShanten < currentShanten)
            {
                return Task.FromResult<PlayerResponse>(new PonResponse(pon.HandTiles));
            }
        }

        foreach (var chi in candidates.GetCandidates<ChiCandidate>())
        {
            var postShanten = CalcCallSimulatedShanten(view, calls, CallType.Chi, chi.HandTiles, notification.DiscardedTile, notification.DiscarderIndex);
            if (postShanten < currentShanten)
            {
                return Task.FromResult<PlayerResponse>(new ChiResponse(chi.HandTiles));
            }
        }

        return Task.FromResult<PlayerResponse>(new OkResponse());
    }

    public override Task<PlayerResponse> OnKanAsync(KanNotification notification, CancellationToken ct = default)
    {
        var candidates = notification.CandidateList;
        if (candidates.HasCandidate<ChankanRonCandidate>())
        {
            return Task.FromResult<PlayerResponse>(new ChankanRonResponse());
        }
        if (candidates.HasCandidate<RonCandidate>())
        {
            return Task.FromResult<PlayerResponse>(new RonResponse());
        }
        return Task.FromResult<PlayerResponse>(new OkResponse());
    }

    public override Task<AfterKanTsumoResponse> OnKanTsumoAsync(KanTsumoNotification notification, CancellationToken ct = default)
    {
        EnsureRules();
        evaluator_.ClearEvalOnly();
        var candidates = notification.CandidateList;
        if (candidates.HasCandidate<RinshanTsumoAgariCandidate>())
        {
            return Task.FromResult<AfterKanTsumoResponse>(new RinshanTsumoResponse());
        }

        var view = notification.View;
        var currentShanten = CalcYakuAwareShanten(view.OwnHand, GetCalls(view), view);

        if (!ShouldSkipKanDueToRiichi(view, currentShanten))
        {
            var ankanCall = TryFindAnkanSame(view, candidates, currentShanten);
            if (ankanCall is not null)
            {
                return Task.FromResult<AfterKanTsumoResponse>(new KanTsumoAnkanResponse(ankanCall));
            }

            var kakanCall = TryFindKakanSame(view, candidates, currentShanten);
            if (kakanCall is not null)
            {
                return Task.FromResult<AfterKanTsumoResponse>(new KanTsumoKakanResponse(kakanCall));
            }
        }

        var dahaiCandidate = candidates.GetCandidates<DahaiCandidate>().FirstOrDefault()
            ?? throw new InvalidOperationException("嶺上ツモフェーズに DahaiCandidate が提示されませんでした。");

        var chosen = SelectDahai(view, dahaiCandidate.DahaiOptionList);
        return Task.FromResult<AfterKanTsumoResponse>(new KanTsumoDahaiResponse(chosen.Tile, chosen.RiichiAvailable));
    }

    // ================= 打牌選択 =================

    private DahaiOption SelectDahai(PlayerRoundView view, DahaiOptionList options)
    {
        if (options.Count == 0)
        {
            throw new InvalidOperationException("DahaiCandidate の選択肢が空です。");
        }

        if (!HasActiveRiichiOpponent(view))
        {
            return SelectAttackDahai(view, options);
        }

        var calls = GetCalls(view);
        var shanten = CalcYakuAwareShanten(view.OwnHand, calls, view);
        if (shanten <= 0)
        {
            return SelectAttackDahai(view, options);
        }

        if (shanten == 1)
        {
            var attackDahai = SelectAttackDahai(view, options);
            var attackDanger = DangerEvaluator.CalcDanger(attackDahai.Tile, view);
            if (attackDanger <= MAWASHI_DANGER_THRESHOLD)
            {
                return attackDahai;
            }
        }

        return SelectSafestDahai(view, options);
    }

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

    private DahaiOption SelectAttackDahai(PlayerRoundView view, DahaiOptionList options)
    {
        var hand14 = view.OwnHand;
        var calls = GetCalls(view);
        var roundWindIndex = view.RoundWind.Value;
        var seatWindIndex = CalcSeatWindIndex(view);
        var rules = rules_!;

        var yakuShanten = YakuAwareShantenHelper.Calc(hand14, calls, rules, roundWindIndex, seatWindIndex);
        return yakuShanten <= EVALUATION_SHANTEN_THRESHOLD
            ? SelectDahaiByHandShapeEvaluation(view, options, hand14, calls)
            : SelectDahaiByUsefulTileScore(view, options, hand14, calls, roundWindIndex, seatWindIndex, rules);
    }

    private DahaiOption SelectDahaiByHandShapeEvaluation(
        PlayerRoundView view, DahaiOptionList options, Hands.Hand hand14, CallList calls)
    {
        var sw = SlowDecisionLogThresholdMs > 0 ? System.Diagnostics.Stopwatch.StartNew() : null;
        if (sw is not null) { evaluator_.ResetCounters(); }
        var ctx = BuildContext(view, calls);
        var meldCount = calls.Count;
        var shanten14 = ShantenHelper.CalcShanten(hand14, meldCount);

        var scored = new List<(DahaiOption Option, long Score)>();
        var backtrackOptions = new List<DahaiOption>();

        foreach (var opt in options)
        {
            var hand13 = hand14.RemoveTile(opt.Tile);
            var shanten13 = ShantenHelper.CalcShanten(hand13, meldCount);
            if (shanten13 > shanten14)
            {
                backtrackOptions.Add(opt);
            }
            else
            {
                var ev = evaluator_.EvaluateHand13(hand13, back: null, ctx);
                scored.Add((opt, ev));
            }
        }

        var baselineMax = scored.Count > 0 ? scored.Max(x => x.Score) : 0L;
        foreach (var opt in backtrackOptions)
        {
            var hand13 = hand14.RemoveTile(opt.Tile);
            var ev = evaluator_.EvaluateBacktrack(hand13, opt.Tile.Kind, minEvPerTile: baselineMax * 2, ctx);
            scored.Add((opt, ev));
        }

        var maxScore = scored.Max(x => x.Score);
        var finalists = scored
            .Where(x => x.Score == maxScore)
            .Select(x => x.Option)
            .ToList();
        var chosen = finalists[rng_.Next(finalists.Count)];

        if (sw is not null)
        {
            sw.Stop();
            if (sw.ElapsedMilliseconds >= SlowDecisionLogThresholdMs)
            {
                var freq = System.Diagnostics.Stopwatch.Frequency;
                double ToMs(long t)
                {
                    return t * 1000.0 / freq;
                }

                Console.WriteLine(
                    $"[v0.6.1 slow-dahai-eval] {sw.ElapsedMilliseconds}ms shanten14={shanten14} options={options.Count} " +
                    $"handScore={evaluator_.CalcHandScoreCount}(hcalc={ToMs(evaluator_.HandCalcTicks):F0}ms) " +
                    $"h13={evaluator_.EvaluateHand13Count} h14={evaluator_.EvaluateHand14Count} " +
                    $"fulou={evaluator_.FulouCallCount}({ToMs(evaluator_.FulouTicks):F0}ms) " +
                    $"shantenMiss={evaluator_.ShantenCallCount}({ToMs(evaluator_.ShantenTicks):F0}ms) " +
                    $"usefulMiss={evaluator_.UsefulCallCount}({ToMs(evaluator_.UsefulTicks):F0}ms)");
            }
        }
        return chosen;
    }

    private DahaiOption SelectDahaiByUsefulTileScore(
        PlayerRoundView view, DahaiOptionList options, Hands.Hand hand14, CallList calls,
        int roundWindIndex, int seatWindIndex, GameRules rules)
    {
        var tileWeights = TileWeights.Build(hand14, calls);

        var shantenCache = new Dictionary<TileKind, int>();
        var evCache = new Dictionary<TileKind, int>();

        int GetShanten(Hands.Hand hand13, TileKind kind)
        {
            if (shantenCache.TryGetValue(kind, out var cached)) { return cached; }
            var value = YakuAwareShantenHelper.Calc(hand13, calls, rules, roundWindIndex, seatWindIndex);
            shantenCache[kind] = value;
            return value;
        }

        int GetEvaluation(Hands.Hand hand13, TileKind kind)
        {
            if (evCache.TryGetValue(kind, out var cached)) { return cached; }
            var useful = YakuAwareShantenHelper.EnumerateUsefulTileKindsWithCallMark(
                hand13, calls, rules, roundWindIndex, seatWindIndex);
            var score = 0;
            foreach (var (K, mark) in useful)
            {
                var multiplier = mark switch
                {
                    CallMark.Pon => 4,
                    CallMark.Chi => 2,
                    _ => 1,
                };
                score += VisibleTileCounter.CountUnseen(view, K) * multiplier * tileWeights.Of(K);
            }
            evCache[kind] = score;
            return score;
        }

        var evaluated = options
            .Select(x => (Option: x, Hand13: hand14.RemoveTile(x.Tile)))
            .Select(x => (x.Option, x.Hand13, Shanten: GetShanten(x.Hand13, x.Option.Tile.Kind)))
            .ToList();
        var minShanten = evaluated.Min(x => x.Shanten);
        var scored = evaluated
            .Where(x => x.Shanten == minShanten)
            .Select(x => (x.Option, Score: GetEvaluation(x.Hand13, x.Option.Tile.Kind)))
            .ToList();
        var maxScore = scored.Max(x => x.Score);
        var finalists = scored
            .Where(x => x.Score == maxScore)
            .Select(x => x.Option)
            .ToList();

        return finalists[rng_.Next(finalists.Count)];
    }

    private HandShapeEvaluatorContext BuildContext(PlayerRoundView view, CallList calls, string? backMarker = null)
    {
        var roundWindIndex = view.RoundWind.Value;
        var seatWindIndex = CalcSeatWindIndex(view);
        var doraIndicatorKinds = view.DoraIndicators
            .Select(t => t.Kind)
            .ToImmutableArray();

        var unseenByKind = new int[TileKind.All.Count];
        for (var i = 0; i < unseenByKind.Length; i++)
        {
            unseenByKind[i] = VisibleTileCounter.CountUnseen(view, TileKind.All[i]);
        }
        int GetUnseenFast(TileKind k)
        {
            return unseenByKind[k.Value];
        }

        return new HandShapeEvaluatorContext(
            Rules: rules_!,
            RoundWindIndex: roundWindIndex,
            SeatWindIndex: seatWindIndex,
            RoundWind: (Wind)roundWindIndex,
            PlayerWind: (Wind)seatWindIndex,
            DoraIndicatorKinds: doraIndicatorKinds,
            Calls: calls,
            GetUnseen: GetUnseenFast,
            TileWeights: TileWeights.Build(view.OwnHand, calls),
            BackMarker: backMarker);
    }

    private PlayerResponse? SelectFulouByEvaluation(
        PlayerRoundView view,
        CandidateList candidates,
        DahaiNotification notification,
        CallList calls)
    {
        var sw = SlowDecisionLogThresholdMs > 0 ? System.Diagnostics.Stopwatch.StartNew() : null;
        var hand13 = view.OwnHand;
        var preFulouShanten = ShantenHelper.CalcShanten(hand13, calls.Count);
        var noFulouCtx = BuildContext(view, calls, backMarker: "");
        var noFulouEv = evaluator_.EvaluateHand13(hand13, back: null, noFulouCtx);

        var bestFulouEv = noFulouEv;
        PlayerResponse? bestResponse = null;

        foreach (var daiminkan in candidates.GetCandidates<DaiminkanCandidate>())
        {
            var ev = EvaluateFulouCandidate(
                view, calls, hand13, daiminkan.HandTiles, CallType.Daiminkan,
                notification.DiscardedTile, notification.DiscarderIndex, preFulouShanten);
            if (ev > bestFulouEv)
            {
                bestFulouEv = ev;
                bestResponse = new DaiminkanResponse(daiminkan.HandTiles);
            }
        }
        foreach (var pon in candidates.GetCandidates<PonCandidate>())
        {
            var ev = EvaluateFulouCandidate(
                view, calls, hand13, pon.HandTiles, CallType.Pon,
                notification.DiscardedTile, notification.DiscarderIndex, preFulouShanten);
            if (ev > bestFulouEv)
            {
                bestFulouEv = ev;
                bestResponse = new PonResponse(pon.HandTiles);
            }
        }
        foreach (var chi in candidates.GetCandidates<ChiCandidate>())
        {
            var ev = EvaluateFulouCandidate(
                view, calls, hand13, chi.HandTiles, CallType.Chi,
                notification.DiscardedTile, notification.DiscarderIndex, preFulouShanten);
            if (ev > bestFulouEv)
            {
                bestFulouEv = ev;
                bestResponse = new ChiResponse(chi.HandTiles);
            }
        }

        if (sw is not null)
        {
            sw.Stop();
            if (sw.ElapsedMilliseconds >= SlowDecisionLogThresholdMs)
            {
                var daiminkanCount = candidates.GetCandidates<DaiminkanCandidate>().Count();
                var ponCount = candidates.GetCandidates<PonCandidate>().Count();
                var chiCount = candidates.GetCandidates<ChiCandidate>().Count();
                Console.WriteLine($"[v0.6.1 slow-fulou-eval] {sw.ElapsedMilliseconds}ms preShanten={preFulouShanten} dmk={daiminkanCount} pon={ponCount} chi={chiCount} calls={calls.Count} adopted={bestResponse?.GetType().Name ?? "pass"}");
            }
        }
        return bestResponse;
    }

    private long EvaluateFulouCandidate(
        PlayerRoundView view,
        CallList calls,
        Hands.Hand hand13,
        ImmutableArray<Tile> handTiles,
        CallType type,
        Tile discardedTile,
        PlayerIndex discarderIndex,
        int preFulouShanten)
    {
        var handAfter = hand13;
        foreach (var tile in handTiles)
        {
            handAfter = handAfter.RemoveTile(tile);
        }
        var callTiles = handTiles.Add(discardedTile).ToImmutableList();
        var virtualCall = new Call(type, callTiles, discarderIndex, discardedTile);
        var callsAfter = calls.Add(virtualCall);

        var postMeldCount = callsAfter.Count;
        var postShanten = ShantenHelper.CalcShanten(handAfter, postMeldCount);
        if (postShanten >= preFulouShanten) { return 0; }

        var basePostCtx = BuildContext(view, callsAfter, backMarker: "");
        var postCtx = basePostCtx with
        {
            TileWeights = TileWeights.Build(handAfter, callsAfter),
        };

        long evMax = 0;
        var seenKinds = new HashSet<int>();
        foreach (var tile in handAfter)
        {
            if (!seenKinds.Add(tile.Kind.Value)) { continue; }
            var postDahai = handAfter.RemoveTile(tile);
            var shantenAfterDahai = ShantenHelper.CalcShanten(postDahai, postMeldCount);
            if (shantenAfterDahai > postShanten) { continue; }
            var ev = evaluator_.EvaluateHand13(postDahai, back: null, postCtx);
            if (ev > evMax) { evMax = ev; }
        }
        return evMax;
    }

    // ================= 副露 / 槓 シミュレーション =================

    private int CalcCallSimulatedShanten(
        PlayerRoundView view, CallList calls, CallType type, ImmutableArray<Tile> handTiles,
        Tile dahaiTile, PlayerIndex discarderIndex)
    {
        var newHand = view.OwnHand;
        foreach (var tile in handTiles)
        {
            newHand = newHand.RemoveTile(tile);
        }
        var callTiles = handTiles.Add(dahaiTile).ToImmutableList();
        var virtualCall = new Call(type, callTiles, discarderIndex, dahaiTile);
        var newCalls = calls.Add(virtualCall);
        return YakuAwareShantenHelper.Calc(
            newHand,
            newCalls,
            rules_!,
            view.RoundWind.Value,
            CalcSeatWindIndex(view));
    }

    private Tile? TryFindAnkanSame(PlayerRoundView view, CandidateList candidates, int currentShanten)
    {
        var calls = GetCalls(view);
        foreach (var ankan in candidates.GetCandidates<AnkanCandidate>())
        {
            var newHand = view.OwnHand;
            foreach (var tile in ankan.Tiles) { newHand = newHand.RemoveTile(tile); }
            var virtualCall = new Call(CallType.Ankan, [.. ankan.Tiles], view.ViewerIndex, null);
            var newCalls = calls.Add(virtualCall);
            var postShanten = YakuAwareShantenHelper.Calc(
                newHand,
                newCalls,
                rules_!,
                view.RoundWind.Value,
                CalcSeatWindIndex(view));
            if (postShanten == currentShanten)
            {
                return ankan.Tiles[0];
            }
        }
        return null;
    }

    private Tile? TryFindKakanSame(PlayerRoundView view, CandidateList candidates, int currentShanten)
    {
        var calls = GetCalls(view);
        foreach (var kakan in candidates.GetCandidates<KakanCandidate>())
        {
            var newHand = view.OwnHand.RemoveTile(kakan.Tile);
            var originalPon = calls.FirstOrDefault(c => c.Type == CallType.Pon && c.Tiles[0].Kind == kakan.Tile.Kind);
            if (originalPon is null) { continue; }

            var newTiles = originalPon.Tiles.Add(kakan.Tile);
            var replacedCall = new Call(CallType.Kakan, newTiles, originalPon.From, originalPon.CalledTile);
            var newCalls = calls.Replace(originalPon, replacedCall);
            var postShanten = YakuAwareShantenHelper.Calc(
                newHand,
                newCalls,
                rules_!,
                view.RoundWind.Value,
                CalcSeatWindIndex(view)
            );
            if (postShanten == currentShanten)
            {
                return kakan.Tile;
            }
        }
        return null;
    }

    private static bool ShouldSkipKanDueToRiichi(PlayerRoundView view, int currentShanten)
    {
        return HasActiveRiichiOpponent(view) && currentShanten > 0;
    }

    private static bool HasActiveRiichiOpponent(PlayerRoundView view)
    {
        foreach (var status in view.OtherPlayerStatuses)
        {
            if (status.IsRiichi)
            {
                return true;
            }
        }
        return false;
    }

    // ================= ユーティリティ =================

    private int CalcYakuAwareShanten(Hands.Hand hand, CallList calls, PlayerRoundView view)
    {
        return YakuAwareShantenHelper.Calc(
            hand, calls, rules_!, view.RoundWind.Value, CalcSeatWindIndex(view));
    }

    private static int CalcSeatWindIndex(PlayerRoundView view)
    {
        var dealerIndex = view.RoundNumber.ToDealer();
        return (view.ViewerIndex.Value - dealerIndex.Value + PlayerIndex.PLAYER_COUNT) % PlayerIndex.PLAYER_COUNT;
    }

    private static CallList GetCalls(PlayerRoundView view)
    {
        return view.CallListArray[view.ViewerIndex];
    }

    private void EnsureRules()
    {
        if (rules_ is null)
        {
            throw new InvalidOperationException("OnGameStartAsync 未受信のまま AI が呼ばれました。");
        }
    }
}

/// <summary>
/// <see cref="AI_v0_6_1_手作り"/> を席順ごとに生成する <see cref="IPlayerFactory"/>。
/// </summary>
public sealed class AI_v0_6_1_手作りFactory(int seed)
    : AiPlayerFactoryBase<AI_v0_6_1_手作り>(seed, AI_v0_6_1_手作り.DISPLAY_NAME)
{
    protected override AI_v0_6_1_手作り CreatePlayer(PlayerId id, PlayerIndex index, Random rng)
    {
        return new(id, index, rng);
    }
}
