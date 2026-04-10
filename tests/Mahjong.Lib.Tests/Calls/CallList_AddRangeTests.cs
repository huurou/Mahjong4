using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Calls;

public class CallList_AddRangeTests
{
    [Fact]
    public void 複数の副露を追加_新しいCallListが返される()
    {
        // Arrange
        var initialCall = Call.Chi(new TileKindList(man: "123"));
        var callList = new CallList([initialCall]);
        var newCalls = new[]
        {
            Call.Pon(new TileKindList(man: "111")),
            Call.Ankan(new TileKindList(man: "2222")),
        };

        // Act
        var result = callList.AddRange(newCalls);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(initialCall, result);
        Assert.Contains(newCalls[0], result);
        Assert.Contains(newCalls[1], result);
        Assert.Equal(1, callList.Count);
    }
}
