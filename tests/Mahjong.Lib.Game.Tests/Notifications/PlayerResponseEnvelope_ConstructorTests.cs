using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Notifications.Bodies;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Notifications;

public class PlayerResponseEnvelope_ConstructorTests
{
    [Fact]
    public void WireDTO_全フィールドが保持される()
    {
        // Arrange
        var id = Guid.NewGuid();
        var body = new DahaiResponseBody(new Tile(10), IsRiichi: true);

        // Act
        var envelope = new PlayerResponseEnvelope(
            id,
            1,
            new PlayerIndex(2),
            body
        );

        // Assert
        Assert.Equal(id, envelope.NotificationId);
        Assert.Equal(1, envelope.RoundRevision);
        Assert.Equal(new PlayerIndex(2), envelope.PlayerIndex);
        Assert.IsType<DahaiResponseBody>(envelope.Body);
    }

    [Fact]
    public void ResponseBody_全サブ型が生成可能()
    {
        // Assert
        Assert.IsType<ResponseBody>(new OkResponseBody(), exactMatch: false);
        Assert.IsType<ResponseBody>(new DahaiResponseBody(new Tile(0)), exactMatch: false);
        Assert.IsType<ResponseBody>(new CallResponseBody(CallType.Chi, []), exactMatch: false);
        Assert.IsType<ResponseBody>(new KanResponseBody(CallType.Ankan, new Tile(0)), exactMatch: false);
        Assert.IsType<ResponseBody>(new WinResponseBody(), exactMatch: false);
        Assert.IsType<ResponseBody>(new RyuukyokuResponseBody(), exactMatch: false);
    }
}
