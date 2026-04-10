using Mahjong.Lib.Tiles;
using System.Collections;

namespace Mahjong.Lib.Tests.Tiles;

public class TileKindList_GetEnumeratorTests
{
    [Fact]
    public void 反復処理_正しい順序で反復される()
    {
        // Arrange
        var list = new TileKindList(man: "123");
        var expectedTiles = new[] { TileKind.Man1, TileKind.Man2, TileKind.Man3 };

        // Act
        var actualTiles = list.ToArray();

        // Assert
        Assert.Equal(expectedTiles, actualTiles);
    }

    [Fact]
    public void 非ジェネリックIEnumerableとして列挙_正しい順序で取得できる()
    {
        // Arrange
        var list = new TileKindList(man: "12");

        // Act
        var actualTiles = new List<TileKind>();
        var enumerator = ((IEnumerable)list).GetEnumerator();
        while (enumerator.MoveNext())
        {
            actualTiles.Add((TileKind)enumerator.Current);
        }

        // Assert
        Assert.Equal(TileKind.Man1, actualTiles[0]);
        Assert.Equal(TileKind.Man2, actualTiles[1]);
    }
}
