using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class SuuankouTankiDouble_ValidTests
{

    [Fact]
    public void Valid_ダブル役満有効_四暗刻単騎待ちの条件を満たす_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(pin: "444"), new(sou: "55")]);
        var winGroup = new TileKindList(sou: "55");
        var winTile = winGroup[0]; // 雀頭の牌を取得
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = true };
        var gameRules = new GameRules { DoubleYakumanEnabled = true };

        // Act
        var actual = SuuankouTankiDouble.Valid(hand, winGroup, winTile, callList, winSituation, gameRules);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_ダブル役満無効_四暗刻単騎待ちの条件を満たす_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(pin: "222"), new(sou: "333"), new(pin: "444"), new(sou: "55")]);
        var winGroup = new TileKindList(sou: "55");
        var winTile = winGroup[0]; // 雀頭の牌を取得
        var callList = new CallList();
        var winSituation = new WinSituation { IsTsumo = true };
        var gameRules = new GameRules { DoubleYakumanEnabled = false };

        // Act
        var actual = SuuankouTankiDouble.Valid(hand, winGroup, winTile, callList, winSituation, gameRules);

        // Assert
        Assert.False(actual);
    }
}
