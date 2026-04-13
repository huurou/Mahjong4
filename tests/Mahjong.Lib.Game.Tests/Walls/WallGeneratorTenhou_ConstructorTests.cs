using Mahjong.Lib.Game.Walls;

namespace Mahjong.Lib.Game.Tests.Walls;

public class WallGeneratorTenhou_ConstructorTests
{
    [Fact]
    public void シード長が不正_ArgumentExceptionが発生する()
    {
        // Arrange
        var shortSeed = Convert.ToBase64String(new byte[100]);

        // Act
        var ex = Record.Exception(() => new WallGeneratorTenhou(shortSeed));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void シード長が0_ArgumentExceptionが発生する()
    {
        // Arrange
        var emptySeed = Convert.ToBase64String(Array.Empty<byte>());

        // Act
        var ex = Record.Exception(() => new WallGeneratorTenhou(emptySeed));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }
}
