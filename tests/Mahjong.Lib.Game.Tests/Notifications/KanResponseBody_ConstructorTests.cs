using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Notifications.Bodies;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Notifications;

public class KanResponseBody_ConstructorTests
{
    [Theory]
    [InlineData(CallType.Ankan)]
    [InlineData(CallType.Kakan)]
    public void 有効なCallType_正常に生成される(CallType callType)
    {
        // Act
        var body = new KanResponseBody(callType, new Tile(0));

        // Assert
        Assert.Equal(callType, body.CallType);
    }

    [Theory]
    [InlineData(CallType.Chi)]
    [InlineData(CallType.Pon)]
    [InlineData(CallType.Daiminkan)]
    public void 不正なCallType_例外が発生する(CallType callType)
    {
        // Act
        var ex = Record.Exception(() => new KanResponseBody(callType, new Tile(0)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }
}
