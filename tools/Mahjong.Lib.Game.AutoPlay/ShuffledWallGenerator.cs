using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;

namespace Mahjong.Lib.Game.AutoPlay;

/// <summary>
/// 指定シードの <see cref="Random"/> で Fisher-Yates シャッフルした山牌を返すシンプルな <see cref="IWallGenerator"/>。
/// 同一シードで同一山を返すので自動対局の再現性確保に使える
/// </summary>
public sealed class ShuffledWallGenerator(int seed) : IWallGenerator
{
    private readonly Random rng_ = new(seed);

    public Wall Generate()
    {
        var ids = Enumerable.Range(Tile.ID_MIN, Tile.ID_MAX - Tile.ID_MIN + 1).ToArray();
        for (var i = ids.Length - 1; i > 0; i--)
        {
            var j = rng_.Next(i + 1);
            (ids[i], ids[j]) = (ids[j], ids[i]);
        }
        return new Wall(ids.Select(x => new Tile(x)));
    }
}
