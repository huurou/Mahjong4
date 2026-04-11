using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Calls;

public class CallList_CountTests
{
    [Fact]
    public void 副露が存在する場合_副露の数を返す()
    {
        // Arrange
        var callList = new CallList([
            Call.Chi(new TileKindList(man: "123")),
            Call.Pon(new TileKindList(man: "111")),
            Call.Ankan(new TileKindList(man: "2222")),
        ]);

        // Act & Assert
        Assert.Equal(3, callList.Count);
    }

    [Fact]
    public void 空のCallListの場合_0を返す()
    {
        // Arrange
        var callList = new CallList();

        // Act & Assert
        Assert.Equal(0, callList.Count);
    }
}
