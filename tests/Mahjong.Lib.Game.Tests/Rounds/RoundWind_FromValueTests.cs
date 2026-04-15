using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class RoundWind_FromValueTests
{
    [Fact]
    public void 値0_東のシングルトンを返す()
    {
        // Act
        var result = RoundWind.FromValue(0);

        // Assert
        Assert.Same(RoundWind.East, result);
    }

    [Fact]
    public void 値1_南のシングルトンを返す()
    {
        // Act
        var result = RoundWind.FromValue(1);

        // Assert
        Assert.Same(RoundWind.South, result);
    }

    [Fact]
    public void 値2_西のシングルトンを返す()
    {
        // Act
        var result = RoundWind.FromValue(2);

        // Assert
        Assert.Same(RoundWind.West, result);
    }

    [Fact]
    public void 値3_北のシングルトンを返す()
    {
        // Act
        var result = RoundWind.FromValue(3);

        // Assert
        Assert.Same(RoundWind.North, result);
    }

    [Fact]
    public void 範囲外の値_例外を投げる()
    {
        // Act
        var ex = Record.Exception(() => RoundWind.FromValue(4));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
    }

    [Fact]
    public void 負の値_例外を投げる()
    {
        // Act
        var ex = Record.Exception(() => RoundWind.FromValue(-1));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
    }
}
