using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Decisions;
using Mahjong.Lib.Game.Players;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Decisions;

public class RoundDecisionSpec_ConstructorTests
{
    [Fact]
    public void PhaseとPlayerSpecsが保持される()
    {
        // Arrange
        var playerSpecs = ImmutableList.Create(
            new PlayerDecisionSpec(
                new PlayerIndex(0),
                [new OkCandidate()]
            )
        );

        // Act
        var spec = new RoundDecisionSpec(RoundDecisionPhase.Haipai, playerSpecs);

        // Assert
        Assert.Equal(RoundDecisionPhase.Haipai, spec.Phase);
        Assert.Single(spec.PlayerSpecs);
        Assert.Equal(new PlayerIndex(0), spec.PlayerSpecs[0].PlayerIndex);
        Assert.Single(spec.PlayerSpecs[0].CandidateList);
    }
}
