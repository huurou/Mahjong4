using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Adoptions;

public class AdoptedCallAction_ConstructorTests
{
    [Theory]
    [InlineData(CallType.Chi)]
    [InlineData(CallType.Pon)]
    [InlineData(CallType.Daiminkan)]
    public void 有効なCallType_正常に生成される(CallType callType)
    {
        // Act
        var action = new AdoptedCallAction(
            new PlayerIndex(0),
            callType,
            ImmutableList.Create(new Tile(0), new Tile(4))
        );

        // Assert
        Assert.Equal(callType, action.CallType);
    }

    [Theory]
    [InlineData(CallType.Ankan)]
    [InlineData(CallType.Kakan)]
    public void 不正なCallType_例外が発生する(CallType callType)
    {
        // Act
        var ex = Record.Exception(() => new AdoptedCallAction(
            new PlayerIndex(0),
            callType,
            ImmutableList.Create(new Tile(0), new Tile(4))
        ));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }
}
