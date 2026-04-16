using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Responses;

public class AfterKanTsumoResponse_ConstructorTests
{
    [Fact]
    public void KanTsumoDahaiResponse_フィールドが保持される()
    {
        // Act
        var response = new KanTsumoDahaiResponse(new Tile(10), IsRiichi: true);

        // Assert
        Assert.Equal(10, response.Tile.Id);
        Assert.True(response.IsRiichi);
    }

    [Fact]
    public void 全サブ型がAfterKanTsumoResponseを継承している()
    {
        // Assert
        Assert.IsType<AfterKanTsumoResponse>(new RinshanTsumoResponse(), exactMatch: false);
        Assert.IsType<AfterKanTsumoResponse>(new KanTsumoDahaiResponse(new Tile(0)), exactMatch: false);
        Assert.IsType<AfterKanTsumoResponse>(new KanTsumoAnkanResponse(new Tile(0)), exactMatch: false);
        Assert.IsType<AfterKanTsumoResponse>(new KanTsumoKakanResponse(new Tile(0)), exactMatch: false);
    }

    [Fact]
    public void 全サブ型がPlayerResponseを継承している()
    {
        // Assert
        Assert.IsType<PlayerResponse>(new RinshanTsumoResponse(), exactMatch: false);
        Assert.IsType<PlayerResponse>(new KanTsumoDahaiResponse(new Tile(0)), exactMatch: false);
    }
}
