namespace Mahjong.Lib.Game.Walls;

/// <summary>
/// 山牌生成機のインターフェース
/// </summary>
public interface IWallGenerator
{
    /// <summary>
    /// 牌山を生成します。
    /// </summary>
    /// <remarks>
    /// 実装はステートフルである可能性があるため、同一インスタンスの連続呼び出しが同じ山を返すとは限りません。
    /// (例: <see cref="WallGeneratorTenhou"/> は Mersenne Twister の内部状態を消費するため、呼び出しごとに異なる山を返します)
    /// </remarks>
    /// <returns>生成された牌山</returns>
    Wall Generate();
}
