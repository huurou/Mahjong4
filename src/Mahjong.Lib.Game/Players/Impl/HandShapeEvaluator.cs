using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.HandCalculating;
using Mahjong.Lib.Scoring.Shantens;
using System.Collections.Immutable;
using GameRules = Mahjong.Lib.Game.Games.GameRules;
using Hand = Mahjong.Lib.Game.Hands.Hand;
using ScoringConversions = Mahjong.Lib.Game.Games.ScoringConversions;
using TileKind = Mahjong.Lib.Scoring.Tiles.TileKind;

namespace Mahjong.Lib.Game.Players.Impl;

/// <summary>
/// 牌姿の評価値計算器 (kobalab/majiang-ai の 0301〜0305 相当)。
/// v0.6.0 手作り AI の中核ロジックで、ツモ前提・メンゼンならリーチ付与・一発/裏ドラ無しで
/// <see cref="HandCalculator.Calc"/> を呼んで和了打点を求め、打牌・副露の意思決定に使う。
/// 構成: <see cref="CalcHandScore"/> (和了打点) + <see cref="EvaluateHand14"/> / <see cref="EvaluateHand13"/>
/// の再帰評価値 (0301) + <see cref="EvaluateBacktrack"/> のシャンテン戻し (0302) + <see cref="EvaluateFulouForTile"/>
/// の副露評価値 (0303) + <see cref="TileWeights"/> 経由の 0305 染め/三元/四喜重み。
/// </summary>
internal sealed class HandShapeEvaluator
{
    /// <summary>
    /// 役ありシャンテンの再帰計算打ち切り閾値。これ以上は HandCalculator 呼び出しが爆発するのでフォールバック。
    /// </summary>
    private const int RECURSION_CUTOFF_SHANTEN = 3;

    /// <summary>
    /// 和了打点キャッシュ。(hand14 署名, 赤ドラ数, 和了牌 Kind, メンゼン) で一意。
    /// 局中のドラ開示・副露等で Calls 自体や DoraIndicator が変わるため、その際は <see cref="ClearAll"/> を呼ぶ。
    /// </summary>
    private readonly Dictionary<HandScoreKey, int> handScoreCache_ = [];

    /// <summary>
    /// 牌姿評価値キャッシュ (0301)。(handSignature, calls, backMarker) で一意。
    /// hand13/hand14 両方で共有する (枚数が違えば Signature が異なるので衝突しない)。
    /// 自分のツモ・他家の打牌・副露・開槓でクリアする。
    /// </summary>
    private readonly Dictionary<EvalKey, long> evalCache_ = [];

    /// <summary>
    /// <see cref="EvaluateFulouForTile"/> の結果キャッシュ。(hand13Sig, pKind, ctx.CallsSignature, akadora) で一意。
    /// ctx.GetUnseen 等も結果に影響するため <see cref="ClearEvalOnly"/> で <see cref="evalCache_"/> と一緒にクリアする。
    /// akadora を鍵に含めるのは、<see cref="HandSignature"/> / <see cref="CallsSignature"/> が意図的に赤黒を
    /// 区別しない (34 牌種) ため、そのままでは赤 5 含む hand13 と含まない hand13 が同じ fulouKey に
    /// 衝突してしまうためのフィールド。
    /// </summary>
    private readonly Dictionary<(HandSignature Hand13, int P, CallsSignature Calls, int Akadora), long> fulouCache_ = [];

    /// <summary>
    /// シャンテン数キャッシュ。<see cref="ShantenCalculator"/> は牌種 34 個分の面子探索を走らせるため重い。
    /// <see cref="EnumerateUsefulTileKinds"/> は内部で 34 回呼ぶため、再帰評価のボトルネックになりやすい。
    /// (HandSignature, meldCount) でキャッシュして再計算を避ける。未見枚数・ドラ・赤ドラとは独立。
    ///
    /// <b>AI インスタンス単位 + 局スコープ:</b> 以前は静的共有で全対局を跨いで恒久化していたが、
    /// 長時間 AutoPlay (数百局以上) でキャッシュが無制限に膨張し数 GB オーダーのメモリを消費して
    /// GC スラッシング / OOM を引き起こしたため、局単位の辞書に戻した。
    /// <see cref="ClearAll"/> (`OnRoundStart` / `OnHaipai` / `OnRoundEnd` で呼ばれる) でクリアする。
    /// 副露・ドラ開示・ツモ・打牌のタイミングではクリアしない (shanten は局面から独立した純粋関数のため)。
    /// </summary>
    private readonly Dictionary<(HandSignature Hand, int Meld), int> shantenCacheGlobal_ = [];

    /// <summary>
    /// 有効牌集合キャッシュ。<see cref="EvaluateHand13"/> の主たるループで使用し、
    /// 1 hand13 あたり最大 34 回の <see cref="ShantenCalculator.Calc"/> 呼び出しを 1 回に抑える。
    /// <see cref="shantenCacheGlobal_"/> と同じ局スコープ。
    /// </summary>
    private readonly Dictionary<(HandSignature Hand, int Meld), ImmutableHashSet<TileKind>> usefulCacheGlobal_ = [];

