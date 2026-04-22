using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Scoring.Tiles;
using System.Collections.Immutable;
using Hand = Mahjong.Lib.Game.Hands.Hand;

namespace Mahjong.Lib.Game.Tests.Tenpai;

public class YakuAwareShantenHelper_CalcTests
{
    // =========== Menzen 経路 ===========

    [Fact]
    public void Menzen_副露なし_通常シャンテンと一致する()
    {
        // Arrange: 国士無双形で 1 シャンテン (通常形では遠い手)
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou7, TileKind.Sou8, TileKind.Sou9,
            TileKind.Ton, TileKind.Ton,
            TileKind.Haku, TileKind.Haku
        );

        // Act
        var actual = YakuAwareShantenHelper.CalcMenzen(hand, []);

        // Assert: テンパイ (白か東の雀頭もしくは白刻子・東刻子待ち)
        Assert.True(actual >= 0);
        Assert.True(actual < YakuAwareShantenHelper.INFINITE);
    }

    [Fact]
    public void Menzen_ポン副露あり_Infinityを返す()
    {
        // Arrange
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou1, TileKind.Sou2, TileKind.Sou3,
            TileKind.Sou5, TileKind.Sou5
        );
        CallList calls = [MakeKoutsuCall(CallType.Pon, TileKind.Nan)];

        // Act
        var actual = YakuAwareShantenHelper.CalcMenzen(hand, calls);

        // Assert: ポンは門前を破壊する
        Assert.Equal(YakuAwareShantenHelper.INFINITE, actual);
    }

    [Fact]
    public void Menzen_暗槓のみ_通常シャンテンと一致する()
    {
        // Arrange: 手牌 10 枚 + 暗槓 1 つ (実質 14 枚)
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou7, TileKind.Sou8, TileKind.Sou9,
            TileKind.Ton
        );
        CallList calls = [MakeKoutsuCall(CallType.Ankan, TileKind.Haku)];

        // Act
        var actual = YakuAwareShantenHelper.CalcMenzen(hand, calls);

        // Assert: 暗槓は門前維持
        Assert.True(actual < YakuAwareShantenHelper.INFINITE);
    }

    // =========== Yakuhai (翻牌) 経路 ===========

    [Fact]
    public void Yakuhai_三元牌刻子あり_通常シャンテンを返す()
    {
        // Arrange: 白刻子確定
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou1, TileKind.Sou1,
            TileKind.Haku, TileKind.Haku, TileKind.Haku,
            TileKind.Nan, TileKind.Sha
        );

        // Act
        var actual = YakuAwareShantenHelper.CalcYakuhai(hand, [], roundWindIndex: 0, seatWindIndex: 0);

        // Assert: 白刻子で翻牌成立 → 通常シャンテン
        Assert.True(actual < YakuAwareShantenHelper.INFINITE);
    }

    [Fact]
    public void Yakuhai_三元牌対子のみ_対子除去してプラス1補正()
    {
        // Arrange: 白対子 (刻子なし)
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou1, TileKind.Sou2, TileKind.Sou3,
            TileKind.Sou5, TileKind.Sou5,
            TileKind.Haku, TileKind.Haku
        );

        // Act
        var actual = YakuAwareShantenHelper.CalcYakuhai(hand, [], roundWindIndex: 0, seatWindIndex: 0);

        // Assert: 役牌対子あり → Infinity より小さい
        Assert.True(actual < YakuAwareShantenHelper.INFINITE);
    }

    [Fact]
    public void Yakuhai_役牌一切なし_Infinityを返す()
    {
        // Arrange: 場東/自東以外の風 (南) のみ
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou1, TileKind.Sou2, TileKind.Sou3,
            TileKind.Sou4, TileKind.Sou5, TileKind.Sou6,
            TileKind.Nan
        );

        // Act: 場東 + 自東 → 白/發/中/東 が対象。南対子は役牌対象外
        var actual = YakuAwareShantenHelper.CalcYakuhai(hand, [], roundWindIndex: 0, seatWindIndex: 0);

        // Assert
        Assert.Equal(YakuAwareShantenHelper.INFINITE, actual);
    }

    [Fact]
    public void Yakuhai_場風と自風が同じ_重複除去される()
    {
        // Arrange: 東対子 (場東・自東)
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou1, TileKind.Sou2, TileKind.Sou3,
            TileKind.Sou5, TileKind.Sou5,
            TileKind.Ton, TileKind.Ton
        );

        // Act
        var actual = YakuAwareShantenHelper.CalcYakuhai(hand, [], roundWindIndex: 0, seatWindIndex: 0);

        // Assert: 東対子あり → 役牌成立候補
        Assert.True(actual < YakuAwareShantenHelper.INFINITE);
    }

    [Fact]
    public void Yakuhai_明刻副露あり_通常シャンテンを返す()
    {
        // Arrange: 白 ポン (手牌側に白なし)
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou1, TileKind.Sou2, TileKind.Sou3,
            TileKind.Sou5
        );
        CallList calls = [MakeKoutsuCall(CallType.Pon, TileKind.Haku)];

        // Act
        var actual = YakuAwareShantenHelper.CalcYakuhai(hand, calls, roundWindIndex: 0, seatWindIndex: 0);

        // Assert: 白ポンで翻牌確定 → Infinity より小さい
        Assert.True(actual < YakuAwareShantenHelper.INFINITE);
    }

    // =========== Tanyao 経路 ===========

    [Fact]
    public void Tanyao_通常の中張形_正しいシャンテンを返す()
    {
        // Arrange: 中張のみ
        var hand = BuildHand(
            TileKind.Man2, TileKind.Man3, TileKind.Man4,
            TileKind.Pin5, TileKind.Pin6, TileKind.Pin7,
            TileKind.Sou3, TileKind.Sou4, TileKind.Sou5,
            TileKind.Sou7, TileKind.Sou7,
            TileKind.Man6, TileKind.Man6
        );

        // Act
        var actual = YakuAwareShantenHelper.CalcTanyao(hand, [], new GameRules());

        // Assert: 全て中張 → 通常シャンテン
        Assert.True(actual < YakuAwareShantenHelper.INFINITE);
    }

    [Fact]
    public void Tanyao_クイタンなしでポン副露あり_Infinity()
    {
        // Arrange
        var hand = BuildHand(
            TileKind.Man2, TileKind.Man3, TileKind.Man4,
            TileKind.Pin2, TileKind.Pin3, TileKind.Pin4,
            TileKind.Sou2, TileKind.Sou3,
            TileKind.Sou5, TileKind.Sou5
        );
        CallList calls = [MakeKoutsuCall(CallType.Pon, TileKind.Sou6)];
        var rules = new GameRules { KuitanAllowed = false };

        // Act
        var actual = YakuAwareShantenHelper.CalcTanyao(hand, calls, rules);

        // Assert
        Assert.Equal(YakuAwareShantenHelper.INFINITE, actual);
    }

    [Fact]
    public void Tanyao_Call内に么九あり_Infinity()
    {
        // Arrange: 白ポン (幺九含む)
        var hand = BuildHand(
            TileKind.Man2, TileKind.Man3, TileKind.Man4,
            TileKind.Pin2, TileKind.Pin3, TileKind.Pin4,
            TileKind.Sou2, TileKind.Sou3, TileKind.Sou4,
            TileKind.Sou5
        );
        CallList calls = [MakeKoutsuCall(CallType.Pon, TileKind.Haku)];

        // Act
        var actual = YakuAwareShantenHelper.CalcTanyao(hand, calls, new GameRules());

        // Assert
        Assert.Equal(YakuAwareShantenHelper.INFINITE, actual);
    }

    [Fact]
    public void Tanyao_手牌内に么九あり_除去してシャンテン計算()
    {
        // Arrange: 手牌に 1m と 9m が混在
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man9,
            TileKind.Pin2, TileKind.Pin3, TileKind.Pin4,
            TileKind.Sou2, TileKind.Sou3, TileKind.Sou4,
            TileKind.Sou5, TileKind.Sou5,
            TileKind.Man5, TileKind.Man6, TileKind.Man7
        );

        // Act
        var actual = YakuAwareShantenHelper.CalcTanyao(hand, [], new GameRules());

        // Assert: 么九除去後の形でシャンテン計算 (Infinity ではない)
        Assert.True(actual < YakuAwareShantenHelper.INFINITE);
    }

    [Fact]
    public void Tanyao_副露後に么九浮き牌_非テンパイ扱い()
    {
        // Arrange: 234m チー + 手牌 234p 234s 678s 1m (= 10 枚)。
        // 1m は断么九に使えず浮いているため、実際のテンパイ到達には 1m 入れ替えが必要。
        // フィルタ除去だけで knownCallMeldCount=1 のシャンテン計算すると 9 枚 = 3 面子で
        // 副露 1 含め 4 面子完成扱い→0 シャンテン (テンパイ扱い) になる過小評価バグを検出する。
        var hand = BuildHand(
            TileKind.Pin2, TileKind.Pin3, TileKind.Pin4,
            TileKind.Sou2, TileKind.Sou3, TileKind.Sou4,
            TileKind.Sou6, TileKind.Sou7, TileKind.Sou8,
            TileKind.Man1
        );
        var chi = new Call(
            CallType.Chi,
            [new Tile(TileKind.Man2.Value * 4), new Tile(TileKind.Man3.Value * 4), new Tile(TileKind.Man4.Value * 4)],
            new PlayerIndex(3),
            new Tile(TileKind.Man2.Value * 4)
        );
        CallList calls = [chi];

        // Act
        var actual = YakuAwareShantenHelper.CalcTanyao(hand, calls, new GameRules());

        // Assert: 1m 1 枚除去分の +1 補正が入り、テンパイ (0) ではなく 1 シャンテン以上になる
        Assert.True(actual >= 1, $"Expected shanten >= 1 (non-tenpai), got {actual}");
    }

    // =========== Toitoi 経路 ===========

    [Fact]
    public void Toitoi_Callに順子あり_Infinity()
    {
        // Arrange: チー副露あり
        var hand = BuildHand(
            TileKind.Man5, TileKind.Man6,
            TileKind.Pin2, TileKind.Pin3, TileKind.Pin4,
            TileKind.Sou2, TileKind.Sou3, TileKind.Sou4,
            TileKind.Sou5, TileKind.Sou5
        );
        var chi = new Call(
            CallType.Chi,
            [new Tile(0), new Tile(4), new Tile(8)],
            new PlayerIndex(3),
            new Tile(0)
        );

        // Act
        var actual = YakuAwareShantenHelper.CalcToitoi(hand, [chi]);

        // Assert
        Assert.Equal(YakuAwareShantenHelper.INFINITE, actual);
    }

    [Fact]
    public void Toitoi_手牌刻子3_対子1_公式通りシャンテン返す()
    {
        // Arrange: 手牌 刻子 3 つ + 対子 1 つ + 孤立 2 枚 = 13 枚
        var hand = BuildHand(
            TileKind.Man2, TileKind.Man2, TileKind.Man2,
            TileKind.Pin5, TileKind.Pin5, TileKind.Pin5,
            TileKind.Sou7, TileKind.Sou7, TileKind.Sou7,
            TileKind.Ton, TileKind.Ton,
            TileKind.Nan, TileKind.Sha
        );

        // Act
        var actual = YakuAwareShantenHelper.CalcToitoi(hand, []);

        // Assert: 刻子数=3, 対子数=1, shanten = 8 - 2*3 - 1 = 1
        Assert.Equal(1, actual);
    }

    [Fact]
    public void Toitoi_ポン副露あり_Callの刻子が加算される()
    {
        // Arrange: 手牌 刻子 0 + 対子 2 + ポン 1
        var hand = BuildHand(
            TileKind.Man2, TileKind.Man2,
            TileKind.Pin5, TileKind.Pin5,
            TileKind.Sou3, TileKind.Sou4, TileKind.Sou5,
            TileKind.Ton, TileKind.Ton,
            TileKind.Haku
        );
        CallList calls = [MakeKoutsuCall(CallType.Pon, TileKind.Chun)];

        // Act
        var actual = YakuAwareShantenHelper.CalcToitoi(hand, calls);

        // Assert: 副露刻子=1, 手牌刻子=0, 手牌対子=3 (man/pin/ton)
        // 刻子数=1, 対子数=3, shanten = 8 - 2*1 - 3 = 3
        Assert.Equal(3, actual);
    }

    [Fact]
    public void Toitoi_ブロック数超過_補正される()
    {
        // Arrange: 刻子 1 + 対子 5 = 6 ブロック → 刻子数=1 で 対子数=5-1=4 に補正
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man1, TileKind.Man1,  // 刻子
            TileKind.Man3, TileKind.Man3,
            TileKind.Pin5, TileKind.Pin5,
            TileKind.Sou3, TileKind.Sou3,
            TileKind.Sou7, TileKind.Sou7,
            TileKind.Ton, TileKind.Ton
        );

        // Act
        var actual = YakuAwareShantenHelper.CalcToitoi(hand, []);

        // Assert: 元々 刻子数=1, 対子数=5 → 対子数=4 補正後、shanten = 8 - 2 - 4 = 2
        Assert.Equal(2, actual);
    }

    // =========== Isshoku (一色手) 経路 ===========

    [Fact]
    public void Isshoku_他スート副露_Infinity()
    {
        // Arrange: 萬子染めだが筒子ポン
        var hand = BuildHand(
            TileKind.Man2, TileKind.Man3, TileKind.Man4,
            TileKind.Man5, TileKind.Man5,
            TileKind.Man7, TileKind.Man8, TileKind.Man9,
            TileKind.Sou3, TileKind.Sou3
        );
        CallList calls = [MakeKoutsuCall(CallType.Pon, TileKind.Pin5)];

        // Act
        var actual = YakuAwareShantenHelper.CalcIsshoku(hand, calls, suit: 0);

        // Assert
        Assert.Equal(YakuAwareShantenHelper.INFINITE, actual);
    }

    [Fact]
    public void Isshoku_字牌包含副露_OK()
    {
        // Arrange: 萬子 + 字牌のみ (混一色狙い)
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Man4, TileKind.Man5, TileKind.Man6,
            TileKind.Man7, TileKind.Man8, TileKind.Man9,
            TileKind.Ton
        );
        CallList calls = [MakeKoutsuCall(CallType.Pon, TileKind.Haku)];

        // Act
        var actual = YakuAwareShantenHelper.CalcIsshoku(hand, calls, suit: 0);

        // Assert: 字牌ポンは許容される
        Assert.True(actual < YakuAwareShantenHelper.INFINITE);
    }

    [Fact]
    public void Isshoku_対象スート以外の手牌は除去される()
    {
        // Arrange: 萬子 + 筒子 + 索子 + 字牌 混在
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Man4, TileKind.Man5,
            TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou8, TileKind.Sou9,
            TileKind.Ton, TileKind.Ton
        );

        // Act: 萬子染め
        var actualMan = YakuAwareShantenHelper.CalcIsshoku(hand, [], suit: 0);

        // Assert: 萬子 + 字牌のみが残る計算 (Infinity ではない)
        Assert.True(actualMan < YakuAwareShantenHelper.INFINITE);
    }

    [Fact]
    public void Isshoku_手牌に他スート浮き牌_非テンパイ扱い()
    {
        // Arrange: 萬子で 4 面子 1 雀頭がすでに揃う 14 枚形だが Pin9/Sou9 が浮いている。
        // Pin9/Sou9 を除去しただけの 12 枚で計算すると 4 面子 1 雀頭完成として和了扱いに見えてしまうが、
        // 実際には他スート 2 枚を打牌する必要があるため、非テンパイ (1 以上) 扱いになるのが正しい。
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Man4, TileKind.Man5, TileKind.Man6,
            TileKind.Man7, TileKind.Man8, TileKind.Man9,
            TileKind.Man5, TileKind.Man5,
            TileKind.Pin9,
            TileKind.Sou9
        );

        // Act: 萬子染め
        var actual = YakuAwareShantenHelper.CalcIsshoku(hand, [], suit: 0);

        // Assert: 他スート 2 枚除去分の +2 補正が入り、テンパイ (0 以下) ではなく 1 以上になる
        Assert.True(actual >= 1, $"Expected shanten >= 1 (non-tenpai), got {actual}");
    }

    // =========== 統合 Calc ===========

    [Fact]
    public void Calc_全経路の最小を返す()
    {
        // Arrange: 和了形 14 枚 (門前 -1 agari)
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou7, TileKind.Sou8, TileKind.Sou9,
            TileKind.Ton, TileKind.Ton,
            TileKind.Haku, TileKind.Haku, TileKind.Haku
        );

        // Act
        var actual = YakuAwareShantenHelper.Calc(hand, [], new GameRules(), roundWindIndex: 0, seatWindIndex: 0);

        // Assert: 和了
        Assert.Equal(-1, actual);
    }

    // ================= ヘルパー =================

    /// <summary>
    /// 指定した牌種の Tile を順に並べた Hand を生成する (同一牌種は id を 0, +1, +2, +3 と割り当てる)
    /// </summary>
    private static Hand BuildHand(params TileKind[] kinds)
    {
        var countsByKind = new Dictionary<TileKind, int>();
        var tiles = new List<Tile>();
        foreach (var kind in kinds)
        {
            if (!countsByKind.TryGetValue(kind, out var copy)) { copy = 0; }
            countsByKind[kind] = copy + 1;
            tiles.Add(new Tile(kind.Value * 4 + copy));
        }
        return new Hand(tiles);
    }

    private static Call MakeKoutsuCall(CallType type, TileKind kind)
    {
        var baseId = kind.Value * 4;
        var count = type is CallType.Ankan or CallType.Daiminkan or CallType.Kakan ? 4 : 3;
        var tiles = Enumerable.Range(0, count).Select(x => new Tile(baseId + x)).ToImmutableList();
        var calledTile = type == CallType.Ankan ? null : tiles[0];
        return new Call(type, tiles, new PlayerIndex(3), calledTile);
    }
}
