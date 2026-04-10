using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Sanshokudoukou_ValidTests
{

    [Fact]
    public void Valid_手牌内で三色同刻_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "333"), new(pin: "333"), new(sou: "333"), new(pin: "789"), new(man: "55")]);
        var callList = new CallList();

        // Act
        var actual = Sanshokudoukou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_手牌と副露で三色同刻_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "333"), new(pin: "333"), new(man: "55")]);
        var callList = new CallList([Call.Pon(new(sou: "333"))]);

        // Act
        var actual = Sanshokudoukou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_槓子を含む三色同刻_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "333"), new(man: "55")]);
        var callList = new CallList([Call.Pon(new(pin: "333")), Call.Minkan(new(sou: "3333"))]);

        // Act
        var actual = Sanshokudoukou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_刻子が3つあるが同じ数字ではない_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "333"), new(pin: "555"), new(sou: "777"), new(man: "789"), new(man: "22")]);
        var callList = new CallList();

        // Act
        var actual = Sanshokudoukou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_二色のみ_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "333"), new(pin: "333"), new(sou: "789"), new(pin: "789"), new(man: "55")]);
        var callList = new CallList();

        // Act
        var actual = Sanshokudoukou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_三色の同数字だが順子_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(pin: "123"), new(sou: "123"), new(pin: "789"), new(man: "55")]);
        var callList = new CallList();

        // Act
        var actual = Sanshokudoukou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_刻子が3つない_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "333"), new(pin: "333"), new(sou: "123"), new(pin: "789"), new(man: "55")]);
        var callList = new CallList();

        // Act
        var actual = Sanshokudoukou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_刻子3つ_萬子がない_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "222"), new(pin: "333"), new(sou: "333"), new(pin: "789"), new(man: "55")]);
        var callList = new CallList();

        // Act
        var actual = Sanshokudoukou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
