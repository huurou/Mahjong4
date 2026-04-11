using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class JunseiChuurenpoutouDouble_ValidTests
{
    [Fact]
    public void Valid_ダブル役満有効_純正九蓮宝燈の条件を満たす_成立する()
    {
        // Arrange
        // 純正九蓮宝燈の形：1112345678999 + 1（和了牌）
        var hand = new Hand([new(man: "111"), new(man: "123"), new(man: "456"), new(man: "789"), new(man: "99")]);
        var winTile = TileKind.Man1;
        var gameRules = new GameRules { DoubleYakumanEnabled = true };

        // Act
        var actual = JunseiChuurenpoutouDouble.Valid(hand, winTile, gameRules);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_ダブル役満無効_純正九蓮宝燈の条件を満たす_成立しない()
    {
        // Arrange
        // 純正九蓮宝燈の形：1112345678999 + 1（和了牌）
        var hand = new Hand([new(man: "111"), new(man: "123"), new(man: "456"), new(man: "789"), new(man: "99")]);
        var winTile = TileKind.Man1;
        var gameRules = new GameRules { DoubleYakumanEnabled = false };

        // Act
        var actual = JunseiChuurenpoutouDouble.Valid(hand, winTile, gameRules);

        // Assert
        Assert.False(actual);
    }
}
