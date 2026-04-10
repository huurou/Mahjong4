using Mahjong.Lib.Games;
using Mahjong.Lib.Yakus;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Rinshan_ValidTests
{

    [Fact]
    public void Valid_嶺上開花である場合_成立する()
    {
        // Arrange
        var winSituation = new WinSituation { IsRinshan = true };

        // Act
        var actual = Rinshan.Valid(winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_嶺上開花でない場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation { IsRinshan = false };

        // Act
        var actual = Rinshan.Valid(winSituation);

        // Assert
        Assert.False(actual);
    }
}
