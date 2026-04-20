using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Hands;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tenpai;

/// <summary>
/// テンパイ判定の抽象
/// 実装は Mahjong.Lib.Scoring に依存しない形で Lib.Game 外部 (ApiService 等) から注入する
/// </summary>
public interface ITenpaiChecker
{
    /// <summary>
    /// 指定の手牌・副露がテンパイ (向聴数=0) かを判定します
    /// </summary>
    bool IsTenpai(Hand hand, CallList callList);

    /// <summary>
    /// 指定の手牌・副露の待ち牌種ID (0-33) の集合を返します。テンパイでなければ空集合
    /// </summary>
    ImmutableHashSet<int> EnumerateWaitTileKinds(Hand hand, CallList callList);

    /// <summary>
    /// 指定の牌種 <paramref name="kind"/> が <paramref name="hand"/> のすべてのテンパイ解釈において
    /// 刻子 (暗刻) として使われるかを判定します。順子として使う分解解釈が 1 つでも存在する場合は <c>false</c>。
    /// 立直中の暗槓送り槓禁止判定で使用します。<paramref name="hand"/> は立直確定時点の 13 枚 (もしくはツモ前相当) を渡す前提です
    /// </summary>
    bool IsKoutsuOnlyInAllInterpretations(Hand hand, CallList callList, int kind);
}
