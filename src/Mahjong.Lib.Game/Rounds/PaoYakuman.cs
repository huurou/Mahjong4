namespace Mahjong.Lib.Game.Rounds;

/// <summary>
/// 包 (責任払い) の対象となる役満種別
/// </summary>
public enum PaoYakuman
{
    /// <summary>
    /// 包対象役満なし
    /// </summary>
    None,
    /// <summary>
    /// 大三元
    /// </summary>
    Daisangen,
    /// <summary>
    /// 大四喜
    /// </summary>
    Daisuushii,
    /// <summary>
    /// 四槓子
    /// </summary>
    Suukantsu,
}

/// <summary>
/// <see cref="PaoYakuman"/> の拡張メソッド
/// </summary>
public static class PaoYakumanExtensions
{
    /// <summary>
    /// 包対象役満が検出されたかを返す (<see cref="PaoYakuman.None"/> 以外なら true)
    /// </summary>
    public static bool IsPao(this PaoYakuman yakuman)
    {
        return yakuman != PaoYakuman.None;
    }
}
