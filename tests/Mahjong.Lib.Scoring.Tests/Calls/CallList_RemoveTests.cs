using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Calls;

public class CallList_RemoveTests
{
    [Fact]
    public void 存在する副露を削除_新しいCallListが返される()
    {
        // Arrange
        var call1 = Call.Chi(new TileKindList(man: "123"));
        var call2 = Call.Pon(new TileKindList(man: "111"));
        var callList = new CallList([call1, call2]);

        // Act
        var result = callList.Remove(call1);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.DoesNotContain(call1, result);
        Assert.Contains(call2, result);
        Assert.Equal(2, callList.Count);
    }

    [Fact]
    public void 存在しない副露を削除_ArgumentExceptionが発生する()
    {
        // Arrange
        var call1 = Call.Chi(new TileKindList(man: "123"));
        var callList = new CallList([call1]);
        var nonExistentCall = Call.Pon(new TileKindList(man: "111"));

        // Act
        var ex = Record.Exception(() => callList.Remove(nonExistentCall));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Equal("call", ((ArgumentException)ex).ParamName);
    }

    [Fact]
    public void 不変性の確認_Removeしても元のインスタンスは変更されない()
    {
        // Arrange
        var call1 = Call.Chi(new TileKindList(man: "123"));
        var call2 = Call.Pon(new TileKindList(man: "111"));
        var callList = new CallList([call1, call2]);

        // Act
        var newCallList = callList.Remove(call1);

        // Assert
        Assert.Equal(2, callList.Count);
        Assert.Equal(1, newCallList.Count);
        Assert.NotSame(callList, newCallList);
    }
}
