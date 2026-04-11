using Mahjong.Lib.Games;
using Mahjong.Lib.HandCalculating.Scores;

namespace Mahjong.Lib.Tests.HandCalculating.Scores;

public class ScoreCalculator_CalcTests
{
    [Theory]
    [InlineData(20, 2, 700, 400)]
    [InlineData(20, 3, 1300, 700)]
    [InlineData(20, 4, 2600, 1300)]
    [InlineData(25, 3, 1600, 800)]
    [InlineData(25, 4, 3200, 1600)]
    [InlineData(30, 1, 500, 300)]
    [InlineData(30, 2, 1000, 500)]
    [InlineData(30, 3, 2000, 1000)]
    [InlineData(30, 4, 3900, 2000)]
    [InlineData(40, 1, 700, 400)]
    [InlineData(40, 2, 1300, 700)]
    [InlineData(40, 3, 2600, 1300)]
    [InlineData(50, 1, 800, 400)]
    [InlineData(50, 2, 1600, 800)]
    [InlineData(50, 3, 3200, 1600)]
    [InlineData(60, 1, 1000, 500)]
    [InlineData(60, 2, 2000, 1000)]
    [InlineData(60, 3, 3900, 2000)]
    [InlineData(70, 1, 1200, 600)]
    [InlineData(70, 2, 2300, 1200)]
    [InlineData(80, 1, 1300, 700)]
    [InlineData(80, 2, 2600, 1300)]
    [InlineData(90, 1, 1500, 800)]
    [InlineData(90, 2, 2900, 1500)]
    [InlineData(100, 1, 1600, 800)]
    [InlineData(100, 2, 3200, 1600)]
    [InlineData(110, 2, 3600, 1800)]
    public void 子のツモ_満貫未満_符と翻に応じた点数を返す(int fu, int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.South };
        var gameRules = new GameRules();

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(30, 4, 4000, 2000)]
    [InlineData(60, 3, 4000, 2000)]
    public void 子のツモ_切り上げ満貫_符と翻に応じた点数を返す(int fu, int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.South };
        var gameRules = new GameRules { KiriageEnabled = true };

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(5, 4000, 2000)]
    [InlineData(6, 6000, 3000)]
    [InlineData(7, 6000, 3000)]
    [InlineData(8, 8000, 4000)]
    [InlineData(9, 8000, 4000)]
    [InlineData(10, 8000, 4000)]
    [InlineData(11, 12000, 6000)]
    [InlineData(12, 12000, 6000)]
    public void 子のツモ_満貫以上役満未満_翻に応じた点数を返す(int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.South };
        var gameRules = new GameRules();
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(13, 16000, 8000)]
    [InlineData(26, 16000, 8000)]
    [InlineData(39, 16000, 8000)]
    [InlineData(52, 16000, 8000)]
    [InlineData(65, 16000, 8000)]
    [InlineData(78, 16000, 8000)]
    public void 子のツモ_数え役満Limited_13翻以上はすべて役満の点数を返す(int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.South };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.Limited };
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: false);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(13, 12000, 6000)]
    [InlineData(26, 12000, 6000)]
    [InlineData(39, 12000, 6000)]
    [InlineData(52, 12000, 6000)]
    [InlineData(65, 12000, 6000)]
    [InlineData(78, 12000, 6000)]
    public void 子のツモ_数え役満Sanbaiman_13翻以上はすべて三倍満の点数を返す(int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.South };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.Sanbaiman };
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: false);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(13, 16000, 8000)]
    [InlineData(26, 32000, 16000)]
    [InlineData(39, 48000, 24000)]
    [InlineData(52, 64000, 32000)]
    [InlineData(65, 80000, 40000)]
    [InlineData(78, 96000, 48000)]
    public void 子のツモ_数え役満NoLimit_13翻ごとに役満がかさなった点数を返す(int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.South };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.NoLimit };
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: false);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(13, 16000, 8000)]
    [InlineData(26, 32000, 16000)]
    [InlineData(39, 48000, 24000)]
    [InlineData(52, 64000, 32000)]
    [InlineData(65, 80000, 40000)]
    [InlineData(78, 96000, 48000)]
    public void 子のツモ_役満_翻に応じた点数を返す(int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.South };
        var gameRules = new GameRules();
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: true);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(25, 2, 1600)]
    [InlineData(25, 3, 3200)]
    [InlineData(25, 4, 6400)]
    [InlineData(30, 1, 1000)]
    [InlineData(30, 2, 2000)]
    [InlineData(30, 3, 3900)]
    [InlineData(30, 4, 7700)]
    [InlineData(40, 1, 1300)]
    [InlineData(40, 2, 2600)]
    [InlineData(40, 3, 5200)]
    [InlineData(50, 1, 1600)]
    [InlineData(50, 2, 3200)]
    [InlineData(50, 3, 6400)]
    [InlineData(60, 1, 2000)]
    [InlineData(60, 2, 3900)]
    [InlineData(60, 3, 7700)]
    [InlineData(70, 1, 2300)]
    [InlineData(70, 2, 4500)]
    [InlineData(80, 1, 2600)]
    [InlineData(80, 2, 5200)]
    [InlineData(90, 1, 2900)]
    [InlineData(90, 2, 5800)]
    [InlineData(100, 1, 3200)]
    [InlineData(100, 2, 6400)]
    [InlineData(110, 1, 3600)]
    [InlineData(110, 2, 7100)]
    public void 子のロン_満貫未満_符と翻に応じた点数を返す(int fu, int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.South };
        var gameRules = new GameRules();

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(30, 4, 8000)]
    [InlineData(60, 3, 8000)]
    public void 子のロン_切り上げ満貫_符と翻に応じた点数を返す(int fu, int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.South };
        var gameRules = new GameRules { KiriageEnabled = true };

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(5, 8000)]
    [InlineData(6, 12000)]
    [InlineData(7, 12000)]
    [InlineData(8, 16000)]
    [InlineData(9, 16000)]
    [InlineData(10, 16000)]
    [InlineData(11, 24000)]
    [InlineData(12, 24000)]
    public void 子のロン_満貫以上役満未満_翻に応じた点数を返す(int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.South };
        var gameRules = new GameRules();
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(13, 32000)]
    [InlineData(26, 32000)]
    [InlineData(39, 32000)]
    [InlineData(52, 32000)]
    [InlineData(65, 32000)]
    [InlineData(78, 32000)]
    public void 子のロン_数え役満Limited_13翻以上はすべて役満の点数を返す(int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.South };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.Limited };
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: false);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(13, 24000)]
    [InlineData(26, 24000)]
    [InlineData(39, 24000)]
    [InlineData(52, 24000)]
    [InlineData(65, 24000)]
    [InlineData(78, 24000)]
    public void 子のロン_数え役満Sanbaiman_13翻以上はすべて三倍満の点数を返す(int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.South };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.Sanbaiman };
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: false);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(13, 32000)]
    [InlineData(26, 64000)]
    [InlineData(39, 96000)]
    [InlineData(52, 128000)]
    [InlineData(65, 160000)]
    [InlineData(78, 192000)]
    public void 子のロン_数え役満NoLimit_13翻ごとに役満がかさなった点数を返す(int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.South };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.NoLimit };
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: false);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(13, 32000)]
    [InlineData(26, 64000)]
    [InlineData(39, 96000)]
    [InlineData(52, 128000)]
    [InlineData(65, 160000)]
    [InlineData(78, 192000)]
    public void 子のロン_役満_翻に応じた点数を返す(int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.South };
        var gameRules = new GameRules();
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: true);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(20, 2, 700, 700)]
    [InlineData(20, 3, 1300, 1300)]
    [InlineData(20, 4, 2600, 2600)]
    [InlineData(25, 3, 1600, 1600)]
    [InlineData(25, 4, 3200, 3200)]
    [InlineData(30, 1, 500, 500)]
    [InlineData(30, 2, 1000, 1000)]
    [InlineData(30, 3, 2000, 2000)]
    [InlineData(30, 4, 3900, 3900)]
    [InlineData(40, 1, 700, 700)]
    [InlineData(40, 2, 1300, 1300)]
    [InlineData(40, 3, 2600, 2600)]
    [InlineData(50, 1, 800, 800)]
    [InlineData(50, 2, 1600, 1600)]
    [InlineData(50, 3, 3200, 3200)]
    [InlineData(60, 1, 1000, 1000)]
    [InlineData(60, 2, 2000, 2000)]
    [InlineData(60, 3, 3900, 3900)]
    [InlineData(70, 1, 1200, 1200)]
    [InlineData(70, 2, 2300, 2300)]
    [InlineData(80, 1, 1300, 1300)]
    [InlineData(80, 2, 2600, 2600)]
    [InlineData(90, 1, 1500, 1500)]
    [InlineData(90, 2, 2900, 2900)]
    [InlineData(100, 1, 1600, 1600)]
    [InlineData(100, 2, 3200, 3200)]
    [InlineData(110, 2, 3600, 3600)]
    public void 親のツモ_満貫未満_符と翻に応じた点数を返す(int fu, int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.East };
        var gameRules = new GameRules();

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(30, 4, 4000, 4000)]
    [InlineData(60, 3, 4000, 4000)]
    public void 親のツモ_切り上げ満貫_符と翻に応じた点数を返す(int fu, int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.East };
        var gameRules = new GameRules { KiriageEnabled = true };

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(5, 4000, 4000)]
    [InlineData(6, 6000, 6000)]
    [InlineData(7, 6000, 6000)]
    [InlineData(8, 8000, 8000)]
    [InlineData(9, 8000, 8000)]
    [InlineData(10, 8000, 8000)]
    [InlineData(11, 12000, 12000)]
    [InlineData(12, 12000, 12000)]
    public void 親のツモ_満貫以上役満未満_翻に応じた点数を返す(int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.East };
        var gameRules = new GameRules();
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(13, 16000, 16000)]
    [InlineData(26, 16000, 16000)]
    [InlineData(39, 16000, 16000)]
    [InlineData(52, 16000, 16000)]
    [InlineData(65, 16000, 16000)]
    [InlineData(78, 16000, 16000)]
    public void 親のツモ_数え役満Limited_13翻以上はすべて役満の点数を返す(int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.East };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.Limited };
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: false);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(13, 12000, 12000)]
    [InlineData(26, 12000, 12000)]
    [InlineData(39, 12000, 12000)]
    [InlineData(52, 12000, 12000)]
    [InlineData(65, 12000, 12000)]
    [InlineData(78, 12000, 12000)]
    public void 親のツモ_数え役満Sanbaiman_13翻以上はすべて三倍満の点数を返す(int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.East };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.Sanbaiman };
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: false);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(13, 16000, 16000)]
    [InlineData(26, 32000, 32000)]
    [InlineData(39, 48000, 48000)]
    [InlineData(52, 64000, 64000)]
    [InlineData(65, 80000, 80000)]
    [InlineData(78, 96000, 96000)]
    public void 親のツモ_数え役満NoLimit_13翻ごとに役満がかさなった点数を返す(int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.East };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.NoLimit };
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: false);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(13, 16000, 16000)]
    [InlineData(26, 32000, 32000)]
    [InlineData(39, 48000, 48000)]
    [InlineData(52, 64000, 64000)]
    [InlineData(65, 80000, 80000)]
    [InlineData(78, 96000, 96000)]
    public void 親のツモ_役満_翻に応じた点数を返す(int han, int expectedMain, int expectedSub)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.East };
        var gameRules = new GameRules();
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: true);

        // Assert
        Assert.Equal(new Score(expectedMain, expectedSub), actual);
    }

    [Theory]
    [InlineData(25, 2, 2400)]
    [InlineData(25, 3, 4800)]
    [InlineData(25, 4, 9600)]
    [InlineData(30, 1, 1500)]
    [InlineData(30, 2, 2900)]
    [InlineData(30, 3, 5800)]
    [InlineData(30, 4, 11600)]
    [InlineData(40, 1, 2000)]
    [InlineData(40, 2, 3900)]
    [InlineData(40, 3, 7700)]
    [InlineData(50, 1, 2400)]
    [InlineData(50, 2, 4800)]
    [InlineData(50, 3, 9600)]
    [InlineData(60, 1, 2900)]
    [InlineData(60, 2, 5800)]
    [InlineData(60, 3, 11600)]
    [InlineData(70, 1, 3400)]
    [InlineData(70, 2, 6800)]
    [InlineData(80, 1, 3900)]
    [InlineData(80, 2, 7700)]
    [InlineData(90, 1, 4400)]
    [InlineData(90, 2, 8700)]
    [InlineData(100, 1, 4800)]
    [InlineData(100, 2, 9600)]
    [InlineData(110, 1, 5300)]
    [InlineData(110, 2, 10600)]
    public void 親のロン_満貫未満_符と翻に応じた点数を返す(int fu, int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.East };
        var gameRules = new GameRules();

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(30, 4, 12000)]
    [InlineData(60, 3, 12000)]
    public void 親のロン_切り上げ満貫_符と翻に応じた点数を返す(int fu, int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.East };
        var gameRules = new GameRules { KiriageEnabled = true };

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(5, 12000)]
    [InlineData(6, 18000)]
    [InlineData(7, 18000)]
    [InlineData(8, 24000)]
    [InlineData(9, 24000)]
    [InlineData(10, 24000)]
    [InlineData(11, 36000)]
    [InlineData(12, 36000)]
    public void 親のロン_満貫以上役満未満_翻に応じた点数を返す(int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.East };
        var gameRules = new GameRules();
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(13, 48000)]
    [InlineData(26, 48000)]
    [InlineData(39, 48000)]
    [InlineData(52, 48000)]
    [InlineData(65, 48000)]
    [InlineData(78, 48000)]
    public void 親のロン_数え役満Limited_13翻以上はすべて役満の点数を返す(int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.East };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.Limited };
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: false);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(13, 36000)]
    [InlineData(26, 36000)]
    [InlineData(39, 36000)]
    [InlineData(52, 36000)]
    [InlineData(65, 36000)]
    [InlineData(78, 36000)]
    public void 親のロン_数え役満Sanbaiman_13翻以上はすべて三倍満の点数を返す(int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.East };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.Sanbaiman };
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: false);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(13, 48000)]
    [InlineData(26, 96000)]
    [InlineData(39, 144000)]
    [InlineData(52, 192000)]
    [InlineData(65, 240000)]
    [InlineData(78, 288000)]
    public void 親のロン_数え役満NoLimit_13翻ごとに役満がかさなった点数を返す(int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.East };
        var gameRules = new GameRules { KazoeLimit = KazoeLimit.NoLimit };
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: false);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Theory]
    [InlineData(13, 48000)]
    [InlineData(26, 96000)]
    [InlineData(39, 144000)]
    [InlineData(52, 192000)]
    [InlineData(65, 240000)]
    [InlineData(78, 288000)]
    public void 親のロン_役満_翻に応じた点数を返す(int han, int expectedMain)
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = false, PlayerWind = Wind.East };
        var gameRules = new GameRules();
        var fu = 30;

        // Act
        var actual = ScoreCalculator.Calc(fu, han, winSituation, gameRules, isYakuman: true);

        // Assert
        Assert.Equal(new Score(expectedMain), actual);
    }

    [Fact]
    public void 翻が0_例外がスローされる()
    {
        // Arrange
        var winSituation = new WinSituation();
        var gameRules = new GameRules();

        // Act
        var exception = Record.Exception(() => ScoreCalculator.Calc(30, 0, winSituation, gameRules));

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void 数え役満のルールが不正_例外がスローされる()
    {
        // Arrange
        var winSituation = new WinSituation { IsTsumo = true, PlayerWind = Wind.South };
        var gameRules = new GameRules { KazoeLimit = (KazoeLimit)3 };
        var fu = 30;

        // Act
        var exception = Record.Exception(() => ScoreCalculator.Calc(fu, 13, winSituation, gameRules, isYakuman: false));

        // Assert
        var argEx = Assert.IsType<ArgumentException>(exception);
        Assert.Contains("数え役満のルールが不正です。", argEx.Message);
    }
}
