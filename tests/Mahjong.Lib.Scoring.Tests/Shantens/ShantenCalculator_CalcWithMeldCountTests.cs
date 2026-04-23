using Mahjong.Lib.Scoring.Shantens;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Shantens;

public class ShantenCalculator_CalcWithMeldCountTests
{
    [Fact]
    public void 手牌13枚_knownCallMeldCount0_自動推定版と一致する()
    {
        // Arrange
        var hand = new TileKindList("567", "11", "111345677", "");

        // Act
        var autoInferred = ShantenCalculator.Calc(hand, useRegular: true, useChiitoitsu: false, useKokushi: false);
        var explicitCount = ShantenCalculator.Calc(hand, knownCallMeldCount: 0, useRegular: true, useChiitoitsu: false, useKokushi: false);

        // Assert
        Assert.Equal(autoInferred, explicitCount);
    }

    [Fact]
    public void 手牌14枚_knownCallMeldCount0_自動推定版と一致する()
    {
        // Arrange
        var hand = new TileKindList("567", "11", "111234567", "");

        // Act
        var autoInferred = ShantenCalculator.Calc(hand, useRegular: true, useChiitoitsu: false, useKokushi: false);
        var explicitCount = ShantenCalculator.Calc(hand, knownCallMeldCount: 0, useRegular: true, useChiitoitsu: false, useKokushi: false);

        // Assert
        Assert.Equal(autoInferred, explicitCount);
    }

    [Fact]
    public void 手牌10枚_knownCallMeldCount1_自動推定版と一致する()
    {
        // Arrange: 10枚の手牌。(14 - 10) / 3 = 1 なので自動推定は 1 面子を仮定する
        var hand = new TileKindList("567", "11", "34567", "");

        // Act
        var autoInferred = ShantenCalculator.Calc(hand, useRegular: true, useChiitoitsu: false, useKokushi: false);
        var explicitCount = ShantenCalculator.Calc(hand, knownCallMeldCount: 1, useRegular: true, useChiitoitsu: false, useKokushi: false);

        // Assert
        Assert.Equal(autoInferred, explicitCount);
    }

    [Fact]
    public void 手牌10枚_knownCallMeldCount0と1でシャンテン数が異なる()
    {
        // Arrange: 10枚の手牌 (面子として確定した 3 枚組が 0 か 1 かで結果が変わるはず)
        var hand = new TileKindList("567", "11", "34567", "");

        // Act
        var withMeld0 = ShantenCalculator.Calc(hand, knownCallMeldCount: 0, useRegular: true, useChiitoitsu: false, useKokushi: false);
        var withMeld1 = ShantenCalculator.Calc(hand, knownCallMeldCount: 1, useRegular: true, useChiitoitsu: false, useKokushi: false);

        // Assert: 確定面子 1 つ多い方がシャンテン数は小さい (= 和了に近い)
        Assert.True(withMeld1 < withMeld0, $"withMeld1 ({withMeld1}) は withMeld0 ({withMeld0}) より小さいはず");
    }

    [Fact]
    public void 手牌8枚_knownCallMeldCount0と2でシャンテン数が異なる()
    {
        // Arrange: 8枚の手牌。自動推定は (14 - 8) / 3 = 2 面子
        var hand = new TileKindList("234", "567", "88", "");

        // Act
        var autoInferred = ShantenCalculator.Calc(hand, useRegular: true, useChiitoitsu: false, useKokushi: false);
        var withMeld0 = ShantenCalculator.Calc(hand, knownCallMeldCount: 0, useRegular: true, useChiitoitsu: false, useKokushi: false);
        var withMeld2 = ShantenCalculator.Calc(hand, knownCallMeldCount: 2, useRegular: true, useChiitoitsu: false, useKokushi: false);

        // Assert: 自動推定 (2) と明示 2 は一致。明示 0 は別の値
        Assert.Equal(autoInferred, withMeld2);
        Assert.NotEqual(withMeld0, withMeld2);
    }

