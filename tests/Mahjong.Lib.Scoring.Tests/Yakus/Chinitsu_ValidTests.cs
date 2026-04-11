using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Chinitsu_ValidTests
{
    [Fact]
    public void Valid_同一スートの牌のみ_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "123456789")]);
        var callList = new CallList();

        // Act
        var actual = Chinitsu.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_同一スートの牌と副露_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "12345")]);
        var callList = new CallList(
        [
            Call.Chi(new TileKindList(man: "678")),
            Call.Pon(new TileKindList(man: "999"))
        ]);

        // Act
        var actual = Chinitsu.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_複数のスートが混在_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "123"), new(pin: "456")]);
        var callList = new CallList();

        // Act
        var actual = Chinitsu.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_字牌が含まれる_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "11123456789"), new(honor: "ttt")]);
        var callList = new CallList();

        // Act
        var actual = Chinitsu.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
