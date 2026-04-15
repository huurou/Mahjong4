using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.States.RoundStates.Impl;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundEventResponseKan_ConstructorTests
{
    [Theory]
    [InlineData(CallType.Ankan)]
    [InlineData(CallType.Kakan)]
    public void 槓種別を指定_正常に作成される(CallType type)
    {
        // Act
        var evt = new RoundEventResponseKan(type, new Tile(0));

        // Assert
        Assert.Equal(type, evt.CallType);
    }

    [Theory]
    [InlineData(CallType.Chi)]
    [InlineData(CallType.Pon)]
    [InlineData(CallType.Daiminkan)]
    public void 副露種別を指定_ArgumentExceptionが発生する(CallType type)
    {
        // Act
        var ex = Record.Exception(() => new RoundEventResponseKan(type, new Tile(0)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }
}
