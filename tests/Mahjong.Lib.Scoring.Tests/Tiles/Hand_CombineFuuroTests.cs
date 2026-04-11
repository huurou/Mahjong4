using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Tiles;

public class Hand_CombineFuuroTests
{
    [Fact]
    public void 空の副露リスト_手牌のみが返される()
    {
        // Arrange
        var hand = new Hand([
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        ]);
        var callList = new CallList();

        // Act
        var result = hand.CombineFuuro(callList);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(new TileKindList(man: "123"), result[0]);
        Assert.Equal(new TileKindList(pin: "456"), result[1]);
    }

    [Fact]
    public void 副露ありの場合_手牌と副露が結合される()
    {
        // Arrange
        var hand = new Hand([new TileKindList(man: "123")]);
        var call = Call.Chi(new TileKindList(pin: "456"));
        var callList = new CallList([call]);

        // Act
        var result = hand.CombineFuuro(callList);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(new TileKindList(man: "123"), result[0]);
        Assert.Equal(new TileKindList(pin: "456"), result[1]);
    }
}
