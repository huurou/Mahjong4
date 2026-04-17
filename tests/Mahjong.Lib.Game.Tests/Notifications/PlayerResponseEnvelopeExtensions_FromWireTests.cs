using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Notifications.Bodies;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Notifications;

public class PlayerResponseEnvelopeExtensions_FromWireTests
{
    private const int ROUND_REVISION = 0;

    private static Guid NotificationId { get; } = Guid.NewGuid();
    private static PlayerIndex PlayerIndex { get; } = new(0);

    [Fact]
    public void Haipai_OkResponseBody_OkResponseに変換される()
    {
        var response = FromWire(new OkResponseBody(), RoundDecisionPhase.Haipai);
        Assert.IsType<OkResponse>(response);
    }

    [Fact]
    public void Tsumo_DahaiResponseBody_DahaiResponseに変換される()
    {
        var tile = new Tile(5);
        var response = FromWire(new DahaiResponseBody(tile, IsRiichi: true), RoundDecisionPhase.Tsumo);
        var actual = Assert.IsType<DahaiResponse>(response);
        Assert.Equal(tile, actual.Tile);
        Assert.True(actual.IsRiichi);
    }

    [Fact]
    public void Tsumo_KanResponseBodyAnkan_AnkanResponseに変換される()
    {
        var tile = new Tile(0);
        var response = FromWire(new KanResponseBody(CallType.Ankan, tile), RoundDecisionPhase.Tsumo);
        var actual = Assert.IsType<AnkanResponse>(response);
        Assert.Equal(tile, actual.Tile);
    }

    [Fact]
    public void Tsumo_KanResponseBodyKakan_KakanResponseに変換される()
    {
        var tile = new Tile(0);
        var response = FromWire(new KanResponseBody(CallType.Kakan, tile), RoundDecisionPhase.Tsumo);
        var actual = Assert.IsType<KakanResponse>(response);
        Assert.Equal(tile, actual.Tile);
    }

    [Fact]
    public void Tsumo_WinResponseBody_TsumoAgariResponseに変換される()
    {
        var response = FromWire(new WinResponseBody(), RoundDecisionPhase.Tsumo);
        Assert.IsType<TsumoAgariResponse>(response);
    }

    [Fact]
    public void Tsumo_RyuukyokuResponseBody_KyuushuKyuuhaiResponseに変換される()
    {
        var response = FromWire(new RyuukyokuResponseBody(), RoundDecisionPhase.Tsumo);
        Assert.IsType<KyuushuKyuuhaiResponse>(response);
    }

    [Fact]
    public void Dahai_OkResponseBody_PassResponseに変換される()
    {
        var response = FromWire(new OkResponseBody(), RoundDecisionPhase.Dahai);
        Assert.IsType<PassResponse>(response);
    }

    [Fact]
    public void Dahai_CallResponseBodyChi_ChiResponseに変換される()
    {
        var tiles = ImmutableList.Create(new Tile(0), new Tile(4));
        var response = FromWire(new CallResponseBody(CallType.Chi, tiles), RoundDecisionPhase.Dahai);
        var actual = Assert.IsType<ChiResponse>(response);
        Assert.Equal(tiles, actual.HandTiles);
    }

    [Fact]
    public void Dahai_CallResponseBodyPon_PonResponseに変換される()
    {
        var tiles = ImmutableList.Create(new Tile(0), new Tile(1));
        var response = FromWire(new CallResponseBody(CallType.Pon, tiles), RoundDecisionPhase.Dahai);
        var actual = Assert.IsType<PonResponse>(response);
        Assert.Equal(tiles, actual.HandTiles);
    }

    [Fact]
    public void Dahai_CallResponseBodyDaiminkan_DaiminkanResponseに変換される()
    {
        var tiles = ImmutableList.Create(new Tile(0), new Tile(1), new Tile(2));
        var response = FromWire(new CallResponseBody(CallType.Daiminkan, tiles), RoundDecisionPhase.Dahai);
        var actual = Assert.IsType<DaiminkanResponse>(response);
        Assert.Equal(tiles, actual.HandTiles);
    }

    [Fact]
    public void Dahai_WinResponseBody_RonResponseに変換される()
    {
        var response = FromWire(new WinResponseBody(), RoundDecisionPhase.Dahai);
        Assert.IsType<RonResponse>(response);
    }

    [Fact]
    public void Kan_OkResponseBody_KanPassResponseに変換される()
    {
        var response = FromWire(new OkResponseBody(), RoundDecisionPhase.Kan);
        Assert.IsType<KanPassResponse>(response);
    }

    [Fact]
    public void Kan_WinResponseBody_ChankanRonResponseに変換される()
    {
        var response = FromWire(new WinResponseBody(), RoundDecisionPhase.Kan);
        Assert.IsType<ChankanRonResponse>(response);
    }

    [Fact]
    public void KanTsumo_DahaiResponseBody_KanTsumoDahaiResponseに変換される()
    {
        var tile = new Tile(0);
        var response = FromWire(new DahaiResponseBody(tile), RoundDecisionPhase.KanTsumo);
        var actual = Assert.IsType<KanTsumoDahaiResponse>(response);
        Assert.Equal(tile, actual.Tile);
    }

    [Fact]
    public void KanTsumo_KanResponseBodyAnkan_KanTsumoAnkanResponseに変換される()
    {
        var tile = new Tile(0);
        var response = FromWire(new KanResponseBody(CallType.Ankan, tile), RoundDecisionPhase.KanTsumo);
        var actual = Assert.IsType<KanTsumoAnkanResponse>(response);
        Assert.Equal(tile, actual.Tile);
    }

