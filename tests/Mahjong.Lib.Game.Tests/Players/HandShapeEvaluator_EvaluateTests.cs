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

public class HandShapeEvaluator_EvaluateTests
{
    [Fact]
    public void EvaluateHand14_和了形_CalcHandScoreと同じ値を返す()
    {
        // Arrange: Man1-9 + Chun×3 + Pin5×2 の和了形 14 枚
        var hand14 = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(132), new Tile(133), new Tile(134),
            new Tile(53), new Tile(54),
        ]);
        var winTile = TileKind.Pin5;
        var ctx = CreateContext();
        var evaluator = new HandShapeEvaluator();

        // Act
        var ev = evaluator.EvaluateHand14(hand14, winTile, back: null, ctx);
        var handScore = evaluator.CalcHandScore(hand14, winTile, ctx);

        // Assert
        Assert.Equal(handScore, ev);
        Assert.True(ev > 0);
    }

    [Fact]
    public void EvaluateHand13_テンパイ手_正の評価値を返す()
    {
        // Arrange: 同じ和了形から winTile を抜いたテンパイ 13 枚 (単騎・Pin5 待ち)
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

        // Act: Pin5 を 4 枚未見として評価 (GetUnseen が全 Kind で 4 を返す)
        var ev = evaluator.EvaluateHand13(hand13, back: null, ctx);

        // Assert: 有効牌 Pin5 × 未見 4 × 和了打点 で正の値になる
        Assert.True(ev > 0);
    }

    [Fact]
    public void EvaluateHand13_3シャンテン以上_0を返す()
    {
        // Arrange: 完全に孤立した 13 枚 (3 シャンテン超え)
        var hand13 = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(12), new Tile(28),    // 1m 2m 4m 8m
            new Tile(36), new Tile(48), new Tile(60),                // 1p 4p 7p
            new Tile(72), new Tile(88), new Tile(100),               // 1s 5s 8s
            new Tile(108), new Tile(116), new Tile(124),             // 東 西 白
        ]);
        var ctx = CreateContext();
        var evaluator = new HandShapeEvaluator();

        // Act
        var ev = evaluator.EvaluateHand13(hand13, back: null, ctx);

        // Assert: 3 シャンテン以上のため 0 (フォールバックは Phase F で接続)
        Assert.Equal(0, ev);
    }

    [Fact]
    public void EvaluateHand13_同一手牌の2回呼出_キャッシュから同じ値が返る()
    {
        // Arrange
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

        // Act
        var first = evaluator.EvaluateHand13(hand13, back: null, ctx);
        var second = evaluator.EvaluateHand13(hand13, back: null, ctx);

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void EvaluateHand13_赤5と通常5_異なる評価値を返す()
    {
        // Arrange: テンパイ手牌 (Pin5 単騎) を 2 種類 — 一方は赤 Pin5、一方は通常 Pin5。
        // HandSignature / CallsSignature は赤黒を区別しないため、キャッシュキーに akadora を
        // 入れていないと両者が同じ evalCache エントリに衝突する。
        var hand13Red = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(132), new Tile(133), new Tile(134),
            new Tile(52),                                 // 赤 Pin5
        ]);
        var hand13Normal = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(132), new Tile(133), new Tile(134),
            new Tile(53),                                 // 通常 Pin5
        ]);
        var ctx = CreateContext();
        var evaluator = new HandShapeEvaluator();

        // Act: 同じ evaluator インスタンスで両方評価する (キャッシュが効く状態)
        var evRed = evaluator.EvaluateHand13(hand13Red, back: null, ctx);
        var evNormal = evaluator.EvaluateHand13(hand13Normal, back: null, ctx);

        // Assert: 赤ドラ 1 枚分の打点差が評価値に反映される
        Assert.True(evRed > 0);
        Assert.True(evNormal > 0);
        Assert.True(evRed > evNormal);
    }

    [Fact]
    public void ClearAll_キャッシュ後にクリア_再計算が行われる()
    {
        // Arrange: ctx1 は全未見 4、ctx2 は全未見 0 (どの牌も引けない)
        var hand13 = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(132), new Tile(133), new Tile(134),
            new Tile(53),
        ]);
        var ctx1 = CreateContext();                            // 未見 4
        var ctx2 = CreateContext(getUnseen: _ => 0);           // 未見 0
        var evaluator = new HandShapeEvaluator();

        // Act: ctx1 で評価 → キャッシュに保存
        var ev1 = evaluator.EvaluateHand13(hand13, back: null, ctx1);
        // ClearAll で未見違いに対応
        evaluator.ClearAll();
        var ev2 = evaluator.EvaluateHand13(hand13, back: null, ctx2);

        // Assert
        Assert.True(ev1 > 0);
        Assert.Equal(0, ev2);
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
