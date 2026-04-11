using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Calls;

public class CallList_ConstructorTests
{
    [Fact]
    public void デフォルトコンストラクタ_空のCallListが作成される()
    {
        // Act
        var callList = new CallList();

        // Assert
        Assert.Equal(0, callList.Count);
        Assert.False(callList.HasOpen);
        Assert.Empty(callList.TileKindLists);
        Assert.Empty(callList);
    }

    [Fact]
    public void IEnumerableコンストラクタ_副露のコレクションから作成される()
    {
        // Arrange
        var calls = new[]
        {
            Call.Chi(new TileKindList(man: "123")),
            Call.Pon(new TileKindList(man: "111")),
        };

        // Act
        var callList = new CallList(calls);

        // Assert
        Assert.Equal(2, callList.Count);
        Assert.True(callList.HasOpen);
        Assert.Equal(calls, callList);
    }

    [Fact]
    public void コレクションビルダー_コレクション式で作成できる()
    {
        // Arrange
        var call1 = Call.Chi(new TileKindList(man: "123"));
        var call2 = Call.Pon(new TileKindList(man: "111"));

        // Act
        CallList callList = [call1, call2];

        // Assert
        Assert.Equal(2, callList.Count);
        Assert.Contains(call1, callList);
        Assert.Contains(call2, callList);
    }
}
