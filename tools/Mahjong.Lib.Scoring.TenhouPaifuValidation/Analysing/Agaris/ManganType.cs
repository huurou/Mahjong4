namespace Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Agaris;

/// <summary>
/// 満貫種別 天鳳の ten 属性3番目の値に対応
/// </summary>
public enum ManganType
{
    /// <summary>
    /// 満貫未満
    /// </summary>
    None,
    /// <summary>
    /// 満貫
    /// </summary>
    Mangan,
    /// <summary>
    /// 跳満
    /// </summary>
    Haneman,
    /// <summary>
    /// 倍満
    /// </summary>
    Baiman,
    /// <summary>
    /// 三倍満
    /// </summary>
    Sanbaiman,
    /// <summary>
    /// 役満
    /// </summary>
    Yakuman,
}
