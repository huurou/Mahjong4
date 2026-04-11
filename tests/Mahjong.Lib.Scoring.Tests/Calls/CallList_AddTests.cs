using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Calls;

public class CallList_AddTests
{
    [Fact]
    public void 副露を追加_新しいCallListが返される()
    {
        // Arrange
        var initialCall = Call.Chi(new TileKindList(man: "123"));
        var callList = new CallList([initialCall]);
        var newCall = Call.Pon(new TileKindList(man: "111"));

        // Act
        var result = callList.Add(newCall);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(initialCall, result);
        Assert.Contains(newCall, result);
        Assert.Equal(1, callList.Count);
    }

    [Fact]
    public void 不変性の確認_Addしても元のインスタンスは変更されない()
    {
        // Arrange
        var originalCall = Call.Chi(new TileKindList(man: "123"));
        var callList = new CallList([originalCall]);
        var newCall = Call.Pon(new TileKindList(man: "111"));

        // Act
        var newCallList = callList.Add(newCall);

        // Assert
        Assert.Equal(1, callList.Count);
        Assert.Equal(2, newCallList.Count);
        Assert.NotSame(callList, newCallList);
    }
}
