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
        var spec = new RoundInquirySpec(RoundInquiryPhase.Haipai, playerSpecs, [], new PlayerIndex(0));

        // Assert
        Assert.Equal(RoundInquiryPhase.Haipai, spec.Phase);
        Assert.Single(spec.PlayerSpecs);
        Assert.Equal(new PlayerIndex(0), spec.PlayerSpecs[0].PlayerIndex);
        Assert.Single(spec.PlayerSpecs[0].CandidateList);
        Assert.Empty(spec.InquiredPlayerIndices);
    }

    [Fact]
    public void InquiredPlayerIndicesが保持されIsInquiredで判定できる()
    {
        // Arrange
        var playerSpecs = ImmutableList.Create(
            new PlayerInquirySpec(new PlayerIndex(1), [new OkCandidate()])
        );
        var inquired = ImmutableArray.Create(new PlayerIndex(1));

        // Act
        var spec = new RoundInquirySpec(RoundInquiryPhase.Tsumo, playerSpecs, inquired, new PlayerIndex(0));

        // Assert
        Assert.Equal(inquired, spec.InquiredPlayerIndices);
        Assert.True(spec.IsInquired(new PlayerIndex(1)));
        Assert.False(spec.IsInquired(new PlayerIndex(0)));
    }
}
