using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class SuuankouTanki_ValidTests
{
    [Fact]
    public void Valid_四暗刻が成立しアガリ牌が雀頭_成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(pin: "444"), new(sou: "55")]);
        var winGroup = new TileKindList(sou: "55");
        var winTile = winGroup[0]; // 雀頭の牌を取得
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = SuuankouTanki.Valid(hand, winGroup, winTile, callList, winSituation);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_四暗刻が成立しない_不成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(man: "456"), new(sou: "55")]);
        var winGroup = new TileKindList(sou: "55");
        var winTile = winGroup[0]; // 雀頭の牌を取得
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = SuuankouTanki.Valid(hand, winGroup, winTile, callList, winSituation);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_アガリ牌が雀頭でない_不成立()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(pin: "444"), new(sou: "55")]);
        var winGroup = new TileKindList(pin: "444");
        var winTile = winGroup[0]; // 暗刻の牌を取得
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = true };

        // Act
        var actual = SuuankouTanki.Valid(hand, winGroup, winTile, callList, winSituation);

        // Assert
        Assert.False(actual);
    }
}
