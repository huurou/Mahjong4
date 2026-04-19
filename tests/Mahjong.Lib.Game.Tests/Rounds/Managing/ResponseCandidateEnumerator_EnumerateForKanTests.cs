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

public class ResponseCandidateEnumerator_EnumerateForKanTests
{
    [Fact]
    public void 暗槓_待ちを含まない場合はOkCandidateのみ提示される()
    {
        // Arrange: 国士テンパイでないため ChankanRonCandidate は提示されない
        var round = RoundTestHelper.CreateRound().Haipai();
        var responder = new PlayerIndex(1);
        var kanTiles = ImmutableArray.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3));
        var enumerator = new ResponseCandidateEnumerator(RoundTestHelper.NoOpTenpaiChecker, new GameRules());

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
        var enumerator = new ResponseCandidateEnumerator(RoundTestHelper.NoOpTenpaiChecker, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForKan(round, responder, kanTiles, CallType.Kakan);

        // Assert
        Assert.False(candidates.HasCandidate<ChankanRonCandidate>());
        Assert.True(candidates.HasCandidate<OkCandidate>());
    }

    [Fact]
    public void 加槓でテンパイ待ちを含みフリテンでない_ChankanRonCandidateが提示される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var responder = new PlayerIndex(1);
        var kanTiles = ImmutableArray.Create(new Tile(4), new Tile(5), new Tile(6), new Tile(7));
        var tenpaiMock = new Mock<ITenpaiChecker>();
        tenpaiMock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([kanTiles[0].Kind]);
        var enumerator = new ResponseCandidateEnumerator(tenpaiMock.Object, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForKan(round, responder, kanTiles, CallType.Kakan);

        // Assert
        Assert.True(candidates.HasCandidate<ChankanRonCandidate>());
    }

    [Fact]
    public void 暗槓_手牌13枚全て幺九で暗槓牌種が待ちに含まれルール許可_ChankanRonCandidateが提示される()
    {
        // Arrange: 国士テンパイ相当の手牌 (13種すべて幺九) で一萬を暗槓
        var round = RoundTestHelper.CreateRound().Haipai();
        var responder = new PlayerIndex(1);
        var kanTiles = ImmutableArray.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3));
        var yaochuuHand = new Tile[]
        {
            new(0), new(32), new(36), new(68), new(72), new(104),
            new(108), new(112), new(116), new(120), new(124), new(128), new(132),
        };
        round = RoundTestHelper.InjectHand(round, responder, yaochuuHand);
        var tenpaiMock = new Mock<ITenpaiChecker>();
        tenpaiMock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([kanTiles[0].Kind]);
        var enumerator = new ResponseCandidateEnumerator(tenpaiMock.Object, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForKan(round, responder, kanTiles, CallType.Ankan);

        // Assert
        Assert.True(candidates.HasCandidate<ChankanRonCandidate>());
    }

    [Fact]
    public void 暗槓_手牌13枚全て幺九で暗槓牌種が待ちに含まるがルール不許可_ChankanRonCandidateは提示されない()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var responder = new PlayerIndex(1);
        var kanTiles = ImmutableArray.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3));
        var yaochuuHand = new Tile[]
        {
            new(0), new(32), new(36), new(68), new(72), new(104),
            new(108), new(112), new(116), new(120), new(124), new(128), new(132),
        };
        round = RoundTestHelper.InjectHand(round, responder, yaochuuHand);
        var tenpaiMock = new Mock<ITenpaiChecker>();
        tenpaiMock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([kanTiles[0].Kind]);
        var rules = new GameRules { AllowAnkanChankanForKokushi = false };
        var enumerator = new ResponseCandidateEnumerator(tenpaiMock.Object, rules);

        // Act
        var candidates = enumerator.EnumerateForKan(round, responder, kanTiles, CallType.Ankan);

        // Assert
        Assert.False(candidates.HasCandidate<ChankanRonCandidate>());
    }

    [Fact]
    public void 暗槓_手牌に幺九以外が含まれる_ChankanRonCandidateは提示されない()
    {
        // Arrange: 手牌に 2m (kind=1, 非幺九) を含む
        var round = RoundTestHelper.CreateRound().Haipai();
        var responder = new PlayerIndex(1);
        var kanTiles = ImmutableArray.Create(new Tile(0), new Tile(1), new Tile(2), new Tile(3));
        var mixedHand = new Tile[]
        {
            new(0), new(32), new(36), new(68), new(72), new(104),
            new(108), new(112), new(116), new(120), new(124), new(128), new(4),
        };
        round = RoundTestHelper.InjectHand(round, responder, mixedHand);
        var tenpaiMock = new Mock<ITenpaiChecker>();
        tenpaiMock.Setup(x => x.EnumerateWaitTileKinds(It.IsAny<Hand>(), It.IsAny<CallList>()))
            .Returns([kanTiles[0].Kind]);
        var enumerator = new ResponseCandidateEnumerator(tenpaiMock.Object, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForKan(round, responder, kanTiles, CallType.Ankan);

        // Assert
        Assert.False(candidates.HasCandidate<ChankanRonCandidate>());
    }
}
