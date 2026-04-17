using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Notifications.Bodies;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Notifications;

public class PlayerResponseExtensions_ToBodyTests
{
    [Fact]
    public void OkResponse_OkResponseBodyに変換される()
    {
        // Arrange
        var response = new OkResponse();

        // Act
        var body = response.ToBody();

        // Assert
        Assert.IsType<OkResponseBody>(body);
    }

    [Fact]
    public void PassResponse_OkResponseBodyに変換される()
    {
        var body = new PassResponse().ToBody();
        Assert.IsType<OkResponseBody>(body);
    }

    [Fact]
    public void KanPassResponse_OkResponseBodyに変換される()
    {
        var body = new KanPassResponse().ToBody();
        Assert.IsType<OkResponseBody>(body);
    }

    [Fact]
    public void ChiResponse_CallResponseBodyChiに変換される()
    {
        // Arrange
        var tiles = ImmutableArray.Create(new Tile(0), new Tile(4));
        var response = new ChiResponse(tiles);

        // Act
        var body = response.ToBody();

        // Assert
        var actual = Assert.IsType<CallResponseBody>(body);
        Assert.Equal(CallType.Chi, actual.CallType);
        Assert.Equal(tiles, actual.HandTiles);
    }

    [Fact]
    public void PonResponse_CallResponseBodyPonに変換される()
    {
        var tiles = ImmutableArray.Create(new Tile(0), new Tile(1));
        var body = new PonResponse(tiles).ToBody();
        var actual = Assert.IsType<CallResponseBody>(body);
        Assert.Equal(CallType.Pon, actual.CallType);
        Assert.Equal(tiles, actual.HandTiles);
    }

    [Fact]
    public void DaiminkanResponse_CallResponseBodyDaiminkanに変換される()
    {
        var tiles = ImmutableArray.Create(new Tile(0), new Tile(1), new Tile(2));
        var body = new DaiminkanResponse(tiles).ToBody();
        var actual = Assert.IsType<CallResponseBody>(body);
        Assert.Equal(CallType.Daiminkan, actual.CallType);
        Assert.Equal(tiles, actual.HandTiles);
    }

    [Fact]
    public void RonResponse_WinResponseBodyに変換される()
    {
        Assert.IsType<WinResponseBody>(new RonResponse().ToBody());
    }

    [Fact]
    public void ChankanRonResponse_WinResponseBodyに変換される()
    {
        Assert.IsType<WinResponseBody>(new ChankanRonResponse().ToBody());
    }

    [Fact]
    public void DahaiResponse_DahaiResponseBodyに変換される()
    {
        // Arrange
        var tile = new Tile(5);
        var response = new DahaiResponse(tile, IsRiichi: true);

        // Act
        var body = response.ToBody();

        // Assert
        var actual = Assert.IsType<DahaiResponseBody>(body);
        Assert.Equal(tile, actual.Tile);
        Assert.True(actual.IsRiichi);
    }

    [Fact]
    public void AnkanResponse_KanResponseBodyAnkanに変換される()
    {
        var tile = new Tile(0);
        var body = new AnkanResponse(tile).ToBody();
        var actual = Assert.IsType<KanResponseBody>(body);
        Assert.Equal(CallType.Ankan, actual.CallType);
        Assert.Equal(tile, actual.Tile);
    }

    [Fact]
    public void KakanResponse_KanResponseBodyKakanに変換される()
    {
        var tile = new Tile(0);
        var body = new KakanResponse(tile).ToBody();
        var actual = Assert.IsType<KanResponseBody>(body);
        Assert.Equal(CallType.Kakan, actual.CallType);
        Assert.Equal(tile, actual.Tile);
    }

    [Fact]
    public void TsumoAgariResponse_WinResponseBodyに変換される()
    {
        Assert.IsType<WinResponseBody>(new TsumoAgariResponse().ToBody());
    }

    [Fact]
    public void KyuushuKyuuhaiResponse_RyuukyokuResponseBodyに変換される()
    {
        Assert.IsType<RyuukyokuResponseBody>(new KyuushuKyuuhaiResponse().ToBody());
    }

    [Fact]
    public void RinshanTsumoResponse_WinResponseBodyに変換される()
    {
        Assert.IsType<WinResponseBody>(new RinshanTsumoResponse().ToBody());
    }

    [Fact]
    public void KanTsumoDahaiResponse_DahaiResponseBodyに変換される()
    {
        var tile = new Tile(10);
        var body = new KanTsumoDahaiResponse(tile).ToBody();
        var actual = Assert.IsType<DahaiResponseBody>(body);
        Assert.Equal(tile, actual.Tile);
        Assert.False(actual.IsRiichi);
    }

    [Fact]
    public void KanTsumoAnkanResponse_KanResponseBodyAnkanに変換される()
    {
        var tile = new Tile(0);
        var body = new KanTsumoAnkanResponse(tile).ToBody();
        var actual = Assert.IsType<KanResponseBody>(body);
        Assert.Equal(CallType.Ankan, actual.CallType);
        Assert.Equal(tile, actual.Tile);
    }

    [Fact]
    public void KanTsumoKakanResponse_KanResponseBodyKakanに変換される()
    {
        var tile = new Tile(0);
        var body = new KanTsumoKakanResponse(tile).ToBody();
        var actual = Assert.IsType<KanResponseBody>(body);
        Assert.Equal(CallType.Kakan, actual.CallType);
        Assert.Equal(tile, actual.Tile);
    }

    [Fact]
    public void Responseがnull_ArgumentNullExceptionが発生する()
    {
        // Act
        PlayerResponse response = null!;
        var ex = Record.Exception(response.ToBody);

        // Assert
        Assert.IsType<ArgumentNullException>(ex);
    }
}
