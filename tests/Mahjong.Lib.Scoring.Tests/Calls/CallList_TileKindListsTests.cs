using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Calls;

public class CallList_TileKindListsTests
{
    [Fact]
    public void 副露のTileKindListのコレクションを返す()
    {
        // Arrange
        var calls = new[]
        {
            Call.Chi(new TileKindList(man: "123")),
            Call.Pon(new TileKindList(man: "111")),
        };
        var callList = new CallList(calls);

        // Act
        var tileKindLists = callList.TileKindLists;

        // Assert
        Assert.Equal(2, tileKindLists.Count);
        Assert.Equal(calls[0].TileKindList, tileKindLists[0]);
        Assert.Equal(calls[1].TileKindList, tileKindLists[1]);
    }
}