    [Fact]
    public void KanTsumo_KanResponseBodyKakan_KanTsumoKakanResponseに変換される()
    {
        var tile = new Tile(0);
        var response = FromWire(new KanResponseBody(CallType.Kakan, tile), RoundDecisionPhase.KanTsumo);
        var actual = Assert.IsType<KanTsumoKakanResponse>(response);
        Assert.Equal(tile, actual.Tile);
    }

    [Fact]
    public void KanTsumo_WinResponseBody_RinshanTsumoResponseに変換される()
    {
        var response = FromWire(new WinResponseBody(), RoundDecisionPhase.KanTsumo);
        Assert.IsType<RinshanTsumoResponse>(response);
    }

    [Fact]
    public void AfterKanTsumo_DahaiResponseBody_KanTsumoDahaiResponseに変換される()
    {
        var tile = new Tile(0);
        var response = FromWire(new DahaiResponseBody(tile), RoundDecisionPhase.AfterKanTsumo);
        Assert.IsType<KanTsumoDahaiResponse>(response);
    }

    [Fact]
    public void AfterKanTsumo_KanResponseBodyAnkan_KanTsumoAnkanResponseに変換される()
    {
        var tile = new Tile(0);
        var response = FromWire(new KanResponseBody(CallType.Ankan, tile), RoundDecisionPhase.AfterKanTsumo);
        var actual = Assert.IsType<KanTsumoAnkanResponse>(response);
        Assert.Equal(tile, actual.Tile);
    }

    [Fact]
    public void AfterKanTsumo_KanResponseBodyKakan_KanTsumoKakanResponseに変換される()
    {
        var tile = new Tile(0);
        var response = FromWire(new KanResponseBody(CallType.Kakan, tile), RoundDecisionPhase.AfterKanTsumo);
        var actual = Assert.IsType<KanTsumoKakanResponse>(response);
        Assert.Equal(tile, actual.Tile);
    }

    [Fact]
    public void AfterKanTsumo_WinResponseBody_ArgumentExceptionが発生する()
    {
        // AfterKanTsumo では和了不可 (Design.md 準拠 KanTsumo で和了済み扱い)
        var ex = Record.Exception(() => FromWire(new WinResponseBody(), RoundDecisionPhase.AfterKanTsumo));
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void Haipai_不正Body_ArgumentExceptionが発生する()
    {
        var ex = Record.Exception(() => FromWire(new WinResponseBody(), RoundDecisionPhase.Haipai));
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void Tsumo_不正Body_ArgumentExceptionが発生する()
    {
        var ex = Record.Exception(() => FromWire(new OkResponseBody(), RoundDecisionPhase.Tsumo));
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void Dahai_不正Body_ArgumentExceptionが発生する()
    {
        var ex = Record.Exception(() => FromWire(new DahaiResponseBody(new Tile(0)), RoundDecisionPhase.Dahai));
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void Kan_不正Body_ArgumentExceptionが発生する()
    {
        var ex = Record.Exception(() => FromWire(new DahaiResponseBody(new Tile(0)), RoundDecisionPhase.Kan));
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void Envelopeがnull_ArgumentNullExceptionが発生する()
    {
        // Act
        PlayerResponseEnvelope envelope = null!;
        var ex = Record.Exception(() => envelope.FromWire(RoundDecisionPhase.Haipai));

        // Assert
        Assert.IsType<ArgumentNullException>(ex);
    }

    [Fact]
    public void ラウンドトリップ_DahaiResponse_ToBodyしてFromWireで元と等価()
    {
        // Arrange
        var original = new DahaiResponse(new Tile(7), IsRiichi: true);

        // Act
        var body = original.ToBody();
        var roundtrip = FromWire(body, RoundDecisionPhase.Tsumo);

        // Assert
        Assert.Equal(original, roundtrip);
    }

    [Fact]
    public void ラウンドトリップ_ChiResponse_ToBodyしてFromWireで元と等価()
    {
        // Arrange
        var tiles = ImmutableArray.Create(new Tile(0), new Tile(4));
        var original = new ChiResponse(tiles);

        // Act
        var body = original.ToBody();
        var roundtrip = FromWire(body, RoundDecisionPhase.Dahai);

        // Assert
        Assert.Equal(original, roundtrip);
    }

    [Fact]
    public void ラウンドトリップ_AnkanResponse_ToBodyしてFromWireで元と等価()
    {
        // Arrange
        var original = new AnkanResponse(new Tile(8));

        // Act
        var body = original.ToBody();
        var roundtrip = FromWire(body, RoundDecisionPhase.Tsumo);

        // Assert
        Assert.Equal(original, roundtrip);
    }

    [Fact]
    public void ラウンドトリップ_KanTsumoKakanResponse_ToBodyしてFromWireで元と等価()
    {
        // Arrange
        var original = new KanTsumoKakanResponse(new Tile(12));

        // Act
        var body = original.ToBody();
        var roundtrip = FromWire(body, RoundDecisionPhase.KanTsumo);

        // Assert
        Assert.Equal(original, roundtrip);
    }

    private static PlayerResponse FromWire(ResponseBody body, RoundDecisionPhase phase)
    {
        var envelope = new PlayerResponseEnvelope(NotificationId, ROUND_REVISION, PlayerIndex, body);
        return envelope.FromWire(phase);
    }
}
