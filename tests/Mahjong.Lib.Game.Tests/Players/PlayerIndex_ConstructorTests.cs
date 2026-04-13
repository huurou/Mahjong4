using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Tests.Players;

public class PlayerIndex_ConstructorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void 有効値0から3_正常に作成される(int value)
    {
        // Act
        var playerIndex = new PlayerIndex(value);

        // Assert
        Assert.Equal(value, playerIndex.Value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void 範囲外の値_ArgumentOutOfRangeExceptionが発生する(int value)
    {
        // Act
        var ex = Record.Exception(() => new PlayerIndex(value));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
    }
}
