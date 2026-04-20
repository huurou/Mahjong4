using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
using GameTile = Mahjong.Lib.Game.Tiles.Tile;

namespace Mahjong.Lib.Game.Scoring.Tests;

public class ScoreCalculatorImpl_CalculateTests
{
    private static GameRules DefaultRules { get; } = new();

    private static GameTile TileOf(int kind, int copy = 0)
    {
        return new(kind * 4 + copy);
    }

    private static Round CreateRound(PlayerIndex turn, PlayerIndex dealerIndex)
    {
        return new Round(
            RoundWind.East,
            new RoundNumber(dealerIndex.Value),
            new Honba(0),
            new KyoutakuRiichiCount(0),
            turn,
            new PointArray(new Point(25000)),
            new Wall(Enumerable.Range(GameTile.ID_MIN, GameTile.ID_MAX + 1).Select(x => new GameTile(x)))
        );
    }

    private static Round WithHand(Round round, PlayerIndex index, IEnumerable<GameTile> tiles)
    {
        var handArray = round.HandArray;
        foreach (var t in handArray[index].ToList())
        {
            handArray = handArray.RemoveTile(index, t);
        }
        handArray = handArray.AddTiles(index, tiles);
        return round with { HandArray = handArray };
    }

    [Fact]
    public void 子のタンヤオロン_和了者と放銃者の点数が反映される()
    {
        // Arrange
        // 手牌 (門前タンヤオ 1 翻 / 40 符 = 1300 点を放銃者が支払う想定)
        // 萬 234 筒 234 筒 567 索 234 萬 44 + ロン 萬4
        // 萬 234 (kinds 1,2,3) 筒 234 (10,11,12) 筒 567 (14,15,16) 索 234 (19,20,21) 萬 44 (3,3) + ロン 萬4 kind 3
        var winnerIndex = new PlayerIndex(1);
        var loserIndex = new PlayerIndex(3);
        var round = CreateRound(turn: winnerIndex, dealerIndex: new PlayerIndex(0));
        var winTile = TileOf(3);
        var handTiles = new[]
        {
            TileOf(1), TileOf(2), TileOf(3, 1),
            TileOf(10), TileOf(11), TileOf(12),
            TileOf(14), TileOf(15), TileOf(16),
            TileOf(19), TileOf(20), TileOf(21),
            TileOf(3, 2),
        };
        round = WithHand(round, winnerIndex, handTiles);
        var calc = new ScoreCalculatorImpl(DefaultRules);
        var request = new ScoreRequest(round, winnerIndex, loserIndex, WinType.Ron, winTile);

        // Act
        var result = calc.Calculate(request);

        // Assert
        Assert.True(result.Han >= 1);
        Assert.True(result.PointDeltas[winnerIndex].Value > 0);
        Assert.True(result.PointDeltas[loserIndex].Value < 0);
        Assert.Equal(-result.PointDeltas[loserIndex].Value, result.PointDeltas[winnerIndex].Value);
    }

    [Fact]
    public void 子のツモ_親子で点数移動量が異なる()
    {
        // Arrange
        // 子のリーチツモ (2 翻 = 40符2翻 親700子400 と仮定) ツモ牌 索2
        // 萬 234 筒 234 筒 567 索 234 萬 44 でロン → ツモに変更。ツモ牌は索2 (kind 19)
        var winnerIndex = new PlayerIndex(1);
        var dealerIndex = new PlayerIndex(0);
        var round = CreateRound(turn: winnerIndex, dealerIndex: dealerIndex);
        round = round with
        {
            PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(
                winnerIndex,
                round.PlayerRoundStatusArray[winnerIndex] with { IsRiichi = true }
            ),
        };
        var winTile = TileOf(19);
        var handTiles = new[]
        {
            TileOf(1), TileOf(2), TileOf(3, 1),
            TileOf(10), TileOf(11), TileOf(12),
            TileOf(14), TileOf(15), TileOf(16),
            TileOf(3, 2), TileOf(3, 3),
            TileOf(20), TileOf(21),
            TileOf(19), // ツモ牌 (手牌末尾)
        };
        round = WithHand(round, winnerIndex, handTiles);
        var calc = new ScoreCalculatorImpl(DefaultRules);
        var request = new ScoreRequest(round, winnerIndex, winnerIndex, WinType.Tsumo, winTile);

        // Act
        var result = calc.Calculate(request);

        // Assert
        Assert.True(result.PointDeltas[winnerIndex].Value > 0);
        Assert.True(result.PointDeltas[dealerIndex].Value < 0);
        // 子ツモ: 親が子よりも多く払う
        var otherChildren = Enumerable.Range(0, 4)
            .Select(x => new PlayerIndex(x))
            .Where(x => x != winnerIndex && x != dealerIndex)
            .ToList();
        foreach (var child in otherChildren)
        {
            Assert.True(result.PointDeltas[child].Value < 0);
            Assert.True(Math.Abs(result.PointDeltas[dealerIndex].Value) >= Math.Abs(result.PointDeltas[child].Value));
        }
        // 合計 0
        var total = Enumerable.Range(0, 4).Sum(x => result.PointDeltas[new PlayerIndex(x)].Value);
        Assert.Equal(0, total);
    }

