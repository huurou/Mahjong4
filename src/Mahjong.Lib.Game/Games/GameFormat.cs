namespace Mahjong.Lib.Game.Games;

/// <summary>
/// 対局形式
/// </summary>
public enum GameFormat
{
    /// <summary>
    /// 東風戦 (4局)
    /// </summary>
    Tonpuu,

    /// <summary>
    /// 東南戦 (8局、既定)
    /// </summary>
    Tonnan,

    /// <summary>
    /// 1局完結 (テスト・デバッグ用)
    /// </summary>
    SingleRound,
}
