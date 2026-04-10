using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using System.Collections;

namespace Mahjong.Lib.Tests.Calls;

public class CallList_GetEnumeratorTests
{
    [Fact]
    public void 副露のコレクションを反復処理できる()
    {
        // Arrange
        var calls = new[]
        {
            Call.Chi(new TileKindList(man: "123")),
            Call.Pon(new TileKindList(man: "111")),
        };
        var callList = new CallList(calls);

        // Act
        var result = new List<Call>();
        foreach (var call in callList)
        {
            result.Add(call);
        }

        // Assert
        Assert.Equal(calls, result);
    }

    [Fact]
    public void 非ジェネリックGetEnumerator_反復処理できる()
    {
        // Arrange
        var calls = new[]
        {
            Call.Chi(new TileKindList(man: "123")),
            Call.Pon(new TileKindList(man: "111")),
        };
        var callList = new CallList(calls);

        // Act
        var result = new List<Call>();
        IEnumerable enumerable = callList;
        foreach (Call call in enumerable)
        {
            result.Add(call);
        }

        // Assert
        Assert.Equal(calls, result);
    }

    [Fact]
    public void インデクサー_指定インデックスの副露を返す()
    {
        // Arrange
        var call1 = Call.Chi(new TileKindList(man: "123"));
        var call2 = Call.Pon(new TileKindList(man: "111"));
        var callList = new CallList([call1, call2]);

        // Act & Assert
        Assert.Equal(call1, callList[0]);
        Assert.Equal(call2, callList[1]);
    }
}
