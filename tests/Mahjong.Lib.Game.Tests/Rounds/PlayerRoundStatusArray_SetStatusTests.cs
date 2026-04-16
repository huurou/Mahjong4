using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class PlayerRoundStatusArray_SetStatusTests
{
    [Fact]
    public void 指定プレイヤーのみ置換される()
    {
        // Arrange
        var array = new PlayerRoundStatusArray();
        var newStatus = new PlayerRoundStatus(IsMenzen: false, IsRiichi: true);

        // Act
        var updated = array.SetStatus(new PlayerIndex(1), newStatus);

        // Assert
        Assert.Equal(newStatus, updated[new PlayerIndex(1)]);
        Assert.Equal(new PlayerRoundStatus(), updated[new PlayerIndex(0)]);
        Assert.Equal(new PlayerRoundStatus(), updated[new PlayerIndex(2)]);
        Assert.Equal(new PlayerRoundStatus(), updated[new PlayerIndex(3)]);
    }

    [Fact]
    public void 元のインスタンスは不変()
    {
        // Arrange
        var array = new PlayerRoundStatusArray();

        // Act
        array.SetStatus(new PlayerIndex(0), new PlayerRoundStatus(IsMenzen: false));

        // Assert
        Assert.Equal(new PlayerRoundStatus(), array[new PlayerIndex(0)]);
    }
}
