using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Tests.Rounds;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds.Managing;

public class ResponseCandidateEnumerator_EnumerateForKanTests
{
    [Fact]
    public void 暗槓_待ちを含まない場合はOkCandidateのみ提示される()
    {
        // Arrange: 国士テンパイでないため ChankanRonCandidate は提示されない
        var round = RoundTestHelper.CreateRound().Haipai();
        var responder = new PlayerIndex(1);
        var kanTiles = ImmutableArray.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3));
        var enumerator = new ResponseCandidateEnumerator(new GameRules());

        // Act
        var candidates = enumerator.EnumerateForKan(round, responder, kanTiles, CallType.Ankan);

        // Assert
        Assert.True(candidates.HasCandidate<OkCandidate>());
        Assert.False(candidates.HasCandidate<ChankanRonCandidate>());
    }

    [Fact]
    public void 加槓で待ちを含まない_ChankanRonCandidateは提示されない()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var responder = new PlayerIndex(1);
        var kanTiles = ImmutableArray.Create(new Tile(4), new Tile(5), new Tile(6), new Tile(7));
        var enumerator = new ResponseCandidateEnumerator(new GameRules());

        // Act
        var candidates = enumerator.EnumerateForKan(round, responder, kanTiles, CallType.Kakan);

        // Assert
        Assert.False(candidates.HasCandidate<ChankanRonCandidate>());
        Assert.True(candidates.HasCandidate<OkCandidate>());
    }
}
