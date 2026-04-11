using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Chiitoitsu_ValidTests
{
    [Fact]
    public void Valid_7つの対子がある場合_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "11"), new(man: "22"), new(pin: "33"), new(pin: "44"), new(sou: "55"), new(sou: "66"), new(honor: "tt")]);

        // Act
        var actual = Chiitoitsu.Valid(hand);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_対子が7つではない場合_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "11"), new(man: "22"), new(pin: "33"), new(pin: "44"), new(sou: "55"), new(sou: "66")]);

        // Act
        var actual = Chiitoitsu.Valid(hand);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_対子ではない組み合わせが含まれる場合_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "11"), new(man: "22"), new(pin: "33"), new(pin: "44"), new(sou: "55"), new(sou: "66"), new(man: "123")]);

        // Act
        var actual = Chiitoitsu.Valid(hand);

        // Assert
        Assert.False(actual);
    }
}
