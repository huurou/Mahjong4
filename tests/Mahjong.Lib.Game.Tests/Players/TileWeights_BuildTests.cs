using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Tiles;
using TileKind = Mahjong.Lib.Scoring.Tiles.TileKind;

namespace Mahjong.Lib.Game.Tests.Players;

public class TileWeights_BuildTests
{
    [Fact]
    public void Build_標準的な手牌_全て倍率1()
    {
        // Arrange: 萬子 3、筒子 3、索子 3、字牌 2 の 11 枚 (どの条件も満たさない)
        var hand = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),       // 1m 2m 3m
            new Tile(36), new Tile(40), new Tile(44),    // 1p 2p 3p
            new Tile(72), new Tile(76), new Tile(80),    // 1s 2s 3s
            new Tile(108), new Tile(128),                 // 東 白
        ]);
        var calls = new CallList();

        // Act
        var weights = TileWeights.Build(hand, calls);

        // Assert
        foreach (var kind in TileKind.All)
        {
            Assert.Equal(1, weights.Of(kind));
        }
    }

    [Fact]
    public void Build_染め条件成立_染め色数牌が2倍_字牌が4倍()
    {
        // Arrange: 萬子 7 枚 + 字牌 3 枚 = 合計 10 枚で染め条件成立
        var hand = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),       // 1m 2m 3m
            new Tile(12), new Tile(17), new Tile(20),    // 4m 5m 6m
            new Tile(24),                                  // 7m
            new Tile(108), new Tile(128), new Tile(132),  // 東 白 中
        ]);
        var calls = new CallList();

        // Act
        var weights = TileWeights.Build(hand, calls);

        // Assert
        // 萬子 (0-8) は ×2
        for (var i = 0; i < 9; i++)
        {
            Assert.Equal(2, weights.Of(TileKind.All[i]));
        }
        // 筒子 (9-17) / 索子 (18-26) は ×1
        for (var i = 9; i < 27; i++)
        {
            Assert.Equal(1, weights.Of(TileKind.All[i]));
        }
        // 字牌 (27-33) は ×4
        for (var i = 27; i < 34; i++)
        {
            Assert.Equal(4, weights.Of(TileKind.All[i]));
        }
    }

    [Fact]
    public void Build_三元牌6枚以上_三元牌が8倍()
    {
        // Arrange: 白×3 + 發×3 で 6 枚
        var hand = new Hand(
        [
            new Tile(124), new Tile(125), new Tile(126),  // 白×3
            new Tile(128), new Tile(129), new Tile(130),  // 發×3
            new Tile(0), new Tile(4), new Tile(8),       // 萬子 3 枚
            new Tile(36), new Tile(40),                    // 筒子 2 枚
        ]);
        var calls = new CallList();

        // Act
        var weights = TileWeights.Build(hand, calls);

        // Assert
        // 三元牌 (白發中、index 31-33) は ×8
        Assert.Equal(8, weights.Of(TileKind.Haku));
        Assert.Equal(8, weights.Of(TileKind.Hatsu));
        Assert.Equal(8, weights.Of(TileKind.Chun));
    }

    [Fact]
    public void Build_風牌9枚以上_風牌が8倍_ただし染め条件と乗算される()
    {
        // Arrange: 東×3 + 南×3 + 西×3 + 萬子×2 = 11 枚
        //   風牌 9 枚で風牌 ×8、さらに萬子 2 + 字牌 9 = 11 で染め条件成立 → 字牌 ×4
        //   よって風牌は ×8 × ×4 = ×32 (仕様通り、複数条件は乗算される)
        var hand = new Hand(
        [
            new Tile(108), new Tile(109), new Tile(110),  // 東×3
            new Tile(112), new Tile(113), new Tile(114),  // 南×3
            new Tile(116), new Tile(117), new Tile(118),  // 西×3
            new Tile(0), new Tile(4),                      // 萬子 2 枚
        ]);
        var calls = new CallList();

        // Act
        var weights = TileWeights.Build(hand, calls);

        // Assert
        // 風牌 ×8 × 字牌染め ×4 = ×32
        Assert.Equal(32, weights.Of(TileKind.Ton));
        Assert.Equal(32, weights.Of(TileKind.Nan));
        Assert.Equal(32, weights.Of(TileKind.Sha));
        Assert.Equal(32, weights.Of(TileKind.Pei));
    }

    [Fact]
    public void Build_風牌のみ9枚_染め条件を避けた場合は風牌のみ8倍()
    {
        // Arrange: 風牌 9 枚だが、萬子だけで染め条件 (字牌 9 + 萬子 3 でちょうど 12) を意図的に超えないよう
        //   萬子を 0 にして三色散らし。ただし手牌 13 枚は必須ではない (Build の入力は任意)
        //   → 手牌 9 枚 (全風牌) で染め条件は発動しない (染めは「特定色+字牌」合計 10 だが特定色=0 で 9)
        var hand = new Hand(
        [
            new Tile(108), new Tile(109), new Tile(110),  // 東×3
            new Tile(112), new Tile(113), new Tile(114),  // 南×3
            new Tile(116), new Tile(117), new Tile(118),  // 西×3
        ]);
        var calls = new CallList();

        // Act
        var weights = TileWeights.Build(hand, calls);

        // Assert
        // 風牌 ×8 のみ、染めは発動しない
        Assert.Equal(8, weights.Of(TileKind.Ton));
        Assert.Equal(8, weights.Of(TileKind.Nan));
        Assert.Equal(8, weights.Of(TileKind.Sha));
        Assert.Equal(8, weights.Of(TileKind.Pei));
        // 三元牌は ×1 (6 枚未満)
        Assert.Equal(1, weights.Of(TileKind.Haku));
    }

    [Fact]
    public void Build_副露も合算して判定する()
    {
        // Arrange: 手牌 7 枚 (萬子のみ) + 副露 3 枚 (字牌中刻子)
        var hand = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24),
        ]);
        var chunPon = new Call(
            CallType.Pon,
            [new Tile(132), new Tile(133), new Tile(134)],
            new PlayerIndex(1),
            new Tile(132));
        var calls = new CallList([chunPon]);

        // Act
        var weights = TileWeights.Build(hand, calls);

        // Assert: 萬子 7 + 字牌 3 = 合計 10 で染め条件成立
        Assert.Equal(2, weights.Of(TileKind.Man1));
        Assert.Equal(4, weights.Of(TileKind.Haku));
    }
}
