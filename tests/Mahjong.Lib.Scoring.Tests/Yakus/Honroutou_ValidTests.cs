using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Tests.Yakus;

public class Honroutou_ValidTests
{
    [Fact]
    public void Valid_么九牌のみ_副露なし_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(man: "999"), new(pin: "111"), new(pin: "999"), new(honor: "東東")]);
        var callList = new CallList();

        // Act
        var actual = Honroutou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_么九牌のみ_副露あり_成立する()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(honor: "東東東"), new(honor: "白白白")]);
        var callList = new CallList([
            Call.Pon(new (man: "999")),
            Call.Minkan(new (honor: "中中中中")),
        ]);

        // Act
        var actual = Honroutou.Valid(hand, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_中張牌がある_成立しない()
    {
        // Arrange
        var hand = new Hand([new(man: "111"), new(man: "999"), new(pin: "234"), new(pin: "999"), new(honor: "東東")]);
        var callList = new CallList();

        // Act
        var actual = Honroutou.Valid(hand, callList);

        // Assert
        Assert.False(actual);
    }
}
