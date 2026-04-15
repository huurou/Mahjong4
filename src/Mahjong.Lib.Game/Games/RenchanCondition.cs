namespace Mahjong.Lib.Game.Games;

/// <summary>
/// 連荘条件
/// </summary>
public enum RenchanCondition
{
    /// <summary>
    /// 連荘なし
    /// </summary>
    None,

    /// <summary>
    /// 親和了のみ連荘
    /// </summary>
    AgariOnly,

    /// <summary>
    /// 親和了または親テンパイで連荘 (既定)
    /// </summary>
    AgariOrTenpai,
}
