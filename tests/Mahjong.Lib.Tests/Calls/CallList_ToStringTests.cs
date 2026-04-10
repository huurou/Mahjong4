using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Calls;

public class CallList_ToStringTests
{
    [Fact]
    public void 副露あり_スペース区切りの文字列を返す()
    {
        // Arrange
        var callList = new CallList([
            Call.Chi(new TileKindList(man: "123")),
            Call.Pon(new TileKindList(man: "111")),
        ]);

        // Act
        var result = callList.ToString();

        // Assert
        Assert.Equal("チー-一二三 ポン-一一一", result);
    }

    [Fact]
    public void 空のCallList_空文字列を返す()
    {
        // Arrange
        var callList = new CallList();

        // Act
        var result = callList.ToString();

        // Assert
        Assert.Equal("", result);
    }
}
