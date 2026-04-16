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
}
