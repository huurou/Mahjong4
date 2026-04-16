using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_SettleRyuukyokuTests
{
    private static Round CreateBaseRound()
    {
        return RoundTestHelper.CreateRound().Haipai()
             with
        { PointArray = new PointArray(new Point(25000)) };
    }

    [Fact]
    public void 荒牌平局_1人テンパイ_テンパイ者3000獲得ノーテン各1000支払い()
    {
        // Arrange
        var round = CreateBaseRound();
        var tenpai = new PlayerIndex(0);

        // Act
        var result = round.SettleRyuukyoku(RyuukyokuType.KouhaiHeikyoku, [tenpai], []);

        // Assert
        Assert.Equal(25000 + 3000, result.PointArray[tenpai].Value);
        Assert.Equal(25000 - 1000, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 - 1000, result.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 1000, result.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void 荒牌平局_2人テンパイ_テンパイ者各1500獲得ノーテン各1500支払い()
    {
        // Arrange
        var round = CreateBaseRound();

        // Act
        var result = round.SettleRyuukyoku(
            RyuukyokuType.KouhaiHeikyoku,
            [new PlayerIndex(0), new PlayerIndex(2)], []);

        // Assert
        Assert.Equal(25000 + 1500, result.PointArray[new PlayerIndex(0)].Value);
        Assert.Equal(25000 - 1500, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 + 1500, result.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 1500, result.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void 荒牌平局_3人テンパイ_テンパイ者各1000獲得ノーテン3000支払い()
    {
        // Arrange
        var round = CreateBaseRound();

        // Act
        var result = round.SettleRyuukyoku(
            RyuukyokuType.KouhaiHeikyoku,
            [new PlayerIndex(0), new PlayerIndex(1), new PlayerIndex(2)], []);

        // Assert
        Assert.Equal(25000 + 1000, result.PointArray[new PlayerIndex(0)].Value);
        Assert.Equal(25000 + 1000, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 + 1000, result.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 3000, result.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void 荒牌平局_0人テンパイ_点数移動なし()
    {
        // Arrange
        var round = CreateBaseRound();

        // Act
        var result = round.SettleRyuukyoku(RyuukyokuType.KouhaiHeikyoku, [], []);

        // Assert
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            Assert.Equal(25000, result.PointArray[new PlayerIndex(i)].Value);
        }
    }

    [Fact]
    public void 荒牌平局_4人テンパイ_点数移動なし()
    {
        // Arrange
        var round = CreateBaseRound();

        // Act
        var result = round.SettleRyuukyoku(
            RyuukyokuType.KouhaiHeikyoku,
            [new PlayerIndex(0), new PlayerIndex(1), new PlayerIndex(2), new PlayerIndex(3)], []);

        // Assert
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            Assert.Equal(25000, result.PointArray[new PlayerIndex(i)].Value);
        }
    }

    [Fact]
    public void 荒牌平局_テンパイ者重複_例外()
    {
        // Arrange
        var round = CreateBaseRound();
        var duplicate = System.Collections.Immutable.ImmutableArray.Create(new PlayerIndex(0), new PlayerIndex(0));

        // Act
        var exception = Record.Exception(() =>
            round.SettleRyuukyoku(RyuukyokuType.KouhaiHeikyoku, duplicate, []));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void 荒牌平局_流し満貫者重複_例外()
    {
        // Arrange
        var round = CreateBaseRound();
        var duplicate = System.Collections.Immutable.ImmutableArray.Create(new PlayerIndex(0), new PlayerIndex(0));

        // Act
        var exception = Record.Exception(() =>
            round.SettleRyuukyoku(RyuukyokuType.KouhaiHeikyoku, [], duplicate));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void 親流し満貫_4000オールが移動()
    {
        // Arrange
        var round = CreateBaseRound();

        // Act
        var result = round.SettleRyuukyoku(RyuukyokuType.KouhaiHeikyoku, [], [new PlayerIndex(0)]);

        // Assert
        Assert.Equal(25000 + 12000, result.PointArray[new PlayerIndex(0)].Value);
        Assert.Equal(25000 - 4000, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 - 4000, result.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 4000, result.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void 子流し満貫_親4000子2000子2000が移動()
    {
        // Arrange
        var round = CreateBaseRound();

        // Act
        var result = round.SettleRyuukyoku(RyuukyokuType.KouhaiHeikyoku, [], [new PlayerIndex(1)]);

        // Assert
        Assert.Equal(25000 - 4000, result.PointArray[new PlayerIndex(0)].Value);
        Assert.Equal(25000 + 8000, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 - 2000, result.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 2000, result.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void 流し満貫がいる場合_テンパイ料は発生しない()
    {
        // Arrange
        var round = CreateBaseRound();

        // Act
        var result = round.SettleRyuukyoku(
            RyuukyokuType.KouhaiHeikyoku,
            [new PlayerIndex(0)],
            [new PlayerIndex(1)]);

        // Assert
        Assert.Equal(25000 - 4000, result.PointArray[new PlayerIndex(0)].Value);
        Assert.Equal(25000 + 8000, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 - 2000, result.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 2000, result.PointArray[new PlayerIndex(3)].Value);
    }

    [Theory]
    [InlineData(RyuukyokuType.KyuushuKyuuhai)]
    [InlineData(RyuukyokuType.Suufonrenda)]
    [InlineData(RyuukyokuType.Suukaikan)]
    [InlineData(RyuukyokuType.SuuchaRiichi)]
    [InlineData(RyuukyokuType.SanchaHou)]
    public void 途中流局_点数移動なし(RyuukyokuType type)
    {
        // Arrange
        var round = CreateBaseRound();

        // Act
        var result = round.SettleRyuukyoku(type, [], []);

        // Assert
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            Assert.Equal(25000, result.PointArray[new PlayerIndex(i)].Value);
        }
    }
}