    /// <summary>
    /// 副露評価値の再帰ガード。<see cref="EvaluateFulouForTile"/> の内部から呼ばれる <see cref="EvaluateHand13"/> で
    /// 再度 <see cref="EvaluateFulouForTile"/> を呼ぶと、仮想副露が再帰のたびに別の <see cref="CallsSignature"/>
    /// を生成しキャッシュヒット率が下がり計算量爆発する。1 段目の副露評価でのみ有効な近似として、
    /// 再帰中は加算をスキップする (同一状態の再計算を打ち切るキャッシュ戦略に相当)。
    /// </summary>
    private bool inFulouRecursion_;

    // 副露評価のコスト切り分け用の一時計測
    internal long FulouCallCount { get; private set; }
    internal long FulouTicks { get; private set; }

    // 性能計測用カウンタ (実運用では影響しない整数インクリメントのみ)
    internal long CalcHandScoreCount { get; private set; }
    internal long EvaluateHand13Count { get; private set; }
    internal long EvaluateHand14Count { get; private set; }
    internal int HandScoreCacheSize => handScoreCache_.Count;
    internal int EvalCacheSize => evalCache_.Count;

    // 詳細ブレークダウン計測: 各処理の累積ティック数 (Stopwatch.GetTimestamp ベース)
    internal long ShantenCallCount { get; private set; }
    internal long ShantenTicks { get; private set; }
    internal long UsefulCallCount { get; private set; }
    internal long UsefulTicks { get; private set; }
    internal long HandCalcTicks { get; private set; }
    internal long SignatureTicks { get; private set; }
    internal long SignatureCount { get; private set; }

    internal void ResetCounters()
    {
        CalcHandScoreCount = 0;
        EvaluateHand13Count = 0;
        EvaluateHand14Count = 0;
        ShantenCallCount = 0;
        ShantenTicks = 0;
        UsefulCallCount = 0;
        UsefulTicks = 0;
        HandCalcTicks = 0;
        SignatureTicks = 0;
        SignatureCount = 0;
        FulouCallCount = 0;
        FulouTicks = 0;
    }

    /// <summary>
    /// 配牌時・ドラ開示時・副露時などに和了打点と評価値の両方のキャッシュをクリアする。
    /// シャンテン / 有効牌キャッシュは純粋関数なので頻繁にはクリアせず、局開始・終了 (および配牌) のみ
    /// 呼び出し側がクリアする。<see cref="ClearBetweenRounds"/> を参照。
    /// </summary>
    public void ClearAll()
    {
        handScoreCache_.Clear();
        evalCache_.Clear();
        fulouCache_.Clear();
    }

    /// <summary>
    /// 局の境界 (OnRoundStart / OnHaipai / OnRoundEnd) でシャンテン・有効牌キャッシュ含む全キャッシュをクリアする。
    /// 局内ではドラ開示・副露が走っても <see cref="shantenCacheGlobal_"/> / <see cref="usefulCacheGlobal_"/>
    /// は局面独立な純粋関数なのでクリアしない。
    /// </summary>
    public void ClearBetweenRounds()
    {
        handScoreCache_.Clear();
        evalCache_.Clear();
        fulouCache_.Clear();
        shantenCacheGlobal_.Clear();
        usefulCacheGlobal_.Clear();
    }

    /// <summary>
    /// 自分のツモ・他家の打牌など、未見枚数だけが変わるタイミングでの評価値キャッシュクリア。
    /// 和了打点は hand14 + 親子・自風・場風・ドラ表示牌が不変なら変わらないので温存する。
    /// シャンテンと有効牌は hand/meldCount のみで決まるため温存する。
    /// </summary>
    public void ClearEvalOnly()
    {
        evalCache_.Clear();
        fulouCache_.Clear();
    }

