using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_IsSuukaikanTests
{
    private static Call MakeAnkan(int kindId)
    {
        var baseId = kindId * 4;
        return new Call(
            CallType.Ankan,
            [new(baseId), new(baseId + 1), new(baseId + 2), new(baseId + 3)],
            new PlayerIndex(0),
            null
        );
    }

    [Fact]
    public void 同一プレイヤー四槓_false_四槓子狙いを妨げない()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var callListArray = round.CallListArray;
        for (var kind = 0; kind < 4; kind++)
        {
            callListArray = callListArray.AddCall(new PlayerIndex(0), MakeAnkan(kind));
        }
        round = round with { CallListArray = callListArray };

        // Act
        var result = round.IsSuukaikan();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void 二人で合計四槓_true()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var callListArray = round.CallListArray;
        callListArray = callListArray.AddCall(new PlayerIndex(0), MakeAnkan(0));
        callListArray = callListArray.AddCall(new PlayerIndex(0), MakeAnkan(1));
        callListArray = callListArray.AddCall(new PlayerIndex(1), MakeAnkan(2));
        callListArray = callListArray.AddCall(new PlayerIndex(1), MakeAnkan(3));
        round = round with { CallListArray = callListArray };

        // Act
        var result = round.IsSuukaikan();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void 四人各一槓_true()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var callListArray = round.CallListArray;
        for (var i = 0; i < 4; i++)
        {
            callListArray = callListArray.AddCall(new PlayerIndex(i), MakeAnkan(i));
        }
        round = round with { CallListArray = callListArray };

        // Act
        var result = round.IsSuukaikan();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void 総槓三回_false()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();
        var callListArray = round.CallListArray;
        callListArray = callListArray.AddCall(new PlayerIndex(0), MakeAnkan(0));
        callListArray = callListArray.AddCall(new PlayerIndex(1), MakeAnkan(1));
        callListArray = callListArray.AddCall(new PlayerIndex(2), MakeAnkan(2));
        round = round with { CallListArray = callListArray };

        // Act
        var result = round.IsSuukaikan();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void 槓なし_false()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound().Haipai();

        // Act
        var result = round.IsSuukaikan();

        // Assert
        Assert.False(result);
    }
}
