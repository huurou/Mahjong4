namespace Mahjong.Lib.Game.Walls;

/// <summary>
/// 山牌生成機のインターフェース
/// </summary>
public interface IWallGenerator
{
    /// <summary>
    /// 牌山を生成します。
    /// </summary>
    /// <returns>生成された牌山</returns>
    Wall Generate();
}
