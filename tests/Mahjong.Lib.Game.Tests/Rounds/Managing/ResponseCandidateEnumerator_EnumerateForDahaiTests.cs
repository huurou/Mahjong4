using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tests.Rounds;
using Mahjong.Lib.Game.Tiles;
using Moq;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds.Managing;

public class ResponseCandidateEnumerator_EnumerateForDahaiTests
{
    [Fact]
    public void 応答者は常にOkCandidateを提示される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var responder = new PlayerIndex(2);
        var enumerator = new ResponseCandidateEnumerator(RoundTestHelper.NoOpTenpaiChecker, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForDahai(round, responder, new Tile(0));

        // Assert
        Assert.True(candidates.HasCandidate<OkCandidate>());
    }

    [Fact]
    public void 放銃牌が待ちに含まれフリテンでない_RonCandidateが提示される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var responder = new PlayerIndex(2);
        var discardedTile = new Tile(60);
        var tenpaiMock = new Mock<ITenpaiChecker>();
        tenpaiMock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([discardedTile.Kind]);
        var enumerator = new ResponseCandidateEnumerator(tenpaiMock.Object, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForDahai(round, responder, discardedTile);

        // Assert
        Assert.True(candidates.HasCandidate<RonCandidate>());
    }

    [Fact]
    public void フリテン時_RonCandidateは提示されない()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var responder = new PlayerIndex(2);
        var discardedTile = new Tile(60);
        var status = round.PlayerRoundStatusArray[responder] with { IsFuriten = true };
        round = round with { PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(responder, status) };
        var tenpaiMock = new Mock<ITenpaiChecker>();
        tenpaiMock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([discardedTile.Kind]);
        var enumerator = new ResponseCandidateEnumerator(tenpaiMock.Object, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForDahai(round, responder, discardedTile);

        // Assert
        Assert.False(candidates.HasCandidate<RonCandidate>());
    }

    [Fact]
    public void 下家以外_ChiCandidateは提示されない()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        // 打牌者は Turn (dealer=0)。下家は PlayerIndex(1)。ここでは PlayerIndex(2) (対面) を応答者とする
        var responder = new PlayerIndex(2);
        var discardedTile = new Tile(60);
        var injectTiles = round.HandArray[responder].Concat([new Tile(56), new Tile(64)]).Take(13);
        round = RoundTestHelper.InjectHand(round, responder, injectTiles);
        var enumerator = new ResponseCandidateEnumerator(RoundTestHelper.NoOpTenpaiChecker, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForDahai(round, responder, discardedTile);

        // Assert
        Assert.False(candidates.HasCandidate<ChiCandidate>());
    }

    [Fact]
    public void 手牌に同種2枚_PonCandidateが提示される()
    {
        // Arrange: 応答者の手牌に同種 2 枚を注入
        var round = RoundTestHelper.CreateRound().Haipai();
        var responder = new PlayerIndex(2);
        var discardedTile = new Tile(60);
        var tilesOfSameKind = new[] { new Tile(61), new Tile(62) };
        var injectTiles = round.HandArray[responder].Take(11).Concat(tilesOfSameKind);
        round = RoundTestHelper.InjectHand(round, responder, injectTiles);
        var enumerator = new ResponseCandidateEnumerator(RoundTestHelper.NoOpTenpaiChecker, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForDahai(round, responder, discardedTile);

        // Assert
        var pons = candidates.GetCandidates<PonCandidate>().ToImmutableArray();
        Assert.Single(pons);
        Assert.Equal(2, pons[0].HandTiles.Length);
    }

    [Fact]
    public void 立直中_ChiやPon候補は提示されない()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var responder = new PlayerIndex(2);
        var discardedTile = new Tile(60);
        var tilesOfSameKind = new[] { new Tile(61), new Tile(62) };
        var injectTiles = round.HandArray[responder].Take(11).Concat(tilesOfSameKind);
        round = RoundTestHelper.InjectHand(round, responder, injectTiles);
        var status = round.PlayerRoundStatusArray[responder] with { IsRiichi = true };
        round = round with { PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(responder, status) };
        var enumerator = new ResponseCandidateEnumerator(RoundTestHelper.NoOpTenpaiChecker, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForDahai(round, responder, discardedTile);

        // Assert
        Assert.False(candidates.HasCandidate<ChiCandidate>());
        Assert.False(candidates.HasCandidate<PonCandidate>());
    }
}
