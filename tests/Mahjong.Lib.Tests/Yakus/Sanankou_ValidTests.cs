using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Sanankou_ValidTests
{

    [Fact]
    public void Valid_暗刻2つシャンポンツモアガリ_成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(man: "123"), new(pin: "222"), new(sou: "333"), new(sou: "55")]);
        var winGroup = new TileKindList(sou: "333");
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = Sanankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_暗刻3つロンアガリ_成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(man: "456"), new(sou: "55")]);
        var winGroup = new TileKindList(man: "456");
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = false };

        // Act
        var actual = Sanankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_暗刻2つシャンポンロンアガリ_不成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(man: "123"), new(pin: "222"), new(sou: "333"), new(sou: "55")]);
        var winGroup = new TileKindList(sou: "333"); // アガリ牌を含む暗刻
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = false };

        // Act
        var actual = Sanankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_暗刻2つ暗槓1つ_成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "55")]);
        var callList = new CallList([Call.Ankan(new(sou: "3333"))]);
        var winGroup = new TileKindList(man: "111");
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = Sanankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_暗槓3つ_成立()
    {
        // Arrange
        var hand = new Hand([new(man: "234"), new(sou: "55")]);
        var callList = new CallList([(Call.Ankan(new(man: "1111"))), (Call.Ankan(new(pin: "2222"))), (Call.Ankan(new(sou: "3333")))]);
        var winGroup = new TileKindList(sou: "55");
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = Sanankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_暗刻2つ_不成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "123"), new(pin: "222"), new(man: "456"), new(sou: "55")]);
        var callList = new CallList();
        var winGroup = new TileKindList(man: "456");
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = Sanankou.Valid(hand, winGroup, callList, winSituation);

        // Assert
        Assert.False(actual);
    }
}
