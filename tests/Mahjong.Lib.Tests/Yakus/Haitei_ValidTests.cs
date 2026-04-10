using Mahjong.Lib.Games;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Haitei_ValidTests
{
    [Fact]
    public void Valid_海底撈月である場合_成立する()
    {
        // Arrange
        var winSituation = new WinSituation { IsHaitei = true };

        // Act
        var actual = Haitei.Valid(winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_海底撈月でない場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation { IsHaitei = false };

        // Act
        var actual = Haitei.Valid(winSituation);

        // Assert
        Assert.False(actual);
    }
}
