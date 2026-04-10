using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Iipeikou_ValidTests
{

    [Fact]
    public void Valid_同一順子が2つあり面前の場合_成立する()
    {
        // Arrange
        var hand = new Hand([
            new(man: "123"), // 同じ順子1つ目
            new(man: "123"), // 同じ順子2つ目
            new(pin: "456"),
            new(sou: "789"),
            new(honor: "tt"),
        ]);
        var callList = new CallList();

        // Act
        var actual = Iipeikou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_順子が2つあっても異なる順子の場合_成立しない()
    {
        // Arrange
        var hand = new Hand([
            new(man: "123"),
            new(man: "456"),
            new(pin: "789"),
            new(sou: "123"),
            new(honor: "tt"),
        ]);
        var callList = new CallList();

        // Act
        var actual = Iipeikou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_同一順子が2つあっても鳴きがある場合_成立しない()
    {
        // Arrange
        var hand = new Hand([
            new(man: "123"),
            new(man: "123"),
            new(pin: "456"),
            new(honor: "tt"),
        ]);
        // 鳴きあり
        var callList = new CallList([Call.Pon(new(sou: "999"))]);

        // Act
        var actual = Iipeikou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_順子が含まれていない場合_成立しない()
    {
        // Arrange
        var hand = new Hand([
            new(man: "111"),
            new(man: "222"),
            new(pin: "333"),
            new(sou: "444"),
            new(honor: "tt"),
        ]);
        var callList = new CallList();

        // Act
        var actual = Iipeikou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
