using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Responses;

public class AfterTsumoResponse_ConstructorTests
{
    [Fact]
    public void DahaiResponse_フィールドが保持される()
    {
        // Arrange & Act
        var response = new DahaiResponse(new Tile(10), IsRiichi: true);

        // Assert
        Assert.Equal(10, response.Tile.Id);
        Assert.True(response.IsRiichi);
    }

    [Fact]
    public void DahaiResponse_IsRiichiの既定値はfalse()
    {
        // Act
        var response = new DahaiResponse(new Tile(0));

        // Assert
        Assert.False(response.IsRiichi);
    }

    [Fact]
    public void AnkanResponse_Tileが保持される()
    {
        // Act
        var response = new AnkanResponse(new Tile(0));

        // Assert
        Assert.Equal(0, response.Tile.Id);
    }

    [Fact]
    public void KakanResponse_Tileが保持される()
    {
        // Act
        var response = new KakanResponse(new Tile(16));

        // Assert
        Assert.Equal(16, response.Tile.Id);
    }

    [Fact]
    public void 全サブ型がAfterTsumoResponseを継承している()
    {
        // Assert
        Assert.IsType<AfterTsumoResponse>(new DahaiResponse(new Tile(0)), exactMatch: false);
        Assert.IsType<AfterTsumoResponse>(new AnkanResponse(new Tile(0)), exactMatch: false);
        Assert.IsType<AfterTsumoResponse>(new KakanResponse(new Tile(0)), exactMatch: false);
        Assert.IsType<AfterTsumoResponse>(new TsumoAgariResponse(), exactMatch: false);
        Assert.IsType<AfterTsumoResponse>(new KyuushuKyuuhaiResponse(), exactMatch: false);
    }

    [Fact]
    public void 全サブ型がPlayerResponseを継承している()
    {
        // Assert
        Assert.IsType<PlayerResponse>(new DahaiResponse(new Tile(0)), exactMatch: false);
        Assert.IsType<PlayerResponse>(new TsumoAgariResponse(), exactMatch: false);
        Assert.IsType<PlayerResponse>(new KyuushuKyuuhaiResponse(), exactMatch: false);
    }
}
