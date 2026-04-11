using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class TileKind_ConstructorTests
{
    [Fact]
    public void 有効値0から33_正常に作成される()
    {
        for (var i = 0; i <= 33; i++)
        {
            // Arrange & Act
            var tileKind = new TileKind(i);

            // Assert
            Assert.Equal(i, tileKind.Value);
        }
    }

    [Fact]
    public void 引数がマイナス1_ArgumentOutOfRangeException発生()
    {
        // Arrange & Act
        var ex = Record.Exception(() => new TileKind(-1));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
        var argEx = (ArgumentOutOfRangeException)ex;
        Assert.Equal("value", argEx.ParamName);
        Assert.Equal(-1, argEx.ActualValue);
    }

    [Fact]
    public void 引数が34_ArgumentOutOfRangeException発生()
    {
        // Arrange & Act
        var ex = Record.Exception(() => new TileKind(34));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
        var argEx = (ArgumentOutOfRangeException)ex;
        Assert.Equal("value", argEx.ParamName);
        Assert.Equal(34, argEx.ActualValue);
    }
}
