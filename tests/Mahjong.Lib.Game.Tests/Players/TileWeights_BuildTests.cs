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
    public void Build_染め条件成立_染め色数牌が4倍_字牌は既定値()
    {
        // Arrange: 萬子 7 枚 + 字牌 3 枚 = 合計 10 枚で染め条件成立 (風 1 + 三元 2、どちらも閾値未達)
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
        // 萬子 (0-8) は ×4 (書籍準拠)
        for (var i = 0; i < 9; i++)
        {
            Assert.Equal(4, weights.Of(TileKind.All[i]));
        }
        // 筒子 (9-17) / 索子 (18-26) は ×1
        for (var i = 9; i < 27; i++)
        {
            Assert.Equal(1, weights.Of(TileKind.All[i]));
        }
        // 字牌 (27-33) は ×1 (書籍準拠で染めによる字牌乗算はしない)
        for (var i = 27; i < 34; i++)
        {
            Assert.Equal(1, weights.Of(TileKind.All[i]));
        }
    }

    [Fact]
    public void Build_三元牌3枚以上_三元牌が8倍()
    {
        // Arrange: 白×3 で 3 枚 (書籍準拠の新閾値 ≥ 3)
        var hand = new Hand(
        [
            new Tile(124), new Tile(125), new Tile(126),  // 白×3
            new Tile(0), new Tile(4), new Tile(8),       // 萬子 3 枚
            new Tile(36), new Tile(40),                    // 筒子 2 枚
            new Tile(72), new Tile(76),                    // 索子 2 枚
        ]);
        var calls = new CallList();

        // Act
        var weights = TileWeights.Build(hand, calls);

        // Assert
        // 三元牌 (白發中、index 31-33) は ×8 (書籍準拠)
        Assert.Equal(8, weights.Of(TileKind.Haku));
        Assert.Equal(8, weights.Of(TileKind.Hatsu));
        Assert.Equal(8, weights.Of(TileKind.Chun));
    }

    [Fact]
    public void Build_風牌2枚以上_風牌が4倍_染め不成立()
    {
        // Arrange: 東×2 + 他の散らし = 風牌 2 枚 (書籍準拠の新閾値 ≥ 2)、染め条件は不成立
        var hand = new Hand(
        [
            new Tile(108), new Tile(109),                  // 東×2
            new Tile(0), new Tile(4), new Tile(8),       // 萬子 3 枚
            new Tile(36), new Tile(40), new Tile(44),    // 筒子 3 枚
            new Tile(72), new Tile(76),                    // 索子 2 枚
        ]);
        var calls = new CallList();

        // Act
        var weights = TileWeights.Build(hand, calls);

        // Assert
        // 風牌 ×4 (書籍準拠)
        Assert.Equal(4, weights.Of(TileKind.Ton));
        Assert.Equal(4, weights.Of(TileKind.Nan));
        Assert.Equal(4, weights.Of(TileKind.Sha));
        Assert.Equal(4, weights.Of(TileKind.Pei));
        // 三元牌は ×1 (3 枚未満)
        Assert.Equal(1, weights.Of(TileKind.Haku));
        // 数牌は ×1 (染め不成立)
        Assert.Equal(1, weights.Of(TileKind.Man1));
    }

    [Fact]
    public void Build_風牌9枚_染め条件と複数成立で乗算される()
    {
        // Arrange: 東×3 + 南×3 + 西×3 + 萬子×2 = 11 枚
        //   風牌 9 枚 (≥2) → 風牌 ×4、萬子 2 + 字牌 9 = 11 枚 で染め (萬子) ×4
        //   風牌は染めの字牌扱いではないので染め ×4 とは乗算されない (書籍準拠: 染めは字牌を乗算しない)
        //   よって風牌は ×4 のまま
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
        // 風牌 ×4 (書籍準拠で染めは字牌に影響しないので乗算されない)
        Assert.Equal(4, weights.Of(TileKind.Ton));
        Assert.Equal(4, weights.Of(TileKind.Nan));
        Assert.Equal(4, weights.Of(TileKind.Sha));
        Assert.Equal(4, weights.Of(TileKind.Pei));
        // 萬子は染め色 ×4
        Assert.Equal(4, weights.Of(TileKind.Man1));
        // 三元牌は ×1 (0 枚)
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

        // Assert: 萬子 7 + 字牌 3 = 合計 10 で染め成立 → 萬子 ×4。中 3 枚で三元条件成立 → 三元 ×8
        Assert.Equal(4, weights.Of(TileKind.Man1));
        Assert.Equal(8, weights.Of(TileKind.Chun));
        Assert.Equal(8, weights.Of(TileKind.Haku));
    }
}