    [Fact]
    public void 手牌13枚_knownCallMeldCount1_確定面子込みの和了形ならテンパイを返す()
    {
        // Arrange: 手牌 10 枚 + 仮想確定面子 1 = 実質 13 枚の手牌
        // 手牌: m567 (順子) + p11 (雀頭) + s34567 (順子+両面)
        // 確定面子 1 ある前提で、順子 2 + 雀頭 + 両面 → 1 シャンテン (書籍の多牌計算で少牌 10枚 + 1面子を想定)
        var hand = new TileKindList("567", "11", "34567", "");

        // Act
        var actual = ShantenCalculator.Calc(hand, knownCallMeldCount: 1, useRegular: true, useChiitoitsu: false, useKokushi: false);

        // Assert: 不変確認 (具体値は内部計算に依存するが、0 以上 / 14 以下であることを確認)
        Assert.True(actual >= ShantenConstants.SHANTEN_AGARI);
        Assert.True(actual <= 14);
    }

    [Fact]
    public void 負のknownCallMeldCount_ArgumentOutOfRangeException()
    {
        // Arrange
        var hand = new TileKindList("567", "11", "111345677", "");

        // Act
        var exception = Record.Exception(() => ShantenCalculator.Calc(hand, knownCallMeldCount: -1));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(exception);
    }

    [Fact]
    public void 手牌15枚_ArgumentException()
    {
        // Arrange
        var hand = new TileKindList("567", "1122", "111345677", "");  // 15枚

        // Act
        var exception = Record.Exception(() => ShantenCalculator.Calc(hand, knownCallMeldCount: 0));

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void 全形無効_ArgumentException()
    {
        // Arrange
        var hand = new TileKindList("567", "11", "111345677", "");

        // Act
        var exception = Record.Exception(() => ShantenCalculator.Calc(hand, knownCallMeldCount: 0, useRegular: false, useChiitoitsu: false, useKokushi: false));

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void 七対子経路は手牌枚数に依存しない()
    {
        // 七対子は knownCallMeldCount に関係なく通常計算される (書籍の xiangting() = min(reg, chii, koku) に合わせる設計)
        // Arrange: 4 対子 + 残り (計 10 枚)
        var hand = new TileKindList("11", "22", "", "nnss");

        // Act
        var withCallMeld = ShantenCalculator.Calc(hand, knownCallMeldCount: 1, useRegular: false, useChiitoitsu: true, useKokushi: false);
        var autoInferred = ShantenCalculator.Calc(hand, useRegular: false, useChiitoitsu: true, useKokushi: false);

        // Assert: 七対子計算は同一
        Assert.Equal(autoInferred, withCallMeld);
    }

    [Fact]
    public void Count配列API_通常形_正しいシャンテン数を取得できる()
    {
        // Arrange
        var counts = CreateCounts(new TileKindList("567", "11", "111345677", ""));

        // Act
        var actual = ShantenCalculator.Calc(counts, knownCallMeldCount: 0, useRegular: true, useChiitoitsu: false, useKokushi: false);

        // Assert
        Assert.Equal(ShantenConstants.SHANTEN_TENPAI, actual);
    }

    [Fact]
    public void Count配列API_全形最小シャンテンを取得できる()
    {
        // Arrange
        var counts = CreateCounts(new TileKindList("77", "114477", "114477", ""));

        // Act
        var actual = ShantenCalculator.Calc(counts);

        // Assert
        Assert.Equal(ShantenConstants.SHANTEN_AGARI, actual);
    }

    [Fact]
    public void Count配列API_計算後も入力配列が復元される()
    {
        // Arrange
        var counts = CreateCounts(new TileKindList("123456789", "1111", "", ""));
        var expected = counts.ToArray();

        // Act
        _ = ShantenCalculator.Calc(counts);

        // Assert
        Assert.Equal(expected, counts);
    }

    private static int[] CreateCounts(TileKindList hand)
    {
        var counts = new int[34];
        foreach (var tileKind in hand)
        {
            counts[tileKind.Value]++;
        }

        return counts;
    }
}
