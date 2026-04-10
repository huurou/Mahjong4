using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Toitoihou_ValidTests
{

    [Fact]
    public void Valid_刻子4つと雀頭_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(honor: "ttt"), new(honor: "hh")]);
        var callList = new CallList();

        // Act
        var actual = Toitoihou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_槓子を含む刻子4つと雀頭_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "11"), new(pin: "333"), new(sou: "333")]);
        var callList = new CallList([Call.Minkan(new(honor: "tttt")), Call.Pon(new(honor: "ccc"))]);

        // Act
        var actual = Toitoihou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_順子があると_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(pin: "222"), new(sou: "333"), new(honor: "ttt"), new(honor: "hh")]);
        var callList = new CallList();

        // Act
        var actual = Toitoihou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_刻子が3つしかない_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(man: "45"), new(honor: "hh")]);
        var callList = new CallList();

        // Act
        var actual = Toitoihou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
