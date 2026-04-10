using Mahjong.Lib.Games;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class RenhouYakuman_ValidTests
{
    [Fact]
    public void Valid_人和かつ役満扱いの場合_成立する()
    {
        // Arrange
        var winSituation = new WinSituation { IsRenhou = true };
        var gameRules = new GameRules { RenhouAsYakumanEnabled = true };

        // Act
        var actual = RenhouYakuman.Valid(winSituation, gameRules);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_人和だが役満扱いでない場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation { IsRenhou = true };
        var gameRules = new GameRules { RenhouAsYakumanEnabled = false };

        // Act
        var actual = RenhouYakuman.Valid(winSituation, gameRules);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_人和でない場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation { IsRenhou = false };
        var gameRules = new GameRules { RenhouAsYakumanEnabled = true };

        // Act
        var actual = RenhouYakuman.Valid(winSituation, gameRules);

        // Assert
        Assert.False(actual);
    }
}
