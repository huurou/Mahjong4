using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Tests.Responses;

public class AfterKanResponse_ConstructorTests
{
    [Fact]
    public void 全アクション派生型がAfterKanResponseを継承している()
    {
        // Assert
        Assert.IsType<AfterKanResponse>(new ChankanRonResponse(), exactMatch: false);
    }

    [Fact]
    public void ChankanRonResponseはPlayerResponseを継承している()
    {
        // Assert
        Assert.IsType<PlayerResponse>(new ChankanRonResponse(), exactMatch: false);
    }
}
