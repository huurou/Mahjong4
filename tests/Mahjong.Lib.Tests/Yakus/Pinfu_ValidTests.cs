using Mahjong.Lib.Fus;
using Mahjong.Lib.Games;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Pinfu_ValidTests
{
    [Fact]
    public void Valid_ツモアガリでピンヅモあり_成立()
    {
        // Arrange
        var fuList = new FuList([Fu.Futei]);
        var winSituation = new WinSituation { IsTsumo = true };
        var gameRules = new GameRules { PinzumoEnabled = true };

        // Act
        var result = Pinfu.Valid(fuList, winSituation, gameRules);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Valid_ツモアガリでピンヅモなし_不成立()
    {
        // Arrange
        // ピンヅモなしのときはツモ符がついて30符になる
        var fuList = new FuList([Fu.Futei, Fu.Tsumo]);
        var winSituation = new WinSituation { IsTsumo = true };
        var gameRules = new GameRules { PinzumoEnabled = false };

        // Act
        var result = Pinfu.Valid(fuList, winSituation, gameRules);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Valid_ツモアガリでツモ符がある_不成立()
    {
        // Arrange
        var fuList = new FuList([Fu.Futei, Fu.Tsumo]);
        var winSituation = new WinSituation { IsTsumo = true };
        var gameRules = new GameRules { PinzumoEnabled = true };

        // Act
        var result = Pinfu.Valid(fuList, winSituation, gameRules);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Valid_ロンアガリで副底と面前符だけ_成立()
    {
        // Arrange
        var fuList = new FuList([Fu.Futei, Fu.Menzen]);
        var winSituation = new WinSituation { IsTsumo = false };
        var gameRules = new GameRules();

        // Act
        var result = Pinfu.Valid(fuList, winSituation, gameRules);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Valid_ロンアガリで他の符がある_不成立()
    {
        // Arrange
        var fuList = new FuList([Fu.Futei, Fu.Menzen, Fu.Kanchan]);
        var winSituation = new WinSituation { IsTsumo = false };
        var gameRules = new GameRules();

        // Act
        var result = Pinfu.Valid(fuList, winSituation, gameRules);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Valid_ロンアガリで副底なし_不成立()
    {
        // Arrange
        var fuList = new FuList([Fu.Menzen]);
        var winSituation = new WinSituation { IsTsumo = false };
        var gameRules = new GameRules();

        // Act
        var result = Pinfu.Valid(fuList, winSituation, gameRules);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Valid_ロンアガリで面前符なし_不成立()
    {
        // Arrange
        var fuList = new FuList([Fu.Futei]);
        var winSituation = new WinSituation { IsTsumo = false };
        var gameRules = new GameRules();

        // Act
        var result = Pinfu.Valid(fuList, winSituation, gameRules);

        // Assert
        Assert.False(result);
    }
}
