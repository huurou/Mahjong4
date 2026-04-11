using Mahjong.Lib.Shantens;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Shantens;

public class IsolationSet_CountTests
{
    [Fact]
    public void 空のセット_0を返す()
    {
        // Arrange
        var isolationSet = new IsolationSet();

        // Act & Assert
        Assert.Equal(0, isolationSet.Count);
    }

    [Fact]
    public void 複数要素のセット_正しい数を返す()
    {
        // Arrange
        var tiles = new List<TileKind> { TileKind.Man1, TileKind.Pin5, TileKind.Sou9, TileKind.Ton };
        var isolationSet = new IsolationSet(tiles);

        // Act & Assert
        Assert.Equal(4, isolationSet.Count);
    }
}
