using Mahjong.Lib.Scoring.Tiles;
using System.Collections;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKindListList_GetEnumeratorTests
{
    [Fact]
    public void 反復処理_正しい順序で反復される()
    {
        // Arrange
        var expectedLists = new[]
        {
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
            new TileKindList(sou: "789"),
        };
        var list = new TileKindListList(expectedLists);

        // Act
        var actualLists = list.ToArray();

        // Assert
        Assert.Equal(expectedLists, actualLists);
    }

    [Fact]
    public void 非ジェネリックIEnumerableとして列挙_正しい順序で取得できる()
    {
        // Arrange
        var expectedLists = new[]
        {
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        };
        var list = new TileKindListList(expectedLists);

        // Act
        var actualLists = new List<TileKindList>();
        var enumerator = ((IEnumerable)list).GetEnumerator();
        while (enumerator.MoveNext())
        {
            actualLists.Add((TileKindList)enumerator.Current);
        }

        // Assert
        Assert.Equal(expectedLists, actualLists);
    }
}