    [Fact]
    public void 親のツモ_親に3人分の点数が集まる()
    {
        // Arrange
        // 親 (PlayerIndex 0) のツモ和了。他3人が同額ずつ払う
        // IsFirstTurnBeforeDiscard=false で天和判定を回避する
        var winnerIndex = new PlayerIndex(0);
        var round = CreateRound(turn: winnerIndex, dealerIndex: winnerIndex);
        round = round with
        {
            PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(
                winnerIndex,
                round.PlayerRoundStatusArray[winnerIndex] with
                {
                    IsRiichi = true,
                    IsFirstTurnBeforeDiscard = false,
                }
            ),
        };
        var winTile = TileOf(19);
        var handTiles = new[]
        {
            TileOf(1), TileOf(2), TileOf(3, 1),
            TileOf(10), TileOf(11), TileOf(12),
            TileOf(14), TileOf(15), TileOf(16),
            TileOf(3, 2), TileOf(3, 3),
            TileOf(20), TileOf(21),
            TileOf(19),
        };
        round = WithHand(round, winnerIndex, handTiles);
        var calc = new ScoreCalculatorImpl(DefaultRules);
        var request = new ScoreRequest(round, winnerIndex, winnerIndex, WinType.Tsumo, winTile);

        // Act
        var result = calc.Calculate(request);

        // Assert
        Assert.True(result.PointDeltas[winnerIndex].Value > 0);
        // 他3人は同額の負
        var others = Enumerable.Range(1, 3).Select(x => new PlayerIndex(x)).ToList();
        var amounts = others.Select(x => result.PointDeltas[x].Value).Distinct().ToList();
        Assert.Single(amounts);
        Assert.True(amounts[0] < 0);
        // 合計 0
        var total = Enumerable.Range(0, 4).Sum(x => result.PointDeltas[new PlayerIndex(x)].Value);
        Assert.Equal(0, total);
    }

    [Fact]
    public void 役満大三元ロン_役満倍数1の役が含まれる()
    {
        // Arrange
        // 大三元: 萬 234 筒 234 白白白 發發發 中中 + ロン 中
        // kinds: 1,2,3 / 10,11,12 / 31,31,31 / 32,32,32 / 33,33 + ロン kind 33
        var winnerIndex = new PlayerIndex(1);
        var loserIndex = new PlayerIndex(3);
        var round = CreateRound(turn: winnerIndex, dealerIndex: new PlayerIndex(0));
        var winTile = TileOf(33);
        var handTiles = new[]
        {
            TileOf(1), TileOf(2), TileOf(3),
            TileOf(10, 0), TileOf(10, 1),
            TileOf(31, 0), TileOf(31, 1), TileOf(31, 2),
            TileOf(32, 0), TileOf(32, 1), TileOf(32, 2),
            TileOf(33, 0), TileOf(33, 1),
        };
        round = WithHand(round, winnerIndex, handTiles);
        var calc = new ScoreCalculatorImpl(DefaultRules);
        var request = new ScoreRequest(round, winnerIndex, loserIndex, WinType.Ron, winTile);

        // Act
        var result = calc.Calculate(request);

        // Assert
        Assert.Contains(result.YakuInfos, x => x.Name == "大三元");
        Assert.Contains(result.YakuInfos, x => x.YakumanCount >= 1);
        Assert.Contains(result.YakuInfos, x => x.IsPaoEligible); // 大三元は包対象
    }
}