    /// <summary>
    /// 14 枚手牌 (winTile を含む) のツモ和了打点を計算する。
    /// メンゼンなら立直を付けて評価。一発・裏ドラは考慮しない。非メンゼンで役なしなら 0 を返す。
    /// </summary>
    /// <param name="hand14">winTile を含む 14 枚の手牌</param>
    /// <param name="winTile">和了牌の牌種</param>
    /// <param name="ctx">局面コンテキスト</param>
    /// <returns>ツモ和了の合計打点 (Score.Main + Score.Sub*2)。役なしなら 0</returns>
    public int CalcHandScore(Hand hand14, TileKind winTile, HandShapeEvaluatorContext ctx)
    {
        CalcHandScoreCount++;
        var isMenzen = IsMenzen(ctx.Calls);
        var akadoraCount = CountAkadora(hand14, ctx.Calls, ctx.Rules);
        var handSignature = HandSignature.FromHand(hand14);

        // Calls を鍵に含める: 仮想副露シミュレーション (EvaluateFulouCandidate 経由) では
        // Calls の違いで和了打点が変わるため、Calls 非参照だとキャッシュ衝突して別候補の打点が混入する。
        // 高速比較のため事前計算した CallsSignature を使う。
        var key = new HandScoreKey(handSignature, akadoraCount, winTile.Value, isMenzen, ctx.CallsSignature);
        if (handScoreCache_.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var winSituation = new WinSituation
        {
            IsTsumo = true,
            IsRiichi = isMenzen,
            PlayerWind = ctx.PlayerWind,
            RoundWind = ctx.RoundWind,
            AkadoraCount = akadoraCount,
        };

        var hcStart = System.Diagnostics.Stopwatch.GetTimestamp();
        var result = HandCalculator.Calc(
            tileKindList: ScoringConversions.ToScoringTileKindList(hand14),
            winTile: winTile,
            callList: ctx.ScoringCallList,
            doraIndicators: ctx.ScoringDoraIndicators,
            uradoraIndicators: null,
            winSituation: winSituation,
            gameRules: ctx.ScoringGameRules
        );
        HandCalcTicks += System.Diagnostics.Stopwatch.GetTimestamp() - hcStart;

        var score = result.ErrorMessage is not null
            ? 0
            : result.Score.Main + result.Score.Sub * 2;
        handScoreCache_[key] = score;
        return score;
    }

    /// <summary>
    /// 副露に暗槓以外が 1 つも含まれなければメンゼン。
    /// </summary>
    private static bool IsMenzen(CallList calls)
    {
        foreach (var call in calls)
        {
            if (call.Type != CallType.Ankan)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 手牌 + 副露に含まれる赤ドラ枚数を数える。
    /// </summary>
    private static int CountAkadora(Hand hand, CallList calls, GameRules rules)
    {
        var count = 0;
        foreach (var tile in hand)
        {
            if (rules.IsRedDora(tile)) { count++; }
        }
        foreach (var call in calls)
        {
            foreach (var tile in call.Tiles)
            {
                if (rules.IsRedDora(tile)) { count++; }
            }
        }
        return count;
    }

    /// <summary>
    /// 13 枚手牌 (打牌後) の評価値 (0301 + 0302)。
    /// 有効牌 K を引いた 14 枚手牌の評価値と K の未見枚数の積の総和に、
    /// シャンテン数に応じた補正係数 (tenpai=×18 / 1s=×3 / 2s=×1) を掛けて返す。
    /// 補正後の値は分母 216 を約分済の long スケール整数で、異なるシャンテン数間の比較に使える。
    /// 役ありシャンテン ≥ <see cref="RECURSION_CUTOFF_SHANTEN"/> の局面では再帰を打ち切り 0 を返す
    /// (v0.3.0 簡易評価値へのフォールバックは Phase F で接続する)。
    /// </summary>
    /// <param name="hand13">打牌後の 13 枚手牌</param>
    /// <param name="back">シャンテン戻しで切った牌種 (引き戻し除外・フリテン判定用、通常打牌時は null)</param>
    /// <param name="ctx">局面コンテキスト</param>
    public long EvaluateHand13(Hand hand13, TileKind? back, HandShapeEvaluatorContext ctx)
    {
        EvaluateHand13Count++;
        var signature = HandSignature.FromHand(hand13);
        var akadora = CountAkadora(hand13, ctx.Calls, ctx.Rules);
        var key = new EvalKey(signature, ctx.CallsSignature, ctx.BackMarker, back?.Value, akadora);
        // 副露評価中 (inFulouRecursion_=true) は eval_fulou 加算を意図的にスキップした値を計算する。
        // そのスキップ版を evalCache_ に保存すると、後続の通常経路 (guard=false) で同一 key に当たったとき
        // 加算スキップ版を誤って再利用してしまう。ガード時はキャッシュの取得・保存どちらも回避する。
        if (!inFulouRecursion_ && evalCache_.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var meldCount = ctx.Calls.Count;
        var shanten = GetShantenCached(hand13, meldCount);
        long result = 0;
        if (shanten < RECURSION_CUTOFF_SHANTEN)
        {
            // 書籍準拠 (v0.6.1): paijia は毎 hand13 + calls で再構築する。親から使い回すと、
            // 親 hand14 で成立していた染め条件が子 hand13 で外れるような打牌候補を誤判定する。
            var weights = TileWeights.Build(hand13, ctx.Calls);
            long sum = 0;
            var useful = GetUsefulCached(hand13, meldCount, shanten);
            foreach (var kind in useful)
            {
                if (back is not null && kind.Value == back.Value) { continue; }
                var unseen = ctx.GetUnseen(kind);
                if (unseen == 0) { continue; }
                var hand14 = hand13.AddTile(RepresentativeNonRedTile(kind));
                var ev = EvaluateHand14(hand14, kind, back, ctx);
                // 0303: シャンテン戻しでなく、かつテンパイしていないときは
                // 「他家打牌でのポン/チー成立による評価値」も加算する (書籍 eval_shoupai 13 枚分岐の挙動)。
                // 再帰ガード: 副露評価中の再帰呼び出しは加算をスキップし、
                // 仮想副露の連鎖でキャッシュヒット率が下がって計算量爆発するのを防ぐ。
                if (back is null && shanten > 0 && !inFulouRecursion_)
                {
                    FulouCallCount++;
                    var fStart = System.Diagnostics.Stopwatch.GetTimestamp();
                    ev += EvaluateFulouForTile(hand13, kind, ctx, shanten);
                    FulouTicks += System.Diagnostics.Stopwatch.GetTimestamp() - fStart;
                }
                // 書籍 0305 準拠: paijia (TileWeights) を乗算する (v0.6.1)
                sum += (long)unseen * ev * weights.Of(kind);
            }
            result = sum * ShantenCoefficient(shanten);
        }

        if (!inFulouRecursion_)
        {
            evalCache_[key] = result;
        }
        return result;
    }

    /// <summary>
    /// 14 枚手牌の評価値 (0301)。和了形なら <see cref="CalcHandScore"/>、そうでなければ
    /// 全打牌候補 (シャンテン戻らないもののみ) の <see cref="EvaluateHand13"/> の最大値を返す。
    /// </summary>
    /// <param name="hand14">14 枚の手牌</param>
    /// <param name="winCandidate">直近で引いた牌の牌種。和了形なら winTile として CalcHandScore に渡す</param>
    /// <param name="back">シャンテン戻しで切った牌種 (フリテン判定用)。和了牌が back と一致すればフリテンなので 0 を返す</param>
    /// <param name="ctx">局面コンテキスト</param>
    public long EvaluateHand14(Hand hand14, TileKind winCandidate, TileKind? back, HandShapeEvaluatorContext ctx)
    {
        EvaluateHand14Count++;
        var meldCount = ctx.Calls.Count;
        var shanten = GetShantenCached(hand14, meldCount);
        if (shanten == SHANTEN_AGARI)
        {
            // フリテン: シャンテン戻しで切った牌で和了するとフリテンなので評価しない
            if (back is not null && winCandidate.Value == back.Value)
            {
                return 0;
            }
            return CalcHandScore(hand14, winCandidate, ctx);
        }

        var signature = HandSignature.FromHand(hand14);
        var akadora = CountAkadora(hand14, ctx.Calls, ctx.Rules);
        var key = new EvalKey(signature, ctx.CallsSignature, ctx.BackMarker, back?.Value, akadora);
        // 副露評価中 (inFulouRecursion_=true) は加算スキップ版を計算するため、evalCache_ を経由しない
        if (!inFulouRecursion_ && evalCache_.TryGetValue(key, out var cached))
        {
            return cached;
        }

        long result = 0;
        if (shanten < RECURSION_CUTOFF_SHANTEN)
        {
            var seenKinds = new HashSet<int>();
            foreach (var tile in hand14)
            {
                if (!seenKinds.Add(tile.Kind.Value)) { continue; }
                var hand13 = hand14.RemoveTile(tile);
                var shanten13 = GetShantenCached(hand13, meldCount);
                if (shanten13 > shanten) { continue; }
                var ev = EvaluateHand13(hand13, back, ctx);
                if (ev > result) { result = ev; }
            }
        }

        if (!inFulouRecursion_)
        {
            evalCache_[key] = result;
        }
        return result;
    }

    /// <summary>
    /// シャンテン戻し打牌専用の評価値計算 (0302)。打牌した牌種 <paramref name="back"/> で引き戻しを除外し、
    /// フリテンを避け、評価値が閾値 <paramref name="minEvPerTile"/> 以下の有効牌は集計から除外する (枝刈り)。
    /// 戻り値は 13 枚と同じスケール (×18/×3/×1 補正済み)。
    /// </summary>
    /// <param name="hand13">シャンテン戻し打牌後の 13 枚手牌</param>
    /// <param name="back">シャンテン戻しで切った牌種</param>
    /// <param name="minEvPerTile">各有効牌の 14 枚評価値がこれ以下なら加算しない閾値 (書籍: 通常打牌最大 × 2)</param>
    /// <param name="ctx">局面コンテキスト</param>
    public long EvaluateBacktrack(Hand hand13, TileKind back, long minEvPerTile, HandShapeEvaluatorContext ctx)
    {
        var meldCount = ctx.Calls.Count;
        var shanten = GetShantenCached(hand13, meldCount);
        if (shanten >= RECURSION_CUTOFF_SHANTEN)
        {
            return 0;
        }

        // 書籍準拠 (v0.6.1): paijia は hand13 + calls から再構築する
        var weights = TileWeights.Build(hand13, ctx.Calls);
        long sum = 0;
        var useful = GetUsefulCached(hand13, meldCount, shanten);
        foreach (var kind in useful)
        {
            if (kind.Value == back.Value) { continue; }     // 引き戻し除外
            var unseen = ctx.GetUnseen(kind);
            if (unseen == 0) { continue; }
            var hand14 = hand13.AddTile(RepresentativeNonRedTile(kind));
            var ev = EvaluateHand14(hand14, kind, back, ctx);
            // 書籍 0305 準拠: 枝刈りは weights 乗算後の値で判定する (v0.6.1)。
            // weight > 1 の牌で `ev < minEvPerTile` でも `ev * weight` が閾値を超えるケースが本来の
            // 書籍挙動では拾われる。
            var weighted = ev * weights.Of(kind);
            if (weighted > minEvPerTile)
            {
                sum += (long)unseen * weighted;
            }
        }
        return sum * ShantenCoefficient(shanten);
    }

    /// <summary>
    /// シャンテン数計算をキャッシュ経由で行う。<see cref="HandSignature"/> と副露面子数で一意。
    /// </summary>
    private int GetShantenCached(Hand hand, int meldCount)
    {
        var sigStart = System.Diagnostics.Stopwatch.GetTimestamp();
        var signature = HandSignature.FromHand(hand);
        SignatureTicks += System.Diagnostics.Stopwatch.GetTimestamp() - sigStart;
        SignatureCount++;
        var key = (signature, meldCount);
        if (shantenCacheGlobal_.TryGetValue(key, out var cached))
        {
            return cached;
        }
        ShantenCallCount++;
        var start = System.Diagnostics.Stopwatch.GetTimestamp();
        var result = ShantenHelper.CalcShanten(hand, meldCount);
        ShantenTicks += System.Diagnostics.Stopwatch.GetTimestamp() - start;
        shantenCacheGlobal_[key] = result;
        return result;
    }

    /// <summary>
    /// 有効牌集合をキャッシュ経由で取得する。<see cref="HandSignature"/> と副露面子数で一意。
    /// インライン化版: 34 種の候補牌 K それぞれについて hand13+K のシャンテンを
    /// <see cref="shantenCacheGlobal_"/> にも格納することで、後続の <see cref="EvaluateHand14"/> からの
    /// <see cref="GetShantenCached"/> 呼び出しがキャッシュヒットするようにする
    /// (従来は useful 列挙で 34 回シャンテン計算し、続く EvaluateHand14 でさらに同じ計算が走っていた)。
    /// </summary>
    private ImmutableHashSet<TileKind> GetUsefulCached(Hand hand13, int meldCount, int knownShanten)
    {
        var signature = HandSignature.FromHand(hand13);
        var key = (signature, meldCount);
        if (usefulCacheGlobal_.TryGetValue(key, out var cached))
        {
            return cached;
        }
        UsefulCallCount++;
        var start = System.Diagnostics.Stopwatch.GetTimestamp();

        // 既に和了 (-1) のときは有効牌なし
        if (knownShanten == SHANTEN_AGARI)
        {
            var emptyResult = ImmutableHashSet<TileKind>.Empty;
            usefulCacheGlobal_[key] = emptyResult;
            UsefulTicks += System.Diagnostics.Stopwatch.GetTimestamp() - start;
            return emptyResult;
        }

        // 34 候補の shanten を計算し、結果を shantenCacheGlobal_ にも格納して二重計算を避ける。
        // tileKindList は shantenCache ミス時のみ必要なので遅延生成する。
        Scoring.Tiles.TileKindList? tileKindList = null;
        Span<int> counts = stackalloc int[34];
        foreach (var tile in hand13)
        {
            counts[tile.Kind.Value]++;
        }

        var builder = ImmutableHashSet.CreateBuilder<TileKind>();
        foreach (var candidate in TileKind.All)
        {
            var v = candidate.Value;
            if (counts[v] >= 4) { continue; }

            counts[v]++;
            var hand14Sig = HandSignature.FromCounts(counts);
            counts[v]--;

            var shantenKey = (hand14Sig, meldCount);
            int hand14Shanten;
            if (shantenCacheGlobal_.TryGetValue(shantenKey, out var cachedShanten))
            {
                hand14Shanten = cachedShanten;
            }
            else
            {
                tileKindList ??= ScoringConversions.ToScoringTileKindList(hand13);
                var appended = tileKindList.Add(candidate);
                hand14Shanten = ShantenCalculator.Calc(appended, meldCount);
                shantenCacheGlobal_[shantenKey] = hand14Shanten;
            }

            if (hand14Shanten < knownShanten)
            {
                builder.Add(candidate);
            }
        }

        var result = builder.ToImmutable();
        UsefulTicks += System.Diagnostics.Stopwatch.GetTimestamp() - start;
        usefulCacheGlobal_[key] = result;
        return result;
    }

    /// <summary>
    /// シャンテン数別の補正係数 (分母 216 を約分済みの long 整数)。
    /// テンパイ = ×18 (= 216/12)、1 シャンテン = ×3 (= 216/72)、2 シャンテン = ×1 (= 216/216)。
    /// </summary>
    private static long ShantenCoefficient(int shanten)
    {
        return shanten switch
        {
            0 => 18,
            1 => 3,
            _ => 1,
        };
    }

    /// <summary>
    /// 副露評価値 (0303)。ポン / チー可能な全パターンを試して「シャンテンが進むもの」の最大評価値を求め、
    /// 書籍の合算式 (peng_max &gt; chi_max ? peng_max*3 : peng_max*2 + chi_max) を適用する。
    /// ポンは 3 家から成立しうるため ×3、チーは下家のみの ×1 として、両立パターンでは peng×2 + chi を採用する。
    /// </summary>
    /// <param name="ponPatterns">ポン可能な (副露後手牌, 副露後コンテキスト) のパターン列</param>
    /// <param name="chiPatterns">チー可能な (副露後手牌, 副露後コンテキスト) のパターン列</param>
    /// <param name="preFulouShanten">副露前の (通常) シャンテン数。副露後がこの値以上ならシャンテン進行なしで除外</param>
    public long EvaluateFulouTotal(
        IEnumerable<(Hand PostHand, HandShapeEvaluatorContext PostCtx)> ponPatterns,
        IEnumerable<(Hand PostHand, HandShapeEvaluatorContext PostCtx)> chiPatterns,
        int preFulouShanten)
    {
        var ponMax = MaxFulouEval(ponPatterns, preFulouShanten);
        var chiMax = MaxFulouEval(chiPatterns, preFulouShanten);
        return ponMax > chiMax
            ? ponMax * 3
            : ponMax * 2 + chiMax;
    }

    private long MaxFulouEval(
        IEnumerable<(Hand PostHand, HandShapeEvaluatorContext PostCtx)> patterns,
        int preFulouShanten)
    {
        long max = 0;
        foreach (var (post, postCtx) in patterns)
        {
            var postMeldCount = postCtx.Calls.Count;
            var postShanten = GetShantenCached(post, postMeldCount);
            if (postShanten >= preFulouShanten) { continue; }

            // 副露直後の手牌は打牌可能な状態 (手牌 3n+2 + 副露 n 面子 = 計 14 枚相当)。
            // 各打牌候補でシャンテン戻さないものの EvaluateHand13 の max を取る。
            long evMax = 0;
            var seenKinds = new HashSet<int>();
            foreach (var tile in post)
            {
                if (!seenKinds.Add(tile.Kind.Value)) { continue; }
                var handAfterDahai = post.RemoveTile(tile);
                var shantenAfterDahai = GetShantenCached(handAfterDahai, postMeldCount);
                if (shantenAfterDahai > postShanten) { continue; }
                var ev = EvaluateHand13(handAfterDahai, back: null, postCtx);
                if (ev > evMax) { evMax = ev; }
            }
            if (evMax > max) { max = evMax; }
        }
        return max;
    }

    /// <summary>
    /// 有効牌 <paramref name="p"/> が他家から出たときにポン/チーで進められる評価値 (0303 eval_fulou 相当)。
    /// 書籍 eval_shoupai の 13 枚分岐で加算される。
    /// ポンは 3 家から成立しうるので ×3、チーは下家のみの ×1、両立時は peng×2 + chi の合算式を採用する。
    /// シャンテンが進むパターンのみ採用、既テンパイ (呼び出し側で shanten &gt; 0 チェック済み) は対象外。
    /// </summary>
    private long EvaluateFulouForTile(Hand hand13, TileKind p, HandShapeEvaluatorContext ctx, int preFulouShanten)
    {
        // Fulou キャッシュ: (hand13, p, ctx.Calls, akadora) が一致すれば結果も一致する (同一 ctx 内の限り)。
        // akadora を含めるのは HandSignature / CallsSignature が赤黒を潰す設計のため。
        var hand13Sig = HandSignature.FromHand(hand13);
        var akadora = CountAkadora(hand13, ctx.Calls, ctx.Rules);
        var fulouKey = (hand13Sig, p.Value, ctx.CallsSignature, akadora);
        if (fulouCache_.TryGetValue(fulouKey, out var cached))
        {
            return cached;
        }

        // 再帰ガードを立てる。このブロック内の EvaluateHand13 は eval_fulou 加算を行わない。
        var prevGuard = inFulouRecursion_;
        inFulouRecursion_ = true;
        try
        {
            Span<int> handKinds = stackalloc int[34];
            foreach (var tile in hand13)
            {
                handKinds[tile.Kind.Value]++;
            }

            // ポン: hand13 に p が 2 枚以上含まれる
            long ponMax = 0;
            if (handKinds[p.Value] >= 2)
            {
                ponMax = EvaluateOneFulouPattern(hand13, ctx, preFulouShanten, CallType.Pon, [p, p], p);
            }

            // チー: p が数牌で、隣接 2 枚が手牌に揃う
            long chiMax = 0;
            if (!p.IsHonor)
            {
                ReadOnlySpan<(int d1, int d2)> chiOffsets = [(-2, -1), (-1, 1), (1, 2)];
                foreach (var (d1, d2) in chiOffsets)
                {
                    if (!p.TryGetAtDistance(d1, out var a)) { continue; }
                    if (!p.TryGetAtDistance(d2, out var b)) { continue; }
                    // a と b が同種の場合 (= 発生し得ないが念のため) は 2 枚必要
                    if (a.Value == b.Value)
                    {
                        if (handKinds[a.Value] < 2) { continue; }
                    }
                    else
                    {
                        if (handKinds[a.Value] < 1 || handKinds[b.Value] < 1) { continue; }
                    }

                    var ev = EvaluateOneFulouPattern(hand13, ctx, preFulouShanten, CallType.Chi, [a, b], p);
                    if (ev > chiMax) { chiMax = ev; }
                }
            }

            var result = ponMax > chiMax
                ? ponMax * 3
                : ponMax * 2 + chiMax;
            fulouCache_[fulouKey] = result;
            return result;
        }
        finally
        {
            inFulouRecursion_ = prevGuard;
        }
    }

    /// <summary>
    /// 1 つの副露パターンについて、副露後の最大評価値を計算する。
    /// <paramref name="takenFromHand"/> は手牌から差し出す牌種 (ポンなら [p, p]、チーなら [a, b])。
    /// 副露後シャンテンが <paramref name="preFulouShanten"/> 以上ならシャンテン進行なしで 0 を返す。
    /// </summary>
    private long EvaluateOneFulouPattern(
        Hand hand13,
        HandShapeEvaluatorContext ctx,
        int preFulouShanten,
        CallType type,
        ReadOnlySpan<TileKind> takenFromHand,
        TileKind calledKind)
    {
        // 手牌から取り除く (Tile 単位で実在する Id を 1 枚ずつ引く)
        var handAfter = hand13;
        foreach (var kind in takenFromHand)
        {
            var matched = false;
            foreach (var tile in handAfter)
            {
                if (tile.Kind.Value == kind.Value)
                {
                    handAfter = handAfter.RemoveTile(tile);
                    matched = true;
                    break;
                }
            }
            if (!matched)
            {
                return 0;
            }
        }

        // 仮想 Call を構築 (From と CalledTile は BackMarker="" キャッシュキー空間内で同一パターンなら衝突しない)
        var calledTile = new Tile(calledKind.Value * 4 + 3);
        var callTiles = new List<Tile>(takenFromHand.Length + 1);
        foreach (var kind in takenFromHand)
        {
            callTiles.Add(new Tile(kind.Value * 4 + 3));
        }
        callTiles.Add(calledTile);
        var virtualCall = new Call(
            type,
            [.. callTiles],
            new PlayerIndex(0),
            calledTile);
        var callsAfter = ctx.Calls.Add(virtualCall);

        var postMeldCount = callsAfter.Count;
        var postShanten = GetShantenCached(handAfter, postMeldCount);
        if (postShanten >= preFulouShanten) { return 0; }

        var postCtx = ctx with
        {
            Calls = callsAfter,
            TileWeights = TileWeights.Build(handAfter, callsAfter),
            BackMarker = "",
        };

        // 副露後 11 枚 (または 10 枚) から 1 枚打牌した手牌の max 評価値。
        // ここで EvaluateHand13 を再帰呼び出しすると仮想副露が積み重なって計算量が爆発するため、
        // 「打牌後がテンパイ形 → 待ち牌 × CalcHandScore の期待値」という 1 段限定の軽量評価に抑える。
        long evMax = 0;
        var seenKinds = new HashSet<int>();
        foreach (var tile in handAfter)
        {
            if (!seenKinds.Add(tile.Kind.Value)) { continue; }
            var handAfterDahai = handAfter.RemoveTile(tile);
            var shantenAfterDahai = GetShantenCached(handAfterDahai, postMeldCount);
            if (shantenAfterDahai > postShanten) { continue; }

            var ev = EvaluateFulouPostDahaiTerminal(handAfterDahai, postCtx, shantenAfterDahai);
            if (ev > evMax) { evMax = ev; }
        }
        return evMax;
    }

    /// <summary>
    /// 副露評価の末端で用いる軽量評価。副露後打牌 13 枚 (または 10 枚) がテンパイなら
    /// 待ち牌 × 和了打点の期待値にテンパイ補正 (×18) を掛けて返す。
    /// 1 シャンテンなら各有効牌 K について 14 枚テンパイ評価値を計算し、×3 補正で返す (v0.6.1 追加)。
    /// 2 シャンテン以深は再帰深さ・仮想副露連鎖での計算量爆発を避けるため 0 返却で打ち切る。
    /// </summary>
    /// <remarks>
    /// 待ち牌列挙は <see cref="TenpaiHelper.EnumerateWaitTileKinds(Hand)"/> ではなく
    /// シャンテン 0 で useful キャッシュ (shanten を 0→-1 に進める牌 = 待ち牌) を使う。
    /// <see cref="TenpaiHelper"/> 実装は毎回 34 回の非キャッシュ shanten 計算を走らせるため、
    /// 副露評価のようにテンパイ判定が多発する経路ではボトルネックになる。
    /// </remarks>
    private long EvaluateFulouPostDahaiTerminal(Hand handAfterDahai, HandShapeEvaluatorContext postCtx, int shantenAfterDahai)
    {
        if (shantenAfterDahai >= 2) { return 0; }

        var postMeldCount = postCtx.Calls.Count;

        if (shantenAfterDahai == 0)
        {
            // テンパイ: 待ち牌 = シャンテン 0 の useful = 引くとシャンテン -1 になる牌
            // 書籍準拠: paijia は handAfterDahai + calls から再構築 (v0.6.1)
            var weights0 = TileWeights.Build(handAfterDahai, postCtx.Calls);
            var waits = GetUsefulCached(handAfterDahai, postMeldCount, shantenAfterDahai);
            long sum = 0;
            foreach (var winKind in waits)
            {
                var unseen = postCtx.GetUnseen(winKind);
                if (unseen == 0) { continue; }
                var hand14 = handAfterDahai.AddTile(RepresentativeNonRedTile(winKind));
                var handScore = CalcHandScore(hand14, winKind, postCtx);
                sum += (long)unseen * handScore * weights0.Of(winKind);
            }
            return sum * ShantenCoefficient(0);
        }

        // 1 シャンテン: 各有効牌 K を引いた 14 枚相当手牌 (テンパイ) から、
        // さらに最良の打牌で作るテンパイ hand13 → 待ち牌 × CalcHandScore の期待値。
        // hand14 を CalcHandScore に直接渡すとテンパイ形で役なし 0 返却されるため、
        // 必ず 1 枚打牌した hand13 を経由してから和了形を構築する (Codex 指摘 P1 の修正)。
        var handAfterDahaiWeights = TileWeights.Build(handAfterDahai, postCtx.Calls);
        var useful = GetUsefulCached(handAfterDahai, postMeldCount, shantenAfterDahai);
        long sum1 = 0;
        foreach (var kind in useful)
        {
            var unseen = postCtx.GetUnseen(kind);
            if (unseen == 0) { continue; }
            var hand14 = handAfterDahai.AddTile(RepresentativeNonRedTile(kind));
            var hand14Shanten = GetShantenCached(hand14, postMeldCount);
            if (hand14Shanten != 0) { continue; }

            // hand14 の最良打牌 (シャンテン 0 維持) を探し、hand13 のテンパイ評価値を取る
            long bestDahaiEv = 0;
            var seenKinds = new HashSet<int>();
            foreach (var tile in hand14)
            {
                if (!seenKinds.Add(tile.Kind.Value)) { continue; }
                var hand13 = hand14.RemoveTile(tile);
                var hand13Shanten = GetShantenCached(hand13, postMeldCount);
                if (hand13Shanten != 0) { continue; }

                // hand13 (テンパイ) の評価値 = 全待ち牌 × CalcHandScore(hand13 + winKind)
                var weights13 = TileWeights.Build(hand13, postCtx.Calls);
                var waits = GetUsefulCached(hand13, postMeldCount, hand13Shanten);
                long tenpaiEv = 0;
                foreach (var winKind in waits)
                {
                    var winUnseen = postCtx.GetUnseen(winKind);
                    if (winUnseen == 0) { continue; }
                    var winHand = hand13.AddTile(RepresentativeNonRedTile(winKind));
                    var handScore = CalcHandScore(winHand, winKind, postCtx);
                    tenpaiEv += (long)winUnseen * handScore * weights13.Of(winKind);
                }
                var tenpaiEvWithCoef = tenpaiEv * ShantenCoefficient(0);
                if (tenpaiEvWithCoef > bestDahaiEv) { bestDahaiEv = tenpaiEvWithCoef; }
            }
            sum1 += (long)unseen * bestDahaiEv * handAfterDahaiWeights.Of(kind);
        }
        return sum1 * ShantenCoefficient(1);
    }

    /// <summary>
    /// <see cref="EvaluateHand13"/> から「有効牌 K を引いた」14 枚手牌を作るための代表 Tile を返す。
    /// 赤ドラでない Id を選ぶことで <see cref="CalcHandScore"/> の AkadoraCount がずれないようにする。
    /// TileKind レベルで評価値は一意に決まるため、Id の選び方は結果に影響しない。
    /// </summary>
    private static Tile RepresentativeNonRedTile(TileKind kind)
    {
        // 赤ドラは既定で Tile(16) / Tile(52) / Tile(88) の 3 枚のみ (Kind の先頭 Id)。
        // +3 オフセットで常に非赤 Id を指す。
        return new Tile(kind.Value * 4 + 3);
    }

    private const int SHANTEN_AGARI = -1;

    /// <summary>
    /// 和了打点キャッシュのキー。
    /// Calls のシグネチャを含めないと、仮想副露シミュレーションで異なる副露を持つ同一手牌組成の
    /// 打点が衝突するため必須フィールド (<see cref="CallsSignature"/> で高速比較)。
    /// </summary>
    private readonly record struct HandScoreKey(
        HandSignature Hand,
        int AkadoraCount,
        int WinTileValue,
        bool IsMenzen,
        CallsSignature Calls
    );

    /// <summary>
    /// 牌姿評価値キャッシュのキー。
    /// Calls は <see cref="CallsSignature"/> (64 bit FNV-1a) で高速比較する。
    /// BackMarker は null (通常ルート) / 空文字列 (副露判定ルート) で区別する。
    /// BackKindValue は 0302 シャンテン戻しで切った牌種 (null = 非戻し) で、フリテン判定結果を別エントリにする。
    /// Akadora は手牌 + 副露の赤ドラ枚数。<see cref="HandSignature"/> / <see cref="CallsSignature"/> が
    /// 意図的に赤黒を区別しない (34 牌種) ため、そのままでは赤 5 含む hand と含まない hand が
    /// 同じ EvalKey に衝突する (<see cref="CalcHandScore"/> は別途 <see cref="HandScoreKey.AkadoraCount"/> で
    /// 赤ドラ差を拾っているため、EvaluateHand13/14 側でも同じ補正が必要)。
    /// </summary>
    private readonly record struct EvalKey(
        HandSignature Hand,
        CallsSignature Calls,
        string? BackMarker,
        int? BackKindValue,
        int Akadora
    );
}
