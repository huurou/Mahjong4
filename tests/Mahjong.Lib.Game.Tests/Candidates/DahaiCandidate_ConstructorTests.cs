using Mahjong.Lib.Game.Candidates;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Candidates;

public class DahaiCandidate_ConstructorTests
{
    [Fact]
    public void DahaiOptionListを指定_正常に保持される()
    {
        // Arrange
        DahaiOptionList optionList = [
            new DahaiOption(new Tile(0), true),
            new DahaiOption(new Tile(4), false)
        ];

        // Act
        var candidate = new DahaiCandidate(optionList);

        // Assert
        Assert.Equal(2, candidate.DahaiOptionList.Count);
        Assert.Equal(0, candidate.DahaiOptionList[0].Tile.Id);
        Assert.True(candidate.DahaiOptionList[0].RiichiAvailable);
        Assert.Equal(4, candidate.DahaiOptionList[1].Tile.Id);
        Assert.False(candidate.DahaiOptionList[1].RiichiAvailable);
    }

    [Fact]
    public void ResponseCandidateを継承している()
    {
        // Arrange & Act
        var candidate = new DahaiCandidate([]);

        // Assert
        Assert.IsType<ResponseCandidate>(candidate, exactMatch: false);
    }
}
