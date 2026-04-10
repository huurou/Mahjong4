using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class RoundWindWest_ValidTests
{
    [Fact]
    public void Valid_東場で東の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ttt")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.East };

        // Act
        var actual = RoundWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_東場で西の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "sss")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.East };

        // Act
        var actual = RoundWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_南場で南の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "nnn")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.South };

        // Act
        var actual = RoundWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_南場で西の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "sss")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.South };

        // Act
        var actual = RoundWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_西場で東の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ttt")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.West };

        // Act
        var actual = RoundWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_西場で西の刻子あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "sss")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.West };

        // Act
        var actual = RoundWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_西場で副露の西の刻子あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234")]);
        var callList = new CallList([Call.Pon(new TileKindList(honor: "sss"))]);
        var winSituation = new WinSituation { RoundWind = Wind.West };

        // Act
        var actual = RoundWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_西場で西の明槓あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234")]);
        var callList = new CallList([Call.Minkan(new TileKindList(honor: "ssss"))]);
        var winSituation = new WinSituation { RoundWind = Wind.West };

        // Act
        var actual = RoundWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_西場で西の暗槓あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234")]);
        var callList = new CallList([Call.Ankan(new TileKindList(honor: "ssss"))]);
        var winSituation = new WinSituation { RoundWind = Wind.West };

        // Act
        var actual = RoundWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_西場で西の牌があるが刻子になっていない場合_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ss")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.West };

        // Act
        var actual = RoundWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_北場で西の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "sss")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.North };

        // Act
        var actual = RoundWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_北場で北の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ppp")]);
        var callList = new CallList();
        var winSituation = new WinSituation { RoundWind = Wind.North };

        // Act
        var actual = RoundWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }
}
