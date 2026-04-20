using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Hands;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tenpai;

/// <summary>
/// シャンテン数評価の抽象
/// 実装は Mahjong.Lib.Scoring に依存しない形で Lib.Game 外部 (ApiService 等) から注入する
/// </summary>
/// <remarks>
/// <paramref name="callList"/> は現行の Mahjong.Lib.Scoring ラッパー実装 (ShantenEvaluatorImpl) では未使用。
/// ShantenCalculator が手牌枚数 (14 - 手牌数) / 3 から副露面子数を推論するため機能的に問題はないが、
/// 将来的に明示的な副露構成を用いる実装 (例: 刻子/順子区別に基づく有効牌評価) を注入可能にするため
/// インターフェース上は callList を受け取る
/// </remarks>
public interface IShantenEvaluator
{
    /// <summary>
    /// 指定の手牌・副露のシャンテン数を返します。テンパイ時は 0、和了形は -1
    /// </summary>
    int CalcShanten(Hand hand, CallList callList);

    /// <summary>
    /// 指定の手牌・副露について 引いた時にシャンテン数が減る牌種ID (0-33) の集合を返します。
    /// 既に 4 枚持っている牌種は対象外です。和了形の場合は空集合
    /// </summary>
    ImmutableHashSet<int> EnumerateUsefulTileKinds(Hand hand, CallList callList);
}
