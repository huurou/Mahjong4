using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds.Managing;

public class ResponseValidator_IsResponseInCandidatesTests
{
    [Fact]
    public void DahaiResponse_候補に含まれる牌_true()
    {
        // Arrange
        var candidates = new CandidateList(
        [
            new DahaiCandidate(new DahaiOptionList([
                new DahaiOption(new Tile(0), RiichiAvailable: false),
                new DahaiOption(new Tile(4), RiichiAvailable: true),
            ])),
        ]);

        // Act
        var result = ResponseValidator.IsResponseInCandidates(new DahaiResponse(new Tile(0)), candidates);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DahaiResponse_候補外の牌_false()
    {
        // Arrange
        var candidates = new CandidateList(
        [
            new DahaiCandidate(new DahaiOptionList([
                new DahaiOption(new Tile(0), RiichiAvailable: false),
            ])),
        ]);

        // Act
        var result = ResponseValidator.IsResponseInCandidates(new DahaiResponse(new Tile(4)), candidates);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void 立直宣言_RiichiAvailableでないOption_false()
    {
        // Arrange
        var candidates = new CandidateList(
        [
            new DahaiCandidate(new DahaiOptionList([
                new DahaiOption(new Tile(0), RiichiAvailable: false),
            ])),
        ]);

        // Act
        var result = ResponseValidator.IsResponseInCandidates(new DahaiResponse(new Tile(0), IsRiichi: true), candidates);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RonResponse_RonCandidate存在時_true()
    {
        // Arrange
        var candidates = new CandidateList(
        [
            new OkCandidate(),
            new RonCandidate(),
        ]);

        // Act
        var result = ResponseValidator.IsResponseInCandidates(new RonResponse(), candidates);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RonResponse_RonCandidate不在時_false()
    {
        // Arrange
        var candidates = new CandidateList(
        [
            new OkCandidate(),
        ]);

        // Act
        var result = ResponseValidator.IsResponseInCandidates(new RonResponse(), candidates);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AnkanResponse_候補と同じKind_true()
    {
        // Arrange: AnkanCandidate は具体的な 4 枚を持つが、AnkanResponse は牌種で指定
        var candidates = new CandidateList(
        [
            new AnkanCandidate([new Tile(0), new Tile(1), new Tile(2), new Tile(3)]),
        ]);

        // Act: 別 Id でも同 Kind (= 0) なら合法
        var result = ResponseValidator.IsResponseInCandidates(new AnkanResponse(new Tile(2)), candidates);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AnkanResponse_候補と異なるKind_false()
    {
        // Arrange
        var candidates = new CandidateList(
        [
            new AnkanCandidate([new Tile(0), new Tile(1), new Tile(2), new Tile(3)]),
        ]);

        // Act: Kind=1 (Tile.Id 4-7) は候補外
        var result = ResponseValidator.IsResponseInCandidates(new AnkanResponse(new Tile(4)), candidates);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ChiResponse_HandTilesがSequenceEqual_true()
    {
        // Arrange
        var candidates = new CandidateList(
        [
            new ChiCandidate([new Tile(0), new Tile(4)]),
        ]);

        // Act
        var result = ResponseValidator.IsResponseInCandidates(new ChiResponse([new Tile(0), new Tile(4)]), candidates);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ChiResponse_HandTiles順序違い_false()
    {
        // Arrange
        var candidates = new CandidateList(
        [
            new ChiCandidate([new Tile(0), new Tile(4)]),
        ]);

        // Act: 順序違い (SequenceEqual で一致しない)
        var result = ResponseValidator.IsResponseInCandidates(new ChiResponse([new Tile(4), new Tile(0)]), candidates);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void KakanResponse_候補と同じTileId_true()
    {
        // Arrange: KakanCandidate.Tile は完全一致 (Tile.Id) で判定される
        var candidates = new CandidateList(
        [
            new KakanCandidate(new Tile(2)),
        ]);

        // Act
        var result = ResponseValidator.IsResponseInCandidates(new KakanResponse(new Tile(2)), candidates);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void KakanResponse_候補と同種だが別TileId_false()
    {
        // Arrange: 赤ドラ採用・非採用は別候補として列挙されるため、同種別 Id は候補外
        var candidates = new CandidateList(
        [
            new KakanCandidate(new Tile(0)),
        ]);

        // Act: Kind=0 同種だが Tile.Id が異なる (Round.Kakan は Tile.Id 一致を要求するため候補外扱いが必要)
        var result = ResponseValidator.IsResponseInCandidates(new KakanResponse(new Tile(2)), candidates);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void KanTsumoKakanResponse_候補と同種だが別TileId_false()
    {
        // Arrange
        var candidates = new CandidateList(
        [
            new KakanCandidate(new Tile(0)),
        ]);

        // Act
        var result = ResponseValidator.IsResponseInCandidates(new KanTsumoKakanResponse(new Tile(2)), candidates);

        // Assert
        Assert.False(result);
    }
}
