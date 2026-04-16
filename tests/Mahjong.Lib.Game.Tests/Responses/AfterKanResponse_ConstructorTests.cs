using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Tests.Responses;

public class AfterKanResponse_ConstructorTests
{
    [Fact]
    public void 全サブ型がAfterKanResponseを継承している()
    {
        // Assert
        Assert.IsType<AfterKanResponse>(new ChankanRonResponse(), exactMatch: false);
        Assert.IsType<AfterKanResponse>(new KanPassResponse(), exactMatch: false);
    }

    [Fact]
    public void 全サブ型がPlayerResponseを継承している()
    {
        // Assert
        Assert.IsType<PlayerResponse>(new ChankanRonResponse(), exactMatch: false);
        Assert.IsType<PlayerResponse>(new KanPassResponse(), exactMatch: false);
    }
}
