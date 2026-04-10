using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Sankantsu_ValidTests
{

    [Fact]
    public void Valid_槓子が3つある場合_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(pin: "234"),]);
        var callList = new CallList([Call.Minkan(new(man: "5555")), Call.Ankan(new(pin: "2222")), Call.Minkan(new(sou: "8888"))]);

        // Act
        var actual = Sankantsu.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_槓子が手牌と合わせて3つある場合_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "1111"), new(man: "456")]);
        var callList = new CallList([Call.Minkan(new(pin: "2222")), Call.Minkan(new(sou: "8888"))]);

        // Act
        var actual = Sankantsu.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_槓子が2つしかない場合_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "123"), new(pin: "456")]);
        var callList = new CallList([Call.Minkan(new(pin: "2222")), Call.Minkan(new(sou: "8888"))]);

        // Act
        var actual = Sankantsu.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_槓子が0個の場合_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(pin: "456"), new(sou: "789"), new(honor: "tt"), new(man: "111")]);
        var callList = new CallList();

        // Act
        var actual = Sankantsu.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
