using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class JunseiChuurenpoutou_ValidTests
{
    [Fact]
    public void Valid_純正九蓮形で1で和了_成立する()
    {
        // Arrange
        // 純正九蓮宝燈の形：1112345678999 + 1（和了牌）
        var hand = new Hand([new(man: "111"), new(man: "123"), new(man: "456"), new(man: "789"), new(man: "99")]);
        var winTile = TileKind.Man1;

        // Act
        var actual = JunseiChuurenpoutou.Valid(hand, winTile);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_純正九蓮形で9で和了_成立する()
    {
        // Arrange
        // 純正九蓮宝燈の形：1112345678999 + 9（和了牌）
        var hand = new Hand([new(pin: "11"), new(pin: "123"), new(pin: "456"), new(pin: "789"), new(pin: "999")]);
        var winTile = TileKind.Pin9;

        // Act
        var actual = JunseiChuurenpoutou.Valid(hand, winTile);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_純正九蓮形で5で和了_成立する()
    {
        // Arrange
        // 純正九蓮宝燈の形：1112345678999 + 5（和了牌）
        var hand = new Hand([new(sou: "111"), new(sou: "234"), new(sou: "55"), new(sou: "678"), new(sou: "999")]);
        var winTile = TileKind.Sou5;

        // Act
        var actual = JunseiChuurenpoutou.Valid(hand, winTile);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_純正でない_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(man: "234"), new(man: "567"), new(man: "789"), new(man: "99")]);
        var winTile = TileKind.Man2;

        // Act
        var actual = JunseiChuurenpoutou.Valid(hand, winTile);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_複数のスートが混在_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(man: "123"), new(man: "345"), new(man: "789"), new(pin: "99")]);
        var winTile = TileKind.Man1;

        // Act
        var actual = JunseiChuurenpoutou.Valid(hand, winTile);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_字牌が含まれる_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(man: "123"), new(man: "456"), new(man: "789"), new(honor: "tt")]);
        var winTile = TileKind.Man1;

        // Act
        var actual = JunseiChuurenpoutou.Valid(hand, winTile);

        // Assert
        Assert.False(actual);
    }
}
