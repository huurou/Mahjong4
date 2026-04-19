using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Players;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Inquiries;

public class RoundInquirySpec_ConstructorTests
{
    [Fact]
    public void PhaseとPlayerSpecsが保持される()
    {
        // Arrange
        var playerSpecs = ImmutableList.Create(
            new PlayerInquirySpec(
                new PlayerIndex(0),
                [new OkCandidate()]
            )
        );

        // Act
        var spec = new RoundInquirySpec(RoundInquiryPhase.Haipai, playerSpecs, null);

        // Assert
        Assert.Equal(RoundInquiryPhase.Haipai, spec.Phase);
        Assert.Single(spec.PlayerSpecs);
        Assert.Equal(new PlayerIndex(0), spec.PlayerSpecs[0].PlayerIndex);
        Assert.Single(spec.PlayerSpecs[0].CandidateList);
    }
}
