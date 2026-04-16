namespace Mahjong.Lib.Game.Rounds;

/// <summary>
/// 流局種別
/// </summary>
public enum RyuukyokuType
{
    /// <summary>
    /// 荒牌平局 (ツモ山が尽きた通常の流局)
    /// </summary>
    KouhaiHeikyoku,

    /// <summary>
    /// 九種九牌
    /// </summary>
    KyuushuKyuuhai,

    /// <summary>
    /// 四風連打
    /// </summary>
    Suufonrenda,

    /// <summary>
    /// 四槓流れ
    /// </summary>
    Suukaikan,

    /// <summary>
    /// 四家立直
    /// </summary>
    SuuchaRiichi,

    /// <summary>
    /// 三家和了
    /// </summary>
    SanchaHou,
}
