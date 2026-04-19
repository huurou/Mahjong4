using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Moq;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_SettleWinPaoTests
{
    // 点数移動のみを検証するテストで winTile の値にはこだわらないためのダミー牌
    private static readonly Tile DUMMY_WIN_TILE = new(0);

    private static ScoreResult YakumanResult(PlayerIndex winnerIndex, int gain, PlayerIndex? loserIndex = null)
    {
        var array = new PointArray(new Point(0)).AddPoint(winnerIndex, gain);
        if (loserIndex is not null)
        {
            array = array.SubtractPoint(loserIndex, gain);
        }
        else
        {
            var each = gain / 3;
            for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
            {
                var p = new PlayerIndex(i);
                if (p != winnerIndex)
                {
                    array = array.SubtractPoint(p, each);
                }
            }
        }
        return new ScoreResult(
            0,
            0,
            array,
            [new YakuInfo(52, "大三元", null, 1, IsPaoEligible: true)]
        );
    }

    private static Round CreateBaseRound()
    {
        return RoundTestHelper.CreateRound().Haipai()
             with
        { PointArray = new PointArray(new Point(25000)) };
    }

    [Fact]
    public void ツモ和了で包適用_責任者1人が全額負担()
    {
        // Arrange
        var winner = new PlayerIndex(0);
        var responsible = new PlayerIndex(2);
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(YakumanResult(winner, 48000));
        var round = CreateBaseRound() with
        {
            PaoResponsibleArray = new PlayerResponsibilityArray().SetResponsible(winner, responsible),
        };

        // Act
        var (result, _) = round.SettleWin([winner], winner, WinType.Tsumo, DUMMY_WIN_TILE, mock.Object);

        // Assert
        Assert.Equal(25000 + 48000, result.PointArray[winner].Value);
        Assert.Equal(25000 - 48000, result.PointArray[responsible].Value);
        Assert.Equal(25000, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000, result.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void ロン和了で包適用_放銃者と責任者で折半()
    {
        // Arrange
        var winner = new PlayerIndex(0);
        var responsible = new PlayerIndex(2);
        var loser = new PlayerIndex(3);
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(YakumanResult(winner, 32000, loser));
        var round = CreateBaseRound() with
        {
            PaoResponsibleArray = new PlayerResponsibilityArray().SetResponsible(winner, responsible),
        };

        // Act
        var (result, _) = round.SettleWin([winner], loser, WinType.Ron, DUMMY_WIN_TILE, mock.Object);

        // Assert
        Assert.Equal(25000 + 32000, result.PointArray[winner].Value);
        Assert.Equal(25000 - 16000, result.PointArray[responsible].Value);
        Assert.Equal(25000 - 16000, result.PointArray[loser].Value);
        Assert.Equal(25000, result.PointArray[new PlayerIndex(1)].Value);
    }

    [Fact]
    public void ロン和了で放銃者と責任者が同一_全額を同一プレイヤーが負担()
    {
        // Arrange
        var winner = new PlayerIndex(0);
        var loserAndResponsible = new PlayerIndex(2);
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(YakumanResult(winner, 32000, loserAndResponsible));
        var round = CreateBaseRound() with
        {
            PaoResponsibleArray = new PlayerResponsibilityArray().SetResponsible(winner, loserAndResponsible),
        };

        // Act
        var (result, _) = round.SettleWin([winner], loserAndResponsible, WinType.Ron, DUMMY_WIN_TILE, mock.Object);

        // Assert
        Assert.Equal(25000 + 32000, result.PointArray[winner].Value);
        Assert.Equal(25000 - 32000, result.PointArray[loserAndResponsible].Value);
        Assert.Equal(25000, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000, result.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void 包対象役がない_通常精算()
    {
        // Arrange
        var winner = new PlayerIndex(0);
        var responsible = new PlayerIndex(2);
        var mock = new Mock<IScoreCalculator>();
        // IsPaoEligible = false の役のみ
        var deltas = new PointArray(new Point(0))
            .AddPoint(winner, 12000)
            .SubtractPoint(new PlayerIndex(1), 4000)
            .SubtractPoint(new PlayerIndex(2), 4000)
            .SubtractPoint(new PlayerIndex(3), 4000);
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(new ScoreResult(0, 0, deltas, [new YakuInfo(1, "立直", 1)]));
        var round = CreateBaseRound() with
        {
            PaoResponsibleArray = new PlayerResponsibilityArray().SetResponsible(winner, responsible),
        };

        // Act
        var (result, _) = round.SettleWin([winner], winner, WinType.Tsumo, DUMMY_WIN_TILE, mock.Object);

        // Assert: 包適用されず均等
        Assert.Equal(25000 + 12000, result.PointArray[winner].Value);
        Assert.Equal(25000 - 4000, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 - 4000, result.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 4000, result.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void 責任者未設定の和了者_通常精算()
    {
        // Arrange
        var winner = new PlayerIndex(0);
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(YakumanResult(winner, 48000));
        var round = CreateBaseRound();

        // Act
        var (result, _) = round.SettleWin([winner], winner, WinType.Tsumo, DUMMY_WIN_TILE, mock.Object);

        // Assert: 包適用されず通常均等分配
        Assert.Equal(25000 + 48000, result.PointArray[winner].Value);
        Assert.Equal(25000 - 16000, result.PointArray[new PlayerIndex(1)].Value);
        Assert.Equal(25000 - 16000, result.PointArray[new PlayerIndex(2)].Value);
        Assert.Equal(25000 - 16000, result.PointArray[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void 大三元ツモで包適用_通知明細のScoreResultも責任者1人負担のdelta()
    {
        // Arrange: 通知用 ScoreResult.PointDeltas が調整後 (責任者1人負担) になっているか検証
        var winner = new PlayerIndex(0);
        var responsible = new PlayerIndex(2);
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(YakumanResult(winner, 48000));
        var round = CreateBaseRound() with
        {
            PaoResponsibleArray = new PlayerResponsibilityArray().SetResponsible(winner, responsible),
        };

        // Act
        var (_, details) = round.SettleWin([winner], winner, WinType.Tsumo, DUMMY_WIN_TILE, mock.Object);

        // Assert
        var adjusted = details.Winners[0].ScoreResult.PointDeltas;
        Assert.Equal(48000, adjusted[winner].Value);
        Assert.Equal(-48000, adjusted[responsible].Value);
        Assert.Equal(0, adjusted[new PlayerIndex(1)].Value);
        Assert.Equal(0, adjusted[new PlayerIndex(3)].Value);
    }

    [Fact]
    public void 大三元ロンで包適用_通知明細のScoreResultも放銃者と責任者折半()
    {
        // Arrange
        var winner = new PlayerIndex(0);
        var responsible = new PlayerIndex(2);
        var loser = new PlayerIndex(3);
        var mock = new Mock<IScoreCalculator>();
        mock.Setup(x => x.Calculate(It.IsAny<ScoreRequest>()))
            .Returns(YakumanResult(winner, 32000, loser));
        var round = CreateBaseRound() with
        {
            PaoResponsibleArray = new PlayerResponsibilityArray().SetResponsible(winner, responsible),
        };

        // Act
        var (_, details) = round.SettleWin([winner], loser, WinType.Ron, DUMMY_WIN_TILE, mock.Object);

        // Assert
        var adjusted = details.Winners[0].ScoreResult.PointDeltas;
        Assert.Equal(32000, adjusted[winner].Value);
        Assert.Equal(-16000, adjusted[responsible].Value);
        Assert.Equal(-16000, adjusted[loser].Value);
        Assert.Equal(0, adjusted[new PlayerIndex(1)].Value);
    }
}
