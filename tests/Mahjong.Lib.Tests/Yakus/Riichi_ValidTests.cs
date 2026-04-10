using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Yakus;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Riichi_ValidTests
{

    [Fact]
    public void Valid_立直かつダブル立直でなく面前の場合_成立する()
    {
        // Arrange
        var winSituation = new WinSituation
        {
            IsRiichi = true,
            IsDoubleRiichi = false
        };
        var callList = new CallList(); // 副露なし（面前）

        // Act
        var actual = Riichi.Valid(winSituation, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_立直でない場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation
        {
            IsRiichi = false,
            IsDoubleRiichi = false
        };
        var callList = new CallList();

        // Act
        var actual = Riichi.Valid(winSituation, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_ダブル立直の場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation
        {
            IsRiichi = true,
            IsDoubleRiichi = true
        };
        var callList = new CallList();

        // Act
        var actual = Riichi.Valid(winSituation, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_副露がある場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation
        {
            IsRiichi = true,
            IsDoubleRiichi = false
        };
        var callList = new CallList([Call.Pon(new(man: "111"))]);

        // Act
        var actual = Riichi.Valid(winSituation, callList);

        // Assert
        Assert.False(actual);
    }
}
