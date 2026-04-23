using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Players.Impl;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.HandCalculating;
using System.Collections.Immutable;
using GameRules = Mahjong.Lib.Game.Games.GameRules;
using TileKind = Mahjong.Lib.Scoring.Tiles.TileKind;

namespace Mahjong.Lib.Game.Tests.Players;

public class HandShapeEvaluator_CalcHandScoreTests
{
    [Fact]
    public void メンゼン手_立直ツモの打点を返す()
    {
        // Arrange: 一気通貫 + 役牌(中) の 14 枚手牌 (メンゼン、Pin5 ツモ和了)
        //   Man1-9 三面子 + Chun×3 刻子 + Pin5×2 (頭、和了牌含む)
        var hand14 = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),       // 一万 二万 三万
            new Tile(12), new Tile(17), new Tile(20),    // 四万 五万 六万
            new Tile(24), new Tile(28), new Tile(32),    // 七万 八万 九万
            new Tile(132), new Tile(133), new Tile(134), // 中 中 中
            new Tile(53), new Tile(54),                  // 五筒 五筒 (赤でない)
        ]);
        var winTile = TileKind.Pin5;
        var rules = new GameRules();
        var ctx = CreateContext(rules);
        var evaluator = new HandShapeEvaluator();

        // Act
        var handScore = evaluator.CalcHandScore(hand14, winTile, ctx);

        // Assert: メンゼンなので立直付与、ツモ付与、一気通貫 + 中 + リーチ + 門前清自摸和 が成立
        // HandCalculator 直接呼出の結果と一致
        var expected = CalcExpected(hand14, winTile, rules, [], isRiichi: true, akadora: 0);
        Assert.Equal(expected, handScore);
        Assert.True(handScore > 0);
    }

    [Fact]
    public void 非メンゼン手_立直は付与されない()
    {
        // Arrange: 同じ和了形を非メンゼン (ポン済み) で扱う
        // Man1-9 + Chun Chun (対子、和了) + Pin5×2 の配牌に、Pin5 を含むポンが別途成立している想定
        // 実際には Pin5 のポンを持つと頭にできないため、Chun のポンで非メンゼン化する
        //   手牌: Man1-9 (3 面子) + Pin5×2 (頭) + Chun Chun (あと 1 枚引けば和了、ただしここは刻子として副露)
        // シンプルにするため: Calls に Chun ポンを載せ、手牌からは Chun×3 を除外。和了形は Man1-9 + Pin5×2 + Chun(ポン) となり 11 枚手牌 + 副露
        // HandCalculator は 「手牌 11 枚 + ポン 1 面子」で面子分解できる必要あり → 一気通貫 (門前限定ではない) が成立
        var hand11 = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(53), new Tile(54),
        ]);
        var winTile = TileKind.Pin5;
        var chunPon = new Call(
            CallType.Pon,
            [new Tile(132), new Tile(133), new Tile(134)],
            new PlayerIndex(1),
            new Tile(133));
        var calls = new CallList([chunPon]);
        var rules = new GameRules();
        var ctx = CreateContext(rules, calls);
        var evaluator = new HandShapeEvaluator();

        // Act
        var handScore = evaluator.CalcHandScore(hand11, winTile, ctx);

        // Assert: 非メンゼンなので立直なし。中 (役牌) + ツモは立直不可なので無し → 中だけの 1 翻? いや、
        // ツモは門前清自摸和なので非メンゼンでは付かない。中のみ + 食い下がった一気通貫 (門前限定なのでなし)
        // → 中のみ 1 翻
        var expected = CalcExpected(hand11, winTile, rules, calls, isRiichi: false, akadora: 0);
        Assert.Equal(expected, handScore);
        Assert.True(handScore > 0);
    }

    [Fact]
    public void 暗槓のみの手_メンゼン扱いで立直が付与される()
    {
        // Arrange: 暗槓 1 つ含む手。一気通貫は門前なので成立、立直も付与される
        // 手牌: Man1-9 (9 枚) + Pin5×2 (2 枚、和了対子) = 11 枚、暗槓 (中×4)
        var hand11 = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(53), new Tile(54),
        ]);
        var winTile = TileKind.Pin5;
        var chunAnkan = new Call(
            CallType.Ankan,
            [new Tile(132), new Tile(133), new Tile(134), new Tile(135)],
            new PlayerIndex(0),
            null);
        var calls = new CallList([chunAnkan]);
        var rules = new GameRules();
        var ctx = CreateContext(rules, calls);
        var evaluator = new HandShapeEvaluator();

        // Act
        var handScore = evaluator.CalcHandScore(hand11, winTile, ctx);

        // Assert: 暗槓のみなのでメンゼン。立直付与される
        var expected = CalcExpected(hand11, winTile, rules, calls, isRiichi: true, akadora: 0);
        Assert.Equal(expected, handScore);
        Assert.True(handScore > 0);
    }

    [Fact]
    public void 赤ドラを含む手_AkadoraCountが反映される()
    {
        // Arrange: 赤 Pin5 を含む手牌
        var hand14 = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(132), new Tile(133), new Tile(134),
            new Tile(52),                                 // 赤五筒
            new Tile(54),                                 // 通常五筒
        ]);
        var winTile = TileKind.Pin5;
        var rules = new GameRules();
        var ctx = CreateContext(rules);
        var evaluator = new HandShapeEvaluator();

        // Act
        var handScoreWithAkadora = evaluator.CalcHandScore(hand14, winTile, ctx);

        // 赤ドラなしの手牌と比較
        var hand14NoRed = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(132), new Tile(133), new Tile(134),
            new Tile(53),                                 // 通常五筒
            new Tile(54),                                 // 通常五筒
        ]);
        var evaluator2 = new HandShapeEvaluator();
        var handScoreNoAkadora = evaluator2.CalcHandScore(hand14NoRed, winTile, ctx);

        // Assert: 赤ドラ 1 枚ぶん打点が高くなる
        Assert.True(handScoreWithAkadora > handScoreNoAkadora);
    }

    [Fact]
    public void 同一手牌を2回計算_キャッシュから同じ値が返る()
    {
        // Arrange
        var hand14 = new Hand(
        [
            new Tile(0), new Tile(4), new Tile(8),
            new Tile(12), new Tile(17), new Tile(20),
            new Tile(24), new Tile(28), new Tile(32),
            new Tile(132), new Tile(133), new Tile(134),
            new Tile(53), new Tile(54),
        ]);
        var winTile = TileKind.Pin5;
        var ctx = CreateContext(new GameRules());
        var evaluator = new HandShapeEvaluator();

        // Act
        var first = evaluator.CalcHandScore(hand14, winTile, ctx);
        var second = evaluator.CalcHandScore(hand14, winTile, ctx);

        // Assert
        Assert.Equal(first, second);
    }

    private static HandShapeEvaluatorContext CreateContext(
        GameRules rules,
        CallList? calls = null,
        IEnumerable<TileKind>? doraIndicators = null)
    {
        calls ??= [];
        var doraArray = doraIndicators is null
            ? []
            : ImmutableArray.CreateRange(doraIndicators);
        return new HandShapeEvaluatorContext(
            Rules: rules,
            RoundWindIndex: 0,
            SeatWindIndex: 0,
            RoundWind: Wind.East,
            PlayerWind: Wind.East,
            DoraIndicatorKinds: doraArray,
            Calls: calls,
            GetUnseen: _ => 4,
            TileWeights: TileWeights.AllOne,
            BackMarker: null);
    }

    private static int CalcExpected(
        Hand hand,
        TileKind winTile,
        GameRules rules,
        CallList calls,
        bool isRiichi,
        int akadora)
    {
        var result = HandCalculator.Calc(
            tileKindList: ScoringConversions.ToScoringTileKindList(hand),
            winTile: winTile,
            callList: ScoringConversions.ToScoringCallList(calls),
            doraIndicators: [],
            uradoraIndicators: null,
            winSituation: new WinSituation
            {
                IsTsumo = true,
                IsRiichi = isRiichi,
                PlayerWind = Wind.East,
                RoundWind = Wind.East,
                AkadoraCount = akadora,
            },
            gameRules: ScoringConversions.ToScoringGameRules(rules));
        return result.ErrorMessage is not null
            ? 0
            : result.Score.Main + result.Score.Sub * 2;
    }
}
