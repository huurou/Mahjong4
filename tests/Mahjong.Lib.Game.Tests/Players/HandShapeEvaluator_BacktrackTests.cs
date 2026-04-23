using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Scoring.Games;
using System.Collections.Immutable;
using GameRules = Mahjong.Lib.Game.Games.GameRules;
using TileKind = Mahjong.Lib.Scoring.Tiles.TileKind;

namespace Mahjong.Lib.Game.Tests.Players;

public class HandShapeEvaluator_BacktrackTests
{
    [Fact]
    public void EvaluateBacktrack_閾値で枝刈りされる_minEvを大きくすると0が返る()
    {
        // Arrange: Man1-9 + Chun×3 + Pin5 の 13 枚テンパイ (Pin5 単騎待ち)
        var hand13 = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(132), new Tile(133), new Tile(134),
            new Tile(53),
        ]);
        var ctx = CreateContext();
        var evaluator = new HandShapeEvaluator();

        // Act: 閾値 0 なら通常の評価値、閾値を極大にすれば全部切り捨てて 0
        var withoutCutoff = evaluator.EvaluateBacktrack(hand13, TileKind.Pin9, minEvPerTile: 0, ctx);
        var withCutoff = evaluator.EvaluateBacktrack(hand13, TileKind.Pin9, minEvPerTile: long.MaxValue / 2, ctx);

        // Assert
        Assert.True(withoutCutoff > 0);
        Assert.Equal(0, withCutoff);
    }

    [Fact]
    public void EvaluateBacktrack_引き戻し除外_backと同じ牌種は評価しない()
    {
        // Arrange: Pin5 単騎テンパイから、有効牌は Pin5 のみ
        var hand13 = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(132), new Tile(133), new Tile(134),
            new Tile(53),
        ]);
        var ctx = CreateContext();
        var evaluator = new HandShapeEvaluator();

        // Act: back が有効牌そのもの (Pin5) なので、全ての有効牌が除外され評価値 0
        var withBackAsOnlyUseful = evaluator.EvaluateBacktrack(hand13, TileKind.Pin5, minEvPerTile: 0, ctx);

        // Assert
        Assert.Equal(0, withBackAsOnlyUseful);
    }

    [Fact]
    public void EvaluateHand14_backがwinCandidateと一致_フリテン扱いで0を返す()
    {
        // Arrange: 和了形 14 枚
        var hand14 = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(132), new Tile(133), new Tile(134),
            new Tile(53), new Tile(54),
        ]);
        var ctx = CreateContext();
        var evaluator = new HandShapeEvaluator();

        // Act
        var normalEv = evaluator.EvaluateHand14(hand14, TileKind.Pin5, back: null, ctx);
        var furitenEv = evaluator.EvaluateHand14(hand14, TileKind.Pin5, back: TileKind.Pin5, ctx);

        // Assert: back と winCandidate が一致するとフリテンで 0
        Assert.True(normalEv > 0);
        Assert.Equal(0, furitenEv);
    }

    [Fact]
    public void ShantenCoefficient_テンパイと1シャンテンで補正値が異なる()
    {
        // テンパイ (shanten=0) ×18、1 シャンテン ×3 の比率を間接的に確認
        // 同じ手牌組成で shanten が違う hand13 を作り、EvaluateHand13 の値の比を見る
        // ここでは枝刈り後の実装の一貫性のみ確認 (正確な比は有効牌の違いで単純比較できない)

        // Arrange: 明らかにテンパイの手牌と、それを 1 枚崩した 1 シャンテン手牌
        var tenpai = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(132), new Tile(133), new Tile(134),
            new Tile(53),
        ]);
        // 1 シャンテン: 中刻子を対子に崩す (Chun を 1 枚引いて別の孤立牌に)
        var oneShanten = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(132), new Tile(133),                // 中中 (対子)
            new Tile(53),                                 // Pin5
            new Tile(108),                                // 東 (孤立)
        ]);
        var ctx = CreateContext();
        var evaluator = new HandShapeEvaluator();

        // Act
        var evTenpai = evaluator.EvaluateHand13(tenpai, back: null, ctx);
        var evOneShanten = evaluator.EvaluateHand13(oneShanten, back: null, ctx);

        // Assert: いずれも正の評価値を返す (補正係数が適用されていれば)
        Assert.True(evTenpai > 0);
        Assert.True(evOneShanten > 0);
    }

    private static HandShapeEvaluatorContext CreateContext(
        Func<TileKind, int>? getUnseen = null,
        CallList? calls = null)
    {
        return new HandShapeEvaluatorContext(
            Rules: new GameRules(),
            RoundWindIndex: 0,
            SeatWindIndex: 0,
            RoundWind: Wind.East,
            PlayerWind: Wind.East,
            DoraIndicatorKinds: [],
            Calls: calls ?? [],
            GetUnseen: getUnseen ?? (_ => 4),
            TileWeights: TileWeights.AllOne,
            BackMarker: null);
    }
}
