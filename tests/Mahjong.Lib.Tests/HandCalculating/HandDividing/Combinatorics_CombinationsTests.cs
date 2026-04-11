using Mahjong.Lib.HandCalculating.HandDividing;

namespace Mahjong.Lib.Tests.HandCalculating.HandDividing;

public class Combinatorics_CombinationsTests
{
    [Fact]
    public void 通常の組み合わせ生成_正しい組み合わせを返す()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        var k = 2;

        // Act
        var actual = Combinatorics.Combinations(source, k).Select(x => x.ToArray()).ToArray();

        // Assert
        int[][] expected =
        [
            [1, 2],
            [1, 3],
            [2, 3],
        ];
        Assert.Equal(expected.Length, actual.Length);
        foreach (var comb in expected)
        {
            Assert.Contains(actual, x => x.SequenceEqual(comb));
        }
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Kが0_空の組み合わせのみ返る()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        var k = 0;

        // Act
        var actual = Combinatorics.Combinations(source, k).ToArray();

        // Assert
        Assert.Single(actual);
        Assert.Empty(actual[0]);
    }

    [Fact]
    public void Kが負数_何も返さない()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        var k = -1;

        // Act
        var actual = Combinatorics.Combinations(source, k).ToArray();

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void Kが要素数より大きい_何も返さない()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        var k = 4;

        // Act
        var actual = Combinatorics.Combinations(source, k).ToArray();

        // Assert
        Assert.Empty(actual);
    }
}
