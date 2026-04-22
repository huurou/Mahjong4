using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Rounds.Managing;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds.Managing;

public class ResponseValidator_ValidateSemanticTests
{
    private static Round CreateHaipaiRound()
    {
        return RoundTestHelper.CreateRound().Haipai();
    }

    /// <summary>
    /// 親が <paramref name="dahaiTile"/> を確実に打牌できるように親手牌を差し替えてから Dahai する。
    /// 親手牌は <paramref name="dahaiTile"/> + 13 枚のダミー (字牌で埋める) で構成する
    /// </summary>
    private static Round SetupDealerDahai(Round round, Tile dahaiTile)
    {
        var dealerHand = new List<Tile> { dahaiTile };
        var fillerIds = new[] { 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120 };
        foreach (var id in fillerIds)
        {
            if (id != dahaiTile.Id)
            {
                dealerHand.Add(new Tile(id));
            }
        }
        while (dealerHand.Count < 14)
        {
            dealerHand.Add(new Tile(dealerHand.Count * 3 + 21));
        }
        round = RoundTestHelper.InjectHand(round, round.Turn, dealerHand.Take(14));
        return round.Dahai(dahaiTile);
    }

    [Fact]
    public void OkResponse_常に有効()
    {
        // Arrange
        var round = CreateHaipaiRound();

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new OkResponse(), round, new PlayerIndex(0), RoundInquiryPhase.Haipai);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void DahaiResponse_手牌にある牌_有効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        var tile = round.HandArray[round.Turn].First();

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new DahaiResponse(tile), round, round.Turn, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void DahaiResponse_手牌にない牌_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        var handSet = round.HandArray[round.Turn].Select(x => x.Id).ToHashSet();
        var absentTile = new Tile(Enumerable.Range(0, 136).First(x => !handSet.Contains(x)));

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new DahaiResponse(absentTile), round, round.Turn, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("手牌", result.Reason!);
    }

    [Fact]
    public void DahaiResponse_立直宣言_門前でない_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        var tile = round.HandArray[round.Turn].First();
        round = round with
        {
            PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(round.Turn, round.PlayerRoundStatusArray[round.Turn] with { IsMenzen = false }),
        };

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new DahaiResponse(tile, IsRiichi: true), round, round.Turn, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("門前", result.Reason!);
    }

    [Fact]
    public void DahaiResponse_立直宣言_点数不足_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        var tile = round.HandArray[round.Turn].First();
        round = round with
        {
            PointArray = new PointArray(new Point(25000))
                .AddPoint(round.Turn, -24500), // 親を 500 点に
        };
        // Act
        var result = ResponseValidator.ValidateSemantic(
            new DahaiResponse(tile, IsRiichi: true), round, round.Turn, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("1000", result.Reason!);
    }

    [Fact]
    public void DahaiResponse_立直宣言_テンパイでない_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        var tile = round.HandArray[round.Turn].First();

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new DahaiResponse(tile, IsRiichi: true), round, round.Turn, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("テンパイ", result.Reason!);
    }

    [Fact]
    public void AnkanResponse_同種4枚あり_有効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        // 手牌を同種 4 枚含む構成に差し替え
        var fourKind = Enumerable.Range(0, 4).Select(x => new Tile(x)).ToList();
        var hand = new List<Tile>(fourKind);
        hand.AddRange(round.HandArray[round.Turn].Where(x => x.Kind != Scoring.Tiles.TileKind.Man1).Take(10));
        round = RoundTestHelper.InjectHand(round, round.Turn, hand);

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new AnkanResponse(new Tile(0)), round, round.Turn, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void AnkanResponse_同種3枚のみ_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        // 手牌: kind 0 を 3 枚だけ
        var hand = new List<Tile> { new(0), new(1), new(2) };
        hand.AddRange(round.HandArray[round.Turn].Where(x => x.Kind != Scoring.Tiles.TileKind.Man1).Take(11));
        round = RoundTestHelper.InjectHand(round, round.Turn, hand);

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new AnkanResponse(new Tile(0)), round, round.Turn, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void RonResponse_永久フリテン_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        round = round with
        {
            PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(
                new PlayerIndex(1),
                round.PlayerRoundStatusArray[new PlayerIndex(1)] with { IsFuriten = true }
            ),
        };

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new RonResponse(), round, new PlayerIndex(1), RoundInquiryPhase.Dahai);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("フリテン", result.Reason!);
    }

    [Fact]
    public void RonResponse_同巡フリテン_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        round = round with
        {
            PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(
                new PlayerIndex(1),
                round.PlayerRoundStatusArray[new PlayerIndex(1)] with { IsTemporaryFuriten = true }
            ),
        };

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new RonResponse(), round, new PlayerIndex(1), RoundInquiryPhase.Dahai);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void RonResponse_フリテンでない_有効()
    {
        // Arrange
        var round = CreateHaipaiRound();

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new RonResponse(), round, new PlayerIndex(1), RoundInquiryPhase.Dahai);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void TsumoAgariResponse_手牌14枚_有効()
    {
        // Arrange
        var round = CreateHaipaiRound().Tsumo(); // 親が 1 巡目ツモして 14 枚

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new TsumoAgariResponse(), round, round.Turn, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void TsumoAgariResponse_手牌13枚_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();

        // Act (子 index 1 は 13 枚のまま)
        var result = ResponseValidator.ValidateSemantic(
            new TsumoAgariResponse(), round, new PlayerIndex(1), RoundInquiryPhase.Tsumo);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void KyuushuKyuuhaiResponse_第一打前でない_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        round = round with
        {
            PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(
                round.Turn,
                round.PlayerRoundStatusArray[round.Turn] with { IsFirstTurnBeforeDiscard = false }
            ),
        };

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new KyuushuKyuuhaiResponse(), round, round.Turn, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void KyuushuKyuuhaiResponse_幺九牌9種未満_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        // 手牌を幺九 3 種だけにする (残りは中張牌)
        var hand = new List<Tile>
        {
            new(0), new(32), new(108),   // m1, m9, 東
            new(20), new(24), new(28), new(40), new(44), new(48), new(56), new(60), new(64), new(72), new(80),
        };
        round = RoundTestHelper.InjectHand(round, round.Turn, hand);

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new KyuushuKyuuhaiResponse(), round, round.Turn, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ChiResponse_手牌2枚と直前打牌で連続3牌_有効()
    {
        // Arrange: 親が m3 (kind=2) を打ったあと、下家がm1/m2でチー
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        var callerHand = new List<Tile>
        {
            new(0), new(4),  // m1, m2
            new(36), new(40), new(44), new(48), new(52), new(56), new(60), new(64), new(68), new(72), new(76),
        };
        round = RoundTestHelper.InjectHand(round, callerIndex, callerHand);
        round = SetupDealerDahai(round, new Tile(8)); // m3 を親が打牌

        // Act
        var handTiles = ImmutableArray.Create(new Tile(0), new Tile(4));
        var result = ResponseValidator.ValidateSemantic(
            new ChiResponse(handTiles), round, callerIndex, RoundInquiryPhase.Dahai);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ChiResponse_手牌使用枚数が2枚でない_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        round = SetupDealerDahai(round, new Tile(8));

        // Act (1 枚のみ)
        var handTiles = ImmutableArray.Create(new Tile(0));
        var result = ResponseValidator.ValidateSemantic(
            new ChiResponse(handTiles), round, callerIndex, RoundInquiryPhase.Dahai);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("2 枚", result.Reason!);
    }

    [Fact]
    public void ChiResponse_手牌に存在しない牌_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        var callerHand = new List<Tile>
        {
            new(0), new(4),
            new(36), new(40), new(44), new(48), new(52), new(56), new(60), new(64), new(68), new(72), new(76),
        };
        round = RoundTestHelper.InjectHand(round, callerIndex, callerHand);
        round = SetupDealerDahai(round, new Tile(8));

        // Act (callerHand に無い m2 の別コピー Id=5 を指定)
        var handTiles = ImmutableArray.Create(new Tile(0), new Tile(5));
        var result = ResponseValidator.ValidateSemantic(
            new ChiResponse(handTiles), round, callerIndex, RoundInquiryPhase.Dahai);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("手牌", result.Reason!);
    }

    [Fact]
    public void ChiResponse_字牌を含む_無効()
    {
        // Arrange: 字牌で順子を組もうとする
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        var callerHand = new List<Tile>
        {
            new(108), new(112),  // 東, 南
            new(36), new(40), new(44), new(48), new(52), new(56), new(60), new(64), new(68), new(72), new(76),
        };
        round = RoundTestHelper.InjectHand(round, callerIndex, callerHand);
        round = SetupDealerDahai(round, new Tile(116)); // 西を親が打牌

        // Act
        var handTiles = ImmutableArray.Create(new Tile(108), new Tile(112));
        var result = ResponseValidator.ValidateSemantic(
            new ChiResponse(handTiles), round, callerIndex, RoundInquiryPhase.Dahai);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("数牌", result.Reason!);
    }

    [Fact]
    public void ChiResponse_異スート_無効()
    {
        // Arrange: m1 + p1 + s1 は同一スートでない
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        var callerHand = new List<Tile>
        {
            new(0), new(36),  // m1, p1
            new(72), new(40), new(44), new(48), new(52), new(56), new(60), new(64), new(68), new(76), new(80),
        };
        round = RoundTestHelper.InjectHand(round, callerIndex, callerHand);
        round = SetupDealerDahai(round, new Tile(72)); // s1

        // Act
        var handTiles = ImmutableArray.Create(new Tile(0), new Tile(36));
        var result = ResponseValidator.ValidateSemantic(
            new ChiResponse(handTiles), round, callerIndex, RoundInquiryPhase.Dahai);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ChiResponse_非連続3牌_無効()
    {
        // Arrange: m1 + m2 + m4 は連続していない
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        var callerHand = new List<Tile>
        {
            new(0), new(4),  // m1, m2
            new(40), new(44), new(48), new(52), new(56), new(60), new(64), new(68), new(72), new(76), new(80),
        };
        round = RoundTestHelper.InjectHand(round, callerIndex, callerHand);
        round = SetupDealerDahai(round, new Tile(12)); // m4

        // Act
        var handTiles = ImmutableArray.Create(new Tile(0), new Tile(4));
        var result = ResponseValidator.ValidateSemantic(
            new ChiResponse(handTiles), round, callerIndex, RoundInquiryPhase.Dahai);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("連続", result.Reason!);
    }

    [Fact]
    public void PonResponse_手牌2枚と直前打牌が同種_有効()
    {
        // Arrange: 親がm1を打ったあと、下家がm1×2でポン
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        var callerHand = new List<Tile>
        {
            new(1), new(2),  // m1×2 (Id違い)
            new(36), new(40), new(44), new(48), new(52), new(56), new(60), new(64), new(68), new(72), new(76),
        };
        round = RoundTestHelper.InjectHand(round, callerIndex, callerHand);
        round = SetupDealerDahai(round, new Tile(0)); // m1

        // Act
        var handTiles = ImmutableArray.Create(new Tile(1), new Tile(2));
        var result = ResponseValidator.ValidateSemantic(
            new PonResponse(handTiles), round, callerIndex, RoundInquiryPhase.Dahai);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void PonResponse_手牌使用枚数が2枚でない_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        round = SetupDealerDahai(round, new Tile(0));

        // Act (3 枚指定)
        var handTiles = ImmutableArray.Create(new Tile(1), new Tile(2), new Tile(3));
        var result = ResponseValidator.ValidateSemantic(
            new PonResponse(handTiles), round, callerIndex, RoundInquiryPhase.Dahai);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("2 枚", result.Reason!);
    }

    [Fact]
    public void PonResponse_直前打牌と異種_無効()
    {
        // Arrange: 親はm2を打ったが、下家がm1×2でポンしようとする
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        var callerHand = new List<Tile>
        {
            new(1), new(2),
            new(36), new(40), new(44), new(48), new(52), new(56), new(60), new(64), new(68), new(72), new(76),
        };
        round = RoundTestHelper.InjectHand(round, callerIndex, callerHand);
        round = SetupDealerDahai(round, new Tile(4)); // m2 を親が打牌

        // Act
        var handTiles = ImmutableArray.Create(new Tile(1), new Tile(2));
        var result = ResponseValidator.ValidateSemantic(
            new PonResponse(handTiles), round, callerIndex, RoundInquiryPhase.Dahai);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("同種", result.Reason!);
    }

    [Fact]
    public void DaiminkanResponse_手牌3枚と直前打牌が同種_有効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        var callerHand = new List<Tile>
        {
            new(1), new(2), new(3),  // m1×3
            new(36), new(40), new(44), new(48), new(52), new(56), new(60), new(64), new(68), new(72),
        };
        round = RoundTestHelper.InjectHand(round, callerIndex, callerHand);
        round = SetupDealerDahai(round, new Tile(0));

        // Act
        var handTiles = ImmutableArray.Create(new Tile(1), new Tile(2), new Tile(3));
        var result = ResponseValidator.ValidateSemantic(
            new DaiminkanResponse(handTiles), round, callerIndex, RoundInquiryPhase.Dahai);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void DaiminkanResponse_手牌使用枚数が3枚でない_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        round = SetupDealerDahai(round, new Tile(0));

        // Act (2 枚指定)
        var handTiles = ImmutableArray.Create(new Tile(1), new Tile(2));
        var result = ResponseValidator.ValidateSemantic(
            new DaiminkanResponse(handTiles), round, callerIndex, RoundInquiryPhase.Dahai);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("3 枚", result.Reason!);
    }

    [Fact]
    public void KakanResponse_対応するポン副露あり_有効()
    {
        // Arrange: 既にm1ポンがあり、手牌にm1の追加牌を持つ
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        var callerHand = new List<Tile>
        {
            new(1), new(2),  // ポン用 m1×2
            new(3),          // 加槓用 m1 の 4 枚目
            new(36), new(40), new(44), new(48), new(52), new(56), new(60), new(64), new(68), new(72),
        };
        round = RoundTestHelper.InjectHand(round, callerIndex, callerHand);
        round = SetupDealerDahai(round, new Tile(0)); // 親が m1 を打牌
        // 下家 m1×2 をポン
        round = round.Pon(callerIndex, [new Tile(1), new Tile(2)]);

        // Act: 加槓で m1 (Id=3) を手牌から出す
        var result = ResponseValidator.ValidateSemantic(
            new KakanResponse(new Tile(3)), round, callerIndex, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void KakanResponse_対応するポン副露なし_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        var hand = new List<Tile>
        {
            new(0), new(36), new(40), new(44), new(48), new(52), new(56), new(60), new(64), new(68), new(72), new(76), new(80), new(84),
        };
        round = RoundTestHelper.InjectHand(round, round.Turn, hand);

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new KakanResponse(new Tile(0)), round, round.Turn, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("ポン", result.Reason!);
    }

    [Fact]
    public void KakanResponse_手牌に加槓牌が存在しない_無効()
    {
        // Arrange: ポン副露は m1 だが手牌に m1 が無い
        var round = CreateHaipaiRound();
        var callerIndex = round.Turn.Next();
        var callerHand = new List<Tile>
        {
            new(1), new(2),
            new(36), new(40), new(44), new(48), new(52), new(56), new(60), new(64), new(68), new(72), new(76),
        };
        round = RoundTestHelper.InjectHand(round, callerIndex, callerHand);
        round = SetupDealerDahai(round, new Tile(0));
        round = round.Pon(callerIndex, [new Tile(1), new Tile(2)]);

        // Act: 手牌に無い m1 (Id=3) を指定
        var result = ResponseValidator.ValidateSemantic(
            new KakanResponse(new Tile(3)), round, callerIndex, RoundInquiryPhase.Tsumo);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("手牌", result.Reason!);
    }

    [Fact]
    public void ChankanRonResponse_フリテンでない_有効()
    {
        // Arrange
        var round = CreateHaipaiRound();

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new ChankanRonResponse(), round, new PlayerIndex(1), RoundInquiryPhase.AfterKanTsumo);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ChankanRonResponse_永久フリテン_無効()
    {
        // Arrange
        var round = CreateHaipaiRound();
        round = round with
        {
            PlayerRoundStatusArray = round.PlayerRoundStatusArray.SetStatus(
                new PlayerIndex(1),
                round.PlayerRoundStatusArray[new PlayerIndex(1)] with { IsFuriten = true }
            ),
        };

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new ChankanRonResponse(), round, new PlayerIndex(1), RoundInquiryPhase.AfterKanTsumo);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("フリテン", result.Reason!);
    }

    [Fact]
    public void RinshanTsumoResponse_手牌14枚相当_有効()
    {
        // Arrange: 親がツモ (14 枚)
        var round = CreateHaipaiRound().Tsumo();

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new RinshanTsumoResponse(), round, round.Turn, RoundInquiryPhase.AfterKanTsumo);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void RinshanTsumoResponse_手牌13枚_無効()
    {
        // Arrange: 親は Haipai だけで 14 枚持つため、子 index=1 の 13 枚をターゲットにする
        var round = CreateHaipaiRound();

        // Act
        var result = ResponseValidator.ValidateSemantic(
            new RinshanTsumoResponse(), round, new PlayerIndex(1), RoundInquiryPhase.AfterKanTsumo);

        // Assert
        Assert.False(result.IsValid);
    }
}
