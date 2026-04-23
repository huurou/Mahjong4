using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Scoring.Games;
using System.Collections.Immutable;
using GameRules = Mahjong.Lib.Game.Games.GameRules;
using ScoringCallList = Mahjong.Lib.Scoring.Calls.CallList;
using ScoringGameRules = Mahjong.Lib.Scoring.Games.GameRules;
using ScoringTileKindList = Mahjong.Lib.Scoring.Tiles.TileKindList;
using TileKind = Mahjong.Lib.Scoring.Tiles.TileKind;

namespace Mahjong.Lib.Game.Players.Impl;

/// <summary>
/// 牌姿の評価値計算に必要な局面の不変情報 (親子・自風・場風・ドラ・副露・未見牌数) をまとめたコンテキスト。
/// ラウンドおよびドラ開示の粒度で再構築する。
/// </summary>
/// <param name="Rules">対局ルール。クイタン・赤ドラ判定等を含む</param>
/// <param name="RoundWindIndex">場風のインデックス (0=東, 1=南, 2=西, 3=北)</param>
/// <param name="SeatWindIndex">自風のインデックス (0=東=親, 1=南, 2=西, 3=北)</param>
/// <param name="RoundWind">WinSituation 用の場風</param>
/// <param name="PlayerWind">WinSituation 用の自風</param>
/// <param name="DoraIndicatorKinds">ドラ表示牌 (TileKind 列、HandCalculator.Calc にそのまま渡す)</param>
/// <param name="Calls">自プレイヤーの副露</param>
/// <param name="GetUnseen">TileKind → 未見枚数 (VisibleTileCounter の結果をラップ)</param>
/// <param name="TileWeights">0305 の牌種別重み (一色手/大三元/四喜和狙い)</param>
/// <param name="BackMarker">副露判定時のキャッシュ区別用マーカー。通常は null、副露判定時は空文字列</param>
internal sealed record HandShapeEvaluatorContext(
    GameRules Rules,
    int RoundWindIndex,
    int SeatWindIndex,
    Wind RoundWind,
    Wind PlayerWind,
    ImmutableArray<TileKind> DoraIndicatorKinds,
    CallList Calls,
    Func<TileKind, int> GetUnseen,
    TileWeights TileWeights,
    string? BackMarker
)
{
    // 派生キャッシュ群。遅延生成する。
    // record の `with { Calls = ... }` でコピーコンストラクタが走ると、既定ではこれらも
    // 旧インスタンスの値がそのまま複製されて Calls と整合しなくなる。これを防ぐため、
    // 下の copy constructor で派生キャッシュをリセットしている。
    private CallsSignature? callsSignature_;
    private ScoringCallList? scoringCallList_;
    private ScoringGameRules? scoringGameRules_;
    private ScoringTileKindList? scoringDoraIndicators_;

    /// <summary>
    /// コピーコンストラクタ。record の <c>with</c> 式で走るコピーで派生キャッシュが持ち越されて
    /// 古い <see cref="Calls"/> / <see cref="Rules"/> に紐付いた値を誤って再利用してしまうのを防ぐ。
    /// 派生フィールドは null 初期化のままにして、新インスタンスの Calls/Rules で再計算させる。
    /// record のコピーコンストラクタは base か parameterless ctor にしかチェーンできないため、
    /// init-only プロパティへの代入で primary ctor パラメータを複製する。
    /// </summary>
    private HandShapeEvaluatorContext(HandShapeEvaluatorContext original)
    {
        Rules = original.Rules;
        RoundWindIndex = original.RoundWindIndex;
        SeatWindIndex = original.SeatWindIndex;
        RoundWind = original.RoundWind;
        PlayerWind = original.PlayerWind;
        DoraIndicatorKinds = original.DoraIndicatorKinds;
        Calls = original.Calls;
        GetUnseen = original.GetUnseen;
        TileWeights = original.TileWeights;
        BackMarker = original.BackMarker;
        // 派生キャッシュ (callsSignature_ / scoring*_) は意図的にコピーしない: null 初期化のまま、
        // 新 Calls/Rules に対して各 getter が遅延再計算する。
    }

    /// <summary>
    /// <see cref="Calls"/> の事前計算済みシグネチャ。キャッシュキーとして <see cref="CallList"/> 自体よりも
    /// 高速に等価比較できる。
    /// </summary>
    public CallsSignature CallsSignature
    {
        get
        {
            callsSignature_ ??= CallsSignature.FromCalls(Calls);
            return callsSignature_.Value;
        }
    }

    /// <summary>
    /// <see cref="HandCalculator"/> に渡すための Scoring 側 <c>CallList</c>。
    /// 毎回の <c>CalcHandScore</c> で再変換していた部分を Context 単位で 1 回に抑える。
    /// </summary>
    public ScoringCallList ScoringCallList
    {
        get
        {
            scoringCallList_ ??= Calls.ToScoringCallList();
            return scoringCallList_;
        }
    }

    /// <summary>
    /// <see cref="HandCalculator"/> に渡すための Scoring 側 <c>GameRules</c> (KuitanEnabled のみ)。
    /// </summary>
    public ScoringGameRules ScoringGameRules
    {
        get
        {
            scoringGameRules_ ??= Rules.ToScoringGameRules();
            return scoringGameRules_;
        }
    }

    /// <summary>
    /// <see cref="HandCalculator"/> に渡すためのドラ表示牌の <c>TileKindList</c>。
    /// </summary>
    public ScoringTileKindList ScoringDoraIndicators
    {
        get
        {
            scoringDoraIndicators_ ??= [.. DoraIndicatorKinds];
            return scoringDoraIndicators_;
        }
    }

    /// <summary>
    /// 親かどうか (自風が東かで判定)
    /// </summary>
    public bool IsDealer => PlayerWind == Wind.East;

    /// <summary>
    /// BackMarker を差し替えた派生 Context を返す。
    /// </summary>
    public HandShapeEvaluatorContext WithBackMarker(string? backMarker)
    {
        return this with { BackMarker = backMarker };
    }

    /// <summary>
    /// Calls を差し替えた派生 Context を返す (副露シミュレーション用)。
    /// </summary>
    public HandShapeEvaluatorContext WithCalls(CallList calls)
    {
        return this with { Calls = calls };
    }
}
