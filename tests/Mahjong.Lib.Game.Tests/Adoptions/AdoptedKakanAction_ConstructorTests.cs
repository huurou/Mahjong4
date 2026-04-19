using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Adoptions;

public class AdoptedKakanAction_ConstructorTests
{
    [Fact]
    public void 牌を1枚指定_正常に生成される()
    {
        // Act
        var action = new AdoptedKakanAction(new Tile(16));

        // Assert
        Assert.Equal(16, action.Tile.Id);
        Assert.IsType<AdoptedKanAction>(action, exactMatch: false);
    }
}
