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

namespace Mahjong.Lib.Game.Players.Impl;

/// <summary>
/// ver.0.5.0 鳴き — 副露対応の AI プレイヤー。
/// v0.4.0 の回し打ち守備を維持しつつ、<see cref="YakuAwareShantenHelper"/> で算出する役ありシャンテン
/// に基づいて、チー/ポン/大明槓/暗槓/加槓の採否と、副露マーク付き有効牌による打牌選択を行う。
/// 打牌評価値: ev = Σ unseen(K) × (Pon:×4 / Chi:×2 / None:×1)。
/// 副露受け: 役ありシャンテンを進めるポン/チー、進めない大明槓は採用、テンパイ時やベタオリ時は副露しない。
/// </summary>
public sealed class AI_v0_5_0_鳴き(
    PlayerId playerId,
    PlayerIndex playerIndex,
    Random rng
) : Player(playerId, DISPLAY_NAME, playerIndex)
{
    public const string DISPLAY_NAME = "ver.0.5.0 鳴き";

    /// <summary>
    /// 1 シャンテン時に回し打ちするかベタオリするかの閾値 (v0.4.0 と同じ)
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
        EnsureRules();
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
    /// 攻撃打牌選択。役ありシャンテン min の候補から、
    /// 副露マーク付き有効牌の評価値 (ポン×4/チー×2/素引き×1) の合計が最大のものを選ぶ。
    /// </summary>
    private DahaiOption SelectAttackDahai(PlayerRoundView view, DahaiOptionList options)
    {
        var hand14 = view.OwnHand;
        var calls = GetCalls(view);
        var roundWindIndex = view.RoundWind.Value;
        var seatWindIndex = CalcSeatWindIndex(view);
        var rules = rules_!;

        // Kind 単位でシャンテン・評価値をメモ化 (同一 Kind の候補は hand13 が同形になるため)
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
                score += VisibleTileCounter.CountUnseen(view, K) * multiplier;
            }
            evCache[kind] = score;
            return score;
        }

        // フェーズ 1: 役ありシャンテン最小で絞る (シャンテン戻しは対象外)
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
/// <see cref="AI_v0_5_0_鳴き"/> を席順ごとに生成する <see cref="IPlayerFactory"/>。
/// </summary>
public sealed class AI_v0_5_0_鳴きFactory(int seed)
    : AiPlayerFactoryBase<AI_v0_5_0_鳴き>(seed, AI_v0_5_0_鳴き.DISPLAY_NAME)
{
    protected override AI_v0_5_0_鳴き CreatePlayer(PlayerId id, PlayerIndex index, Random rng)
    {
        return new(id, index, rng);
    }
}
