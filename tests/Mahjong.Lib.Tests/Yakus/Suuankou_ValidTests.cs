using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Suuankou_ValidTests
{

    [Fact]
    public void Valid_ツモアガリで4つの暗刻がある_成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(pin: "444"), new(sou: "55")]);
        var winGroup = new TileKindList(pin: "444");
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = Suuankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_ロンアガリで4つの暗刻がありアガリ牌が暗刻に含まれない_成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(pin: "444"), new(sou: "55")]);
        var winGroup = new TileKindList(sou: "55");
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = false };

        // Act
        var actual = Suuankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_ロンアガリで4つの暗刻がありアガリ牌が暗刻に含まれる_不成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(pin: "444"), new(sou: "55")]);
        var winGroup = new TileKindList(pin: "444"); // アガリ牌を含む暗刻
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = false };

        // Act
        var actual = Suuankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_3つの暗刻と1つの暗槓がある_成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(sou: "55")]);
        var ankan = Call.Ankan(new(pin: "4444"));
        var callList = new CallList([ankan]);
        var winGroup = new TileKindList(sou: "55");
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = Suuankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_2つの暗刻と2つの暗槓がある_成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "55")]);
        var callList = new CallList([Call.Ankan(new(sou: "3333")), Call.Ankan(new(pin: "4444"))]);
        var winGroup = new TileKindList(sou: "55");
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = Suuankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_3つの暗刻しかない_不成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(man: "456"), new(sou: "55")]);
        var callList = new CallList();
        var winGroup = new TileKindList(sou: "55");
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = Suuankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_暗刻4個ない_不成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "55")]);
        var callList = new CallList([Call.Pon(new(sou: "333"))]);
        var winGroup = new TileKindList(sou: "55");
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = Suuankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.False(actual);
    }
}
