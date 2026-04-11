namespace Mahjong.Lib.Scoring.Games;

/// <summary>
/// 数え役満のルール
/// </summary>
public enum KazoeLimit
{
    /// <summary>
    /// 13翻以上は全て数え役満
    /// </summary>
    Limited,
    /// <summary>
    /// 13翻以上は全て三倍満
    /// </summary>
    Sanbaiman,
    /// <summary>
    /// 13翻以上は13翻ごとに数え役満が重なる
    /// </summary>
    NoLimit,
}
