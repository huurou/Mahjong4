using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Tests.Rounds;

namespace Mahjong.Lib.Game.Tests.Rounds.Managing;

public class ResponseCandidateEnumerator_EnumerateForKanTsumoTests
{
    [Fact]
    public void 通常の手牌14枚_DahaiCandidateが提示される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var enumerator = new ResponseCandidateEnumerator(RoundTestHelper.NoOpTenpaiChecker, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForKanTsumo(round, round.Turn);

        // Assert
        Assert.True(candidates.HasCandidate<DahaiCandidate>());
    }

    [Fact]
    public void 手牌に同種4枚_AnkanCandidateが提示される()
    {
        // Arrange: 親 yama[135,134,133,132] は全 kind 33
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var enumerator = new ResponseCandidateEnumerator(RoundTestHelper.NoOpTenpaiChecker, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForKanTsumo(round, round.Turn);

        // Assert
        Assert.True(candidates.HasCandidate<AnkanCandidate>());
    }

    [Fact]
    public void OkCandidateは提示されない()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var enumerator = new ResponseCandidateEnumerator(RoundTestHelper.NoOpTenpaiChecker, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForKanTsumo(round, round.Turn);

        // Assert
        Assert.False(candidates.HasCandidate<OkCandidate>());
    }
}

public class ResponseCandidateEnumerator_EnumerateForAfterKanTsumoTests
{
    [Fact]
    public void 通常の手牌14枚_DahaiCandidateが提示される()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var enumerator = new ResponseCandidateEnumerator(RoundTestHelper.NoOpTenpaiChecker, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForAfterKanTsumo(round, round.Turn);

        // Assert
        Assert.True(candidates.HasCandidate<DahaiCandidate>());
    }

    [Fact]
    public void TsumoAgariCandidateは提示されない()
    {
        // Arrange: テンパイ・待ちヒットが発生しても AfterKanTsumo ではツモ和了候補は提示されない仕様
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var enumerator = new ResponseCandidateEnumerator(RoundTestHelper.NoOpTenpaiChecker, new GameRules());

        // Act
        var candidates = enumerator.EnumerateForAfterKanTsumo(round, round.Turn);

        // Assert
        Assert.False(candidates.HasCandidate<TsumoAgariCandidate>());
    }
}
