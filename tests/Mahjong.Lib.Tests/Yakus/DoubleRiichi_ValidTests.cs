using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class DoubleRiichi_ValidTests
{
    [Fact]
    public void Valid_ダブル立直のとき_成立する()
    {
        // Arrange
        var winSituation = new WinSituation { IsDoubleRiichi = true };
        var callList = new CallList();

        // Act
        var actual = DoubleRiichi.Valid(winSituation, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_ダブル立直でない場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation { IsDoubleRiichi = false };
        var callList = new CallList();

        // Act
        var actual = DoubleRiichi.Valid(winSituation, callList);

        // Assert
        Assert.False(actual);
    }
}
