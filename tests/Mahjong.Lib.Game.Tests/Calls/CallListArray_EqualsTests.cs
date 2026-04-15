using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Calls;

public class CallListArray_EqualsTests
{
    [Fact]
    public void 同じ内容の別インスタンス_等価になる()
    {
        // Arrange
        var call = new Call(CallType.Pon, [new Tile(0), new Tile(1), new Tile(2)], new PlayerIndex(1), new Tile(0));
        var a = new CallListArray().AddCall(new PlayerIndex(0), call);
        var b = new CallListArray().AddCall(new PlayerIndex(0), call);

        // Act & Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void 異なるプレイヤーに追加した配列_非等価になる()
    {
        // Arrange
        var call = new Call(CallType.Pon, [new Tile(0), new Tile(1), new Tile(2)], new PlayerIndex(1), new Tile(0));
        var a = new CallListArray().AddCall(new PlayerIndex(0), call);
        var b = new CallListArray().AddCall(new PlayerIndex(1), call);

        // Act & Assert
        Assert.NotEqual(a, b);
    }
}
