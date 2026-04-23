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
/// ver.0.6.0 手作り — 牌姿の評価値 (和了打点ベース) で打牌・副露・槓を選ぶ AI。
/// v0.5.0 の鳴き判定と v0.4.0 の回し打ち守備を基盤に、役ありシャンテン ≤ 2 の局面では
/// HandCalculator の実打点をもとにした再帰評価値 (0301)・シャンテン戻し (0302)・
/// 副露評価値 (0303)・一色手/大三元/四喜和狙い (0305) を統合して打牌を選択する。
/// 役ありシャンテン ≥ 3 では v0.5.0 相当のフォールバック。
/// </summary>
public sealed class AI_v0_6_0_手作り(
    PlayerId playerId,
    PlayerIndex playerIndex,
    Random rng
) : Player(playerId, DISPLAY_NAME, playerIndex)
{
    public const string DISPLAY_NAME = "ver.0.6.0 手作り";

    /// <summary>
    /// 1 シャンテン時に回し打ちするかベタオリするかの閾値 (v0.4.0 と同じ)
    /// </summary>
    private const int MAWASHI_DANGER_THRESHOLD = 5;

    /// <summary>
    /// 評価値ベースの打牌選択 / 副露判断に切り替える役ありシャンテンの閾値。
    /// この値 **以下** のとき HandCalculator を用いた再帰評価値を使う。
    /// これより大きければ v0.5.0 相当のシャンテン + 副露マーク付き有効牌評価にフォールバック。
    /// </summary>
    private const int EVALUATION_SHANTEN_THRESHOLD = 2;

    private readonly Random rng_ = rng;
    private readonly HandShapeEvaluator evaluator_ = new();
    private GameRules? rules_;

    /// <summary>
    /// 遅い決定を特定するための計測ログしきい値。これを超えた決定は Console に出力する。
    /// 0 以下で無効化。開発時のみ使用。既定値 0 (無効)、診断時に呼び出し側から設定する。
    /// <c>internal</c> にして外部アセンブリから触れないようにする (並列 AutoPlay で途中変更すると全 worker に波及するため)。
    /// </summary>
    internal static int SlowDecisionLogThresholdMs = 0;

    public override Task<OkResponse> OnGameStartAsync(GameStartNotification notification, CancellationToken ct = default)
    {
        rules_ = notification.Rules;
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnRoundStartAsync(RoundStartNotification notification, CancellationToken ct = default)
    {
        // 局開始時は shanten / useful キャッシュまで含めて全クリア (次局の hand-sig は前局と独立のため)
        evaluator_.ClearBetweenRounds();
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnRoundEndAsync(RoundEndNotification notification, CancellationToken ct = default)
    {
        // 局終了時も全クリアしてメモリを解放 (Game 単位で局が 4〜16 重なっても上限を局内サイズに抑える)
        evaluator_.ClearBetweenRounds();
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnGameEndAsync(GameEndNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnHaipaiAsync(HaipaiNotification notification, CancellationToken ct = default)
    {
        // 配牌は局開始後の新しい hand-sig なので全キャッシュクリア
        evaluator_.ClearBetweenRounds();
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnOtherPlayerTsumoAsync(OtherPlayerTsumoNotification notification, CancellationToken ct = default)
    {
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnCallAsync(CallNotification notification, CancellationToken ct = default)
    {
        // 副露で自分または他家の Calls / 見える牌が変わり、メンゼン状態と打点計算が影響される
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
        // 副露後は非門前のため立直不可
        return Task.FromResult(new DahaiResponse(chosen.Tile));
    }

    public override Task<OkResponse> OnOtherPlayerAfterCallAsync(OtherPlayerAfterCallNotification notification, CancellationToken ct = default)
    {
        // 他家の副露後打牌で見える牌が変わる → 評価値キャッシュ無効化
        evaluator_.ClearEvalOnly();
        return Task.FromResult(new OkResponse());
    }

    public override Task<OkResponse> OnDoraRevealAsync(DoraRevealNotification notification, CancellationToken ct = default)
    {
        // ドラ表示が増えると和了打点が変わる → 両方クリア
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
        // 自分のツモで未見枚数が 1 枚減るため評価値キャッシュを無効化 (和了打点は不変なので handScore キャッシュは維持)
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
            // 暗槓チェック
            var ankanCall = TryFindAnkanSame(view, candidates, currentShanten);
            if (ankanCall is not null)
            {
                return Task.FromResult<AfterTsumoResponse>(new AnkanResponse(ankanCall));
            }

            // 加槓チェック
            var kakanCall = TryFindKakanSame(view, candidates, currentShanten);
            if (kakanCall is not null)
            {
                return Task.FromResult<AfterTsumoResponse>(new KakanResponse(kakanCall));
            }
        }

        // 打牌
        var dahaiCandidate = candidates.GetCandidates<DahaiCandidate>().FirstOrDefault()
            ?? throw new InvalidOperationException("ツモフェーズに DahaiCandidate が提示されませんでした。");
        var chosen = SelectDahai(view, dahaiCandidate.DahaiOptionList);
        return Task.FromResult<AfterTsumoResponse>(new DahaiResponse(chosen.Tile, chosen.RiichiAvailable));
    }

    public override Task<PlayerResponse> OnDahaiAsync(DahaiNotification notification, CancellationToken ct = default)
    {
        EnsureRules();
        // 他家打牌で未見枚数が 1 枚減る → 評価値キャッシュ無効化
        evaluator_.ClearEvalOnly();
        var candidates = notification.CandidateList;
        if (candidates.HasCandidate<RonCandidate>())
        {
            return Task.FromResult<PlayerResponse>(new RonResponse());
        }

        var view = notification.View;
        var currentShanten = CalcYakuAwareShanten(view.OwnHand, GetCalls(view), view);

        // テンパイ時は副露しない
        if (currentShanten <= 0)
        {
            return Task.FromResult<PlayerResponse>(new OkResponse());
        }

        // 他家リーチ & シャンテン > 1 ならベタオリ継続 (副露しない)
        if (HasActiveRiichiOpponent(view) && currentShanten > 1)
        {
            return Task.FromResult<PlayerResponse>(new OkResponse());
        }

        var calls = GetCalls(view);

        // 役ありシャンテン ≤ 2 は評価値ベース副露判定 (0304)
        if (currentShanten <= EVALUATION_SHANTEN_THRESHOLD)
        {
            var evalResponse = SelectFulouByEvaluation(view, candidates, notification, calls);
            if (evalResponse is not null)
            {
                return Task.FromResult(evalResponse);
            }
            return Task.FromResult<PlayerResponse>(new OkResponse());
        }

        // 大明槓はシャンテン『維持』で採用。ポン/チーは『進行』で採用する非対称な判定:
        // 大明槓は成立と同時に嶺上ツモが得られ、かつ新ドラ表示の機会もあるため、
        // シャンテン維持でも +1 ツモ + ドラ期待分だけ得と見積もれる。
        foreach (var daiminkan in candidates.GetCandidates<DaiminkanCandidate>())
        {
            var postShanten = CalcCallSimulatedShanten(view, calls, CallType.Daiminkan, daiminkan.HandTiles, notification.DiscardedTile, notification.DiscarderIndex);
            if (postShanten == currentShanten)
            {
                return Task.FromResult<PlayerResponse>(new DaiminkanResponse(daiminkan.HandTiles));
            }
        }

        // ポン: 役ありシャンテンが進むなら採用
        foreach (var pon in candidates.GetCandidates<PonCandidate>())
        {
            var postShanten = CalcCallSimulatedShanten(view, calls, CallType.Pon, pon.HandTiles, notification.DiscardedTile, notification.DiscarderIndex);
            if (postShanten < currentShanten)
            {
                return Task.FromResult<PlayerResponse>(new PonResponse(pon.HandTiles));
            }
        }

        // チー: 役ありシャンテンが進むなら採用
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
        // 槍槓 (ChankanRonCandidate) は ChankanRonResponse でしか候補に一致しない。
        // RonResponse を返すと ResponseValidator で候補外扱いされ OkResponse に差し替えられるため、
        // 槍槓と通常ロンは明示的に分岐する。
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
        // 嶺上ツモで未見枚数が変わり、新ドラ表示は OnDoraRevealAsync で別途クリアされる
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

    /// <summary>
    /// 回し打ちロジック (v0.4.0 から移植、シャンテン評価を役ありに置換)
    /// </summary>
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

    /// <summary>
    /// 攻撃打牌選択。役ありシャンテン ≤ <see cref="EVALUATION_SHANTEN_THRESHOLD"/> なら牌姿評価値ベース (0301+0302)、
    /// それ以上なら v0.5.0 相当の「役ありシャンテン + 副露マーク付き有効牌評価」にフォールバックする (0305 TileWeights 乗算)。
    /// </summary>
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

    /// <summary>
    /// 役ありシャンテン ≤ 2 のときの打牌選択 (0301 + 0302)。
    /// 各打牌候補の牌姿評価値を計算し、最大値の打牌を選ぶ。
    /// シャンテン戻し打牌は通常打牌最大値 × 2 の閾値を超えるもののみ候補に加える。
    /// </summary>
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
                    $"[v0.6.0 slow-dahai-eval] {sw.ElapsedMilliseconds}ms shanten14={shanten14} options={options.Count} " +
                    $"handScore={evaluator_.CalcHandScoreCount}(hcalc={ToMs(evaluator_.HandCalcTicks):F0}ms) " +
                    $"h13={evaluator_.EvaluateHand13Count} h14={evaluator_.EvaluateHand14Count} " +
                    $"fulou={evaluator_.FulouCallCount}({ToMs(evaluator_.FulouTicks):F0}ms) " +
                    $"shantenMiss={evaluator_.ShantenCallCount}({ToMs(evaluator_.ShantenTicks):F0}ms) " +
                    $"usefulMiss={evaluator_.UsefulCallCount}({ToMs(evaluator_.UsefulTicks):F0}ms)");
            }
        }
        return chosen;
    }

    /// <summary>
    /// 役ありシャンテン &gt; 2 のときの打牌選択 (v0.5.0 相当フォールバック + 0305 TileWeights 乗算)。
    /// </summary>
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

    /// <summary>
    /// 現在の <see cref="PlayerRoundView"/> から <see cref="HandShapeEvaluatorContext"/> を構築する。
    /// 自風・場風は Wind enum (0=East, 1=South, ...) に直接キャストできる前提。
    /// </summary>
    /// <remarks>
    /// <paramref name="view"/> の未見枚数を 34 種分まとめて事前計算し、決定中に数百回呼ばれる
    /// <see cref="HandShapeEvaluatorContext.GetUnseen"/> を配列参照で返すクロージャにすることで、
    /// 評価値計算中の VisibleTileCounter 呼び出し (各回 O(河 + 副露 + 手牌)) を消す。
    /// </remarks>
    private HandShapeEvaluatorContext BuildContext(PlayerRoundView view, CallList calls, string? backMarker = null)
    {
        var roundWindIndex = view.RoundWind.Value;
        var seatWindIndex = CalcSeatWindIndex(view);
        var doraIndicatorKinds = view.DoraIndicators
            .Select(t => t.Kind)
            .ToImmutableArray();

        // 未見枚数を 34 種分まとめて 1 回だけ計算する
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

    /// <summary>
    /// 役ありシャンテン ≤ 2 のときの評価値ベース副露判定 (0304)。
    /// 副露しない場合の評価値と、各副露候補 (大明槓/ポン/チー) の副露後評価値を比較し、
    /// 最大評価値の行動を選択する。副露が勝らなければ null を返す (呼び出し側で OK 応答とする)。
    /// </summary>
    private PlayerResponse? SelectFulouByEvaluation(
        PlayerRoundView view,
        CandidateList candidates,
        DahaiNotification notification,
        CallList calls)
    {
        var sw = SlowDecisionLogThresholdMs > 0 ? System.Diagnostics.Stopwatch.StartNew() : null;
        var hand13 = view.OwnHand;
        var preFulouShanten = ShantenHelper.CalcShanten(hand13, calls.Count);
        // 副露判定時は有効牌 1 枚見逃しの影響で評価値が変わるため、別キャッシュ (BackMarker="") を使う
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
                Console.WriteLine($"[v0.6.0 slow-fulou-eval] {sw.ElapsedMilliseconds}ms preShanten={preFulouShanten} dmk={daiminkanCount} pon={ponCount} chi={chiCount} calls={calls.Count} adopted={bestResponse?.GetType().Name ?? "pass"}");
            }
        }
        return bestResponse;
    }

    /// <summary>
    /// 1 つの副露候補に対する副露後評価値を計算する。
    /// 副露後の手牌から 1 枚打牌して得られる 13 枚相当手牌の評価値の最大値を返す。
    /// シャンテンが進まない副露は 0 を返して採用を避ける。
    /// </summary>
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
        // 副露後の手牌と仮想 Call
        var handAfter = hand13;
        foreach (var tile in handTiles)
        {
            handAfter = handAfter.RemoveTile(tile);
        }
        var callTiles = handTiles.Add(discardedTile).ToImmutableList();
        var virtualCall = new Call(type, callTiles, discarderIndex, discardedTile);
        var callsAfter = calls.Add(virtualCall);

        // シャンテンが進まない副露は採用しない
        var postMeldCount = callsAfter.Count;
        var postShanten = ShantenHelper.CalcShanten(handAfter, postMeldCount);
        if (postShanten >= preFulouShanten) { return 0; }

        // 副露後コンテキスト (副露判定なので BackMarker="") — BuildContext を流用して未見枚数配列を共有する
        var basePostCtx = BuildContext(view, callsAfter, backMarker: "");
        var postCtx = basePostCtx with
        {
            TileWeights = TileWeights.Build(handAfter, callsAfter),
        };

        // 副露直後の手牌は打牌可能 (手牌 3n+2 + 副露 = 計 14 枚相当)。
        // 各打牌候補 (シャンテン戻さないもの) の EvaluateHand13 の max を取る
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

    /// <summary>
    /// 手牌 <paramref name="handTiles"/> と打牌 <paramref name="dahaiTile"/> で副露した後の
    /// 役ありシャンテンを計算する。副露後の手牌は <paramref name="handTiles"/> の牌を除去、
    /// 仮想 Call (type, handTiles + dahaiTile, discarderIndex, dahaiTile) を追加した状態。
    /// </summary>
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

    /// <summary>
    /// 暗槓候補のうち、役ありシャンテンを維持できるものを探す。
    /// </summary>
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

    /// <summary>
    /// 加槓候補のうち、役ありシャンテンを維持できるものを探す。加槓は手牌の 1 枚を取り込み、既存のポンを槓に変換する。
    /// ここではシャンテン計算上、当該牌種のポンを暗槓として扱う近似で十分 (4 枚目の牌による実装差は shanten に影響しない)。
    /// </summary>
    private Tile? TryFindKakanSame(PlayerRoundView view, CandidateList candidates, int currentShanten)
    {
        var calls = GetCalls(view);
        foreach (var kakan in candidates.GetCandidates<KakanCandidate>())
        {
            var newHand = view.OwnHand.RemoveTile(kakan.Tile);
            // 対応するポンを探して大明槓扱いに置換したシャンテン計算を行う
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
        // 他家リーチ中かつ非テンパイならカンしない (テンパイでないと守備的にカンを避ける)
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
/// <see cref="AI_v0_6_0_手作り"/> を席順ごとに生成する <see cref="IPlayerFactory"/>。
/// </summary>
public sealed class AI_v0_6_0_手作りFactory(int seed)
    : AiPlayerFactoryBase<AI_v0_6_0_手作り>(seed, AI_v0_6_0_手作り.DISPLAY_NAME)
{
    protected override AI_v0_6_0_手作り CreatePlayer(PlayerId id, PlayerIndex index, Random rng)
    {
        return new(id, index, rng);
    }
}
