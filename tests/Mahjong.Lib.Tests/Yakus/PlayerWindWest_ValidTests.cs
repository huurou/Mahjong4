using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class PlayerWindWest_ValidTests
{

    [Fact]
    public void Valid_東家で東の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ttt")]);
        var callList = new CallList();
        var winSituation = new WinSituation { PlayerWind = Wind.East };

        // Act
        var actual = PlayerWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_東家で西の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "sss")]);
        var callList = new CallList();
        var winSituation = new WinSituation { PlayerWind = Wind.East };

        // Act
        var actual = PlayerWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_南家で南の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "nnn")]);
        var callList = new CallList();
        var winSituation = new WinSituation { PlayerWind = Wind.South };

        // Act
        var actual = PlayerWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_南家で西の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "sss")]);
        var callList = new CallList();
        var winSituation = new WinSituation { PlayerWind = Wind.South };

        // Act
        var actual = PlayerWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_西家で東の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ttt")]);
        var callList = new CallList();
        var winSituation = new WinSituation { PlayerWind = Wind.West };

        // Act
        var actual = PlayerWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_西家で西の刻子あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "sss")]);
        var callList = new CallList();
        var winSituation = new WinSituation { PlayerWind = Wind.West };

        // Act
        var actual = PlayerWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_西家で西の明槓あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234")]);
        var callList = new CallList([Call.Minkan(new TileKindList(honor: "ssss"))]);
        var winSituation = new WinSituation { PlayerWind = Wind.West };

        // Act
        var actual = PlayerWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_西家で西の暗槓あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234")]);
        var callList = new CallList([Call.Ankan(new TileKindList(honor: "ssss"))]);
        var winSituation = new WinSituation { PlayerWind = Wind.West };

        // Act
        var actual = PlayerWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_北家で西の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "sss")]);
        var callList = new CallList();
        var winSituation = new WinSituation { PlayerWind = Wind.North };

        // Act
        var actual = PlayerWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_北家で北の刻子あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(pin: "11"), new(man: "234"), new(sou: "234"), new(honor: "ppp")]);
        var callList = new CallList();
        var winSituation = new WinSituation { PlayerWind = Wind.North };

        // Act
        var actual = PlayerWindWest.Valid(hand, callList, winSituation);

        // Assert
        Assert.False(actual);
    }
}
