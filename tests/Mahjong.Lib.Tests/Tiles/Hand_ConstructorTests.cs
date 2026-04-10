using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Tiles;

public class Hand_ConstructorTests
{
    [Fact]
    public void デフォルトコンストラクタ_空の手牌が作成される()
    {
        // Arrange & Act
        var hand = new Hand();

        // Assert
        Assert.Equal(0, hand.Count);
        Assert.Empty(hand);
    }

    [Fact]
    public void IEnumerableコンストラクタ_TileKindList配列指定_正常に作成される()
    {
        // Arrange
        var tileKindLists = new[]
        {
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        };

        // Act
        var hand = new Hand(tileKindLists);

        // Assert
        Assert.Equal(2, hand.Count);
        Assert.Equal(new TileKindList(man: "123"), hand[0]);
        Assert.Equal(new TileKindList(pin: "456"), hand[1]);
    }

    [Fact]
    public void HandBuilder_Create_TileKindList配列から正常に作成される()
    {
        // Arrange
        var tileKindLists = new[]
        {
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        };

        // Act
        var hand = Hand.HandBuilder.Create(tileKindLists);

        // Assert
        Assert.Equal(2, hand.Count);
        Assert.Equal(new TileKindList(man: "123"), hand[0]);
        Assert.Equal(new TileKindList(pin: "456"), hand[1]);
    }

    [Fact]
    public void HandBuilder_Create_空の配列_空の手牌が作成される()
    {
        // Arrange
        var tileKindLists = Array.Empty<TileKindList>();

        // Act
        var hand = Hand.HandBuilder.Create(tileKindLists);

        // Assert
        Assert.Equal(0, hand.Count);
        Assert.Empty(hand);
    }

    [Fact]
    public void コレクションビルダー_配列初期化構文_正常に作成される()
    {
        // Arrange & Act
        Hand hand = [
            new TileKindList(man: "123"),
            new TileKindList(pin: "456"),
        ];

        // Assert
        Assert.Equal(2, hand.Count);
        Assert.Equal(new TileKindList(man: "123"), hand[0]);
        Assert.Equal(new TileKindList(pin: "456"), hand[1]);
    }
}
