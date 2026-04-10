using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Daisharin_ValidTests
{
    [Fact]
    public void Valid_大車輪形の手牌でルール有効_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "22"), new(pin: "33"), new(pin: "44"), new(pin: "55"), new(pin: "66"), new(pin: "77"), new(pin: "88")]);
        var gameRules = new GameRules { DaisharinEnabled = true };

        // Act
        var actual = Daisharin.Valid(hand, gameRules);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_大車輪形の手牌でルール無効_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "22"), new(pin: "33"), new(pin: "44"), new(pin: "55"), new(pin: "66"), new(pin: "77"), new(pin: "88")]);
        var gameRules = new GameRules { DaisharinEnabled = false };

        // Act
        var actual = Daisharin.Valid(hand, gameRules);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_大車輪形ではない手牌_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "22"), new(pin: "33"), new(pin: "44"), new(pin: "55"), new(pin: "66"), new(pin: "77"), new(man: "88")]);
        var gameRules = new GameRules { DaisharinEnabled = true };

        // Act
        var actual = Daisharin.Valid(hand, gameRules);

        // Assert
        Assert.False(actual);
    }
}
