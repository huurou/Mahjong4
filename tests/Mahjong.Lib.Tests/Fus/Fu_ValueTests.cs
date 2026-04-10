using Mahjong.Lib.Fus;

namespace Mahjong.Lib.Tests.Fus;

public class Fu_ValueTests
{
    [Fact]
    public void 不正なFuType_ArgumentOutOfRangeExceptionが発生()
    {
        // Arrange
        var invalidFuType = (FuType)999;
        var fu = new Fu(invalidFuType, 0);

        // Act
        var ex = Record.Exception(() => fu.Value);

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
        Assert.Equal("Type", ((ArgumentOutOfRangeException)ex).ParamName);
    }
}
