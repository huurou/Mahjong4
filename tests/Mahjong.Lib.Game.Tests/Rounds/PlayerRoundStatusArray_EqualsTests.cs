using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class PlayerRoundStatusArray_EqualsTests
{
    [Fact]
    public void 既定の別インスタンス_等価になる()
    {
        // Arrange
        var a = new PlayerRoundStatusArray();
        var b = new PlayerRoundStatusArray();

        // Act & Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void 同じ操作後の別インスタンス_等価になる()
    {
        // Arrange
        var status = new PlayerRoundStatus(IsMenzen: false);
        var a = new PlayerRoundStatusArray().SetStatus(new PlayerIndex(0), status);
        var b = new PlayerRoundStatusArray().SetStatus(new PlayerIndex(0), status);

        // Act & Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void 一人だけ状態が異なる_非等価になる()
    {
        // Arrange
        var a = new PlayerRoundStatusArray();
        var b = new PlayerRoundStatusArray().SetStatus(new PlayerIndex(0), new PlayerRoundStatus(IsMenzen: false));

        // Act & Assert
        Assert.NotEqual(a, b);
    }
}
