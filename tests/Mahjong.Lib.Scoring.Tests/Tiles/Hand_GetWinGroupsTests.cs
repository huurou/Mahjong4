using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class Hand_GetWinGroupsTests
{
    [Fact]
    public void 和了牌を含む面子がある場合_該当する面子を返す()
    {
        // Arrange
        var hand = new Hand([
            new TileKindList(man: "123"),
            new TileKindList(man: "234"),
            new TileKindList(pin: "111"),
        ]);

        // Act
        var result = hand.GetWinGroups(TileKind.Man2);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(new TileKindList(man: "123"), result);
        Assert.Contains(new TileKindList(man: "234"), result);
    }

    [Fact]
    public void 和了牌を含む面子がない場合_空のリストを返す()
    {
        // Arrange
        var hand = new Hand([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        ]);

        // Act
        var result = hand.GetWinGroups(TileKind.Sou1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void 重複する面子がある場合_重複を除去して返す()
    {
        // Arrange
        var hand = new Hand([
            new TileKindList(man: "123"),
            new TileKindList(man: "123"),
            new TileKindList(pin: "111"),
        ]);

        // Act
        var result = hand.GetWinGroups(TileKind.Man1);

        // Assert
        Assert.Single(result);
        Assert.Contains(new TileKindList(man: "123"), result);
    }
}
