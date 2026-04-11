using Mahjong.Lib.Shantens;
using Mahjong.Lib.Tiles;
using System.Collections;

namespace Mahjong.Lib.Tests.Shantens;

public class IsolationSet_GetEnumeratorTests
{
    [Fact]
    public void ジェネリック版_すべての要素を列挙できる()
    {
        // Arrange
        var tiles = new List<TileKind> { TileKind.Man1, TileKind.Pin2, TileKind.Sou3 };
        var isolationSet = new IsolationSet(tiles);

        // Act
        var enumeratedTiles = new List<TileKind>();
        foreach (var tile in isolationSet)
        {
            enumeratedTiles.Add(tile);
        }

        // Assert
        Assert.Equal(3, enumeratedTiles.Count);
        Assert.Contains(TileKind.Man1, enumeratedTiles);
        Assert.Contains(TileKind.Pin2, enumeratedTiles);
        Assert.Contains(TileKind.Sou3, enumeratedTiles);
    }

    [Fact]
    public void 非ジェネリック版_すべての要素を列挙できる()
    {
        // Arrange
        var tiles = new List<TileKind> { TileKind.Man1, TileKind.Pin2 };
        var isolationSet = new IsolationSet(tiles);

        // Act
        var enumeratedTiles = new List<TileKind>();
        var enumerator = ((IEnumerable)isolationSet).GetEnumerator();
        while (enumerator.MoveNext())
        {
            enumeratedTiles.Add((TileKind)enumerator.Current!);
        }

        // Assert
        Assert.Equal(2, enumeratedTiles.Count);
        Assert.Contains(TileKind.Man1, enumeratedTiles);
        Assert.Contains(TileKind.Pin2, enumeratedTiles);
    }
}
