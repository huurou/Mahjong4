namespace Mahjong.Lib.Scoring.Fus;

/// <summary>
/// 符種別
/// </summary>
public enum FuType
{
    /// <summary>
    /// 副底
    /// </summary>
    Futei,
    /// <summary>
    /// 面前加符
    /// </summary>
    Menzen,
    /// <summary>
    /// 七対子符
    /// </summary>
    Chiitoitsu,
    /// <summary>
    /// 副底(食い平和) ロンあがり専用
    /// </summary>
    FuteiOpenPinfu,
    /// <summary>
    /// ツモ符
    /// </summary>
    Tsumo,
    /// <summary>
    /// カンチャン待ち
    /// </summary>
    Kanchan,
    /// <summary>
    /// ペンチャン待ち
    /// </summary>
    Penchan,
    /// <summary>
    /// 単騎待ち
    /// </summary>
    Tanki,
    /// <summary>
    /// 自風の雀頭
    /// </summary>
    JantouPlayerWind,
    /// <summary>
    /// 場風の雀頭
    /// </summary>
    JantouRoundWind,
    /// <summary>
    /// 三元牌の雀頭
    /// </summary>
    JantouDragon,
    /// <summary>
    /// 中張牌の明刻
    /// </summary>
    MinkoChunchan,
    /// <summary>
    /// 么九牌の明刻
    /// </summary>
    MinkoYaochu,
    /// <summary>
    /// 中張牌の暗刻
    /// </summary>
    AnkoChunchan,
    /// <summary>
    /// 么九牌の暗刻
    /// </summary>
    AnkoYaochu,
    /// <summary>
    /// 中張牌の明槓
    /// </summary>
    MinkanChunchan,
    /// <summary>
    /// 么九牌の明槓
    /// </summary>
    MinkanYaochu,
    /// <summary>
    /// 中張牌の暗槓
    /// </summary>
    AnkanChunchan,
    /// <summary>
    /// 么九牌の暗槓
    /// </summary>
    AnkanYaochu,
}

public static class FuTypeExtensions
{
    /// <summary>
    /// 符種別の文字列を取得します。
    /// </summary>
    /// <param name="fuType">符種別</param>
    /// <returns>符種別の文字列</returns>
    public static string ToStr(this FuType fuType)
    {
        return fuType switch
        {
            FuType.Futei => "副底",
            FuType.Menzen => "面前加符",
            FuType.Chiitoitsu => "七対子符",
            FuType.FuteiOpenPinfu => "副底(食い平和)",
            FuType.Tsumo => "ツモ符",
            FuType.Kanchan => "カンチャン待ち",
            FuType.Penchan => "ペンチャン待ち",
            FuType.Tanki => "単騎待ち",
            FuType.JantouPlayerWind => "自風の雀頭",
            FuType.JantouRoundWind => "場風の雀頭",
            FuType.JantouDragon => "三元牌の雀頭",
            FuType.MinkoChunchan => "中張牌の明刻",
            FuType.MinkoYaochu => "么九牌の明刻",
            FuType.AnkoChunchan => "中張牌の暗刻",
            FuType.AnkoYaochu => "么九牌の暗刻",
            FuType.MinkanChunchan => "中張牌の明槓",
            FuType.MinkanYaochu => "么九牌の明槓",
            FuType.AnkanChunchan => "中張牌の暗槓",
            FuType.AnkanYaochu => "么九牌の暗槓",
            _ => throw new ArgumentOutOfRangeException(nameof(fuType), fuType, null),
        };
    }
}
