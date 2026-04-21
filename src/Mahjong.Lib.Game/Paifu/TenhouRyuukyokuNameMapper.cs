using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Paifu;

/// <summary>
/// <see cref="RyuukyokuType"/> (+ 流し満貫フラグ) を天鳳 JSON 牌譜の流局名に変換する
/// </summary>
public static class TenhouRyuukyokuNameMapper
{
    /// <summary>
    /// <paramref name="type"/> と流し満貫の有無から天鳳流局名を返す
    /// </summary>
    /// <param name="type">流局種別</param>
    /// <param name="hasNagashiMangan">流し満貫者が存在するか (荒牌平局時のみ有効)</param>
    public static string ToName(RyuukyokuType type, bool hasNagashiMangan)
    {
        if (type == RyuukyokuType.KouhaiHeikyoku && hasNagashiMangan)
        {
            return "流し満貫";
        }
        return type switch
        {
            RyuukyokuType.KouhaiHeikyoku => "流局",
            RyuukyokuType.KyuushuKyuuhai => "九種九牌",
            RyuukyokuType.Suufonrenda => "四風連打",
            RyuukyokuType.Suukaikan => "四槓散了",
            RyuukyokuType.SuuchaRiichi => "四家立直",
            RyuukyokuType.SanchaHou => "三家和了",
            _ => throw new ArgumentException($"不明な流局種別:{type}", nameof(type)),
        };
    }

    /// <summary>
    /// 当該流局が点棒移動を伴うか (天鳳 JSON の result 配列に delta を含めるかどうかの判定)
    /// </summary>
    public static bool HasPointDelta(RyuukyokuType type, bool hasNagashiMangan)
    {
        // 荒牌平局はテンパイ料ありなので delta 付き。流し満貫も点棒移動あり
        return type == RyuukyokuType.KouhaiHeikyoku;
    }
}
