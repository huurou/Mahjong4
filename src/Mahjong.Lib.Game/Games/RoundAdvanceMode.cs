namespace Mahjong.Lib.Game.Games;

/// <summary>
/// 次局への進め方
/// </summary>
public enum RoundAdvanceMode
{
    /// <summary>
    /// 連荘 (親続行 + 本場+1)
    /// </summary>
    Renchan,

    /// <summary>
    /// 親流れ + 本場+1 (途中流局・ノーテン流局など)
    /// </summary>
    DealerChangeWithHonba,

    /// <summary>
    /// 親流れ + 本場0 (通常の親流れ)
    /// </summary>
    DealerChangeResetHonba,
}
