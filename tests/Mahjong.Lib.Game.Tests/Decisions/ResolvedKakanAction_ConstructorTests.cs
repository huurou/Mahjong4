using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Decisions;

public class ResolvedKakanAction_ConstructorTests
{
    [Fact]
    public void 牌を1枚指定_正常に生成される()
    {
        // Act
        var action = new ResolvedKakanAction(new Tile(16));

        // Assert
        Assert.Equal(16, action.Tile.Id);
        Assert.IsType<ResolvedKanAction>(action, exactMatch: false);
    }
}
