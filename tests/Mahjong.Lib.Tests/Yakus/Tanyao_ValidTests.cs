using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Tanyao_ValidTests
{

    [Fact]
    public void Valid_正常形_副露なし_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "234"), new(man: "567"), new(pin: "88"), new(sou: "345"), new(sou: "678")]);
        var callList = new CallList();
        var gameRurles = new GameRules();

        // Act
        var actual = Tanyao.Valid(hand, callList, gameRurles);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_正常形_副露あり喰いタンあり_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "234"), new(man: "567"), new(pin: "88")]);
        var callList = new CallList([Call.Chi(new(sou: "345")), Call.Pon(new(sou: "888"))]);
        var gameRurles = new GameRules { KuitanEnabled = true };

        // Act
        var actual = Tanyao.Valid(hand, callList, gameRurles);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_正常形_副露あり喰いタンなし_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "234"), new(man: "567"), new(pin: "88")]);
        var callList = new CallList([Call.Chi(new(sou: "345")), Call.Pon(new(sou: "888"))]);
        var gameRurles = new GameRules { KuitanEnabled = false };

        // Act
        var actual = Tanyao.Valid(hand, callList, gameRurles);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_雀頭に么九牌あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "234"), new(man: "567"), new(pin: "99"), new(sou: "345"), new(sou: "678")]);
        var callList = new CallList();
        var gameRurles = new GameRules();

        // Act
        var actual = Tanyao.Valid(hand, callList, gameRurles);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_面子に么九牌あり_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "234"), new(man: "789"), new(pin: "88"), new(sou: "345"), new(sou: "678")]);
        var callList = new CallList();
        var gameRurles = new GameRules();

        // Act
        var actual = Tanyao.Valid(hand, callList, gameRurles);

        // Assert
        Assert.False(actual);
    }
}
