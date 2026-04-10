using Mahjong.Lib.Fus;
using System.Collections;

namespace Mahjong.Lib.Tests.Fus;

public class FuList_GetEnumeratorTests
{
    [Fact]
    public void 符のコレクションを反復処理できる()
    {
        // Arrange
        var expectedFus = new[] { Fu.Futei, Fu.Menzen, Fu.Tsumo };
        var fuList = new FuList(expectedFus);

        // Act
        var actualFus = new List<Fu>();
        foreach (var fu in fuList)
        {
            actualFus.Add(fu);
        }

        // Assert
        Assert.Equal(expectedFus.Length, actualFus.Count);
        for (var i = 0; i < expectedFus.Length; i++)
        {
            Assert.Equal(expectedFus[i], actualFus[i]);
        }
    }

    [Fact]
    public void LINQ拡張メソッドが使用できる()
    {
        // Arrange
        var fuList = new FuList([Fu.Futei, Fu.Menzen, Fu.Tsumo, Fu.Kanchan]);

        // Act & Assert
        Assert.Equal(4, fuList.Count);
        Assert.Contains(Fu.Futei, fuList);
        Assert.Contains(fuList, x => x.Value == 20);
        Assert.Equal(34, fuList.Sum(x => x.Value)); // 20 + 10 + 2 + 2 = 34
    }

    [Fact]
    public void 非ジェネリックGetEnumerator_反復処理できる()
    {
        // Arrange
        var fuList = new FuList([Fu.Futei, Fu.Tsumo]);

        // Act
        var enumerator = ((IEnumerable)fuList).GetEnumerator();
        var items = new List<object?>();
        while (enumerator.MoveNext())
        {
            items.Add(enumerator.Current);
        }

        // Assert
        Assert.Equal(2, items.Count);
        Assert.Equal(Fu.Futei, items[0]);
        Assert.Equal(Fu.Tsumo, items[1]);
    }
}
