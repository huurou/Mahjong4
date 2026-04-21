using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_EvaluateFuritenTests
{
    /// <summary>
    /// 指定プレイヤーの手牌を p5 単騎タンヤオテンパイ (13枚) に差し替える
    /// 待ち牌種は p5 (kind 13)
    /// </summary>
    private static Round InjectP5TankiTenpai(Round round, PlayerIndex index)
    {
        return RoundTestHelper.InjectHand(round, index,
        [
            new Tile(4), new Tile(8), new Tile(12),    // m234
            new Tile(16), new Tile(20), new Tile(24),  // m567
            new Tile(40), new Tile(44), new Tile(48),  // p234
            new Tile(76), new Tile(80), new Tile(84),  // s234
            new Tile(52),                               // p5 (雀頭候補、単騎待ち)
        ]);
    }

    [Fact]
    public void 待ち牌が河にある_IsFuritenがtrue()
    {
        // Arrange: 子(index 1) に p5 単騎テンパイを仕込み、河に p5 (Tile(53)) を積む
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = new PlayerIndex(1);
        round = InjectP5TankiTenpai(round, playerIndex);
        round = round with { RiverArray = round.RiverArray.AddTile(playerIndex, new Tile(53)) };

        // Act
        var result = round.EvaluateFuriten(playerIndex);

        // Assert: 河に待ち牌種 p5 があるためフリテン
        Assert.True(result.PlayerRoundStatusArray[playerIndex].IsFuriten);
    }

    [Fact]
    public void 待ち牌が河にない_IsFuritenはfalse()
    {
        // Arrange: p5 単騎テンパイで 河には p5 を含まない
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = new PlayerIndex(1);
        round = InjectP5TankiTenpai(round, playerIndex);
        // 河には m1 (待ちでない) を 1 枚だけ積む
        round = round with { RiverArray = round.RiverArray.AddTile(playerIndex, new Tile(0)) };

        // Act
        var result = round.EvaluateFuriten(playerIndex);

        // Assert
        Assert.False(result.PlayerRoundStatusArray[playerIndex].IsFuriten);
    }

    [Fact]
    public void 待ち牌が空_IsFuritenはfalse()
    {
        // Arrange: テンパイでない (シャンテン>=1) 手牌 → 待ち牌なし
        var round = RoundTestHelper.CreateRound().Haipai();
        var playerIndex = new PlayerIndex(1);
        // 七対子 2 シャンテン (4 対子 + 5 孤立) の 13 枚
        round = RoundTestHelper.InjectHand(round, playerIndex,
        [
            new Tile(0), new Tile(1),      // m1 m1
            new Tile(8), new Tile(9),      // m3 m3
            new Tile(36), new Tile(37),    // p1 p1
            new Tile(44), new Tile(45),    // p3 p3
            new Tile(16), new Tile(52),    // m5 p5 (孤立)
            new Tile(72), new Tile(80),    // s1 s3 (孤立)
            new Tile(108),                  // 東 (孤立)
        ]);
        round = round with { RiverArray = round.RiverArray.AddTile(playerIndex, new Tile(52)) };

        // Act
        var result = round.EvaluateFuriten(playerIndex);

        // Assert: 待ち牌集合が空のためフリテンではない
        Assert.False(result.PlayerRoundStatusArray[playerIndex].IsFuriten);
    }

    [Fact]
    public void 打牌時にフリテン評価が自動で行われる()
    {
        // Arrange: 親に p5 単騎テンパイを仕込む。親の打牌が p5 (Tile(53)) → Dahai 内で EvaluateFuriten(Turn) → 河に p5 がありフリテン成立
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var dealerIndex = round.Turn;
        // 手牌を差し替え: p5単騎テンパイ (13枚) + 追加の打牌用 Tile(53) (p5) = 14枚
        round = RoundTestHelper.InjectHand(round, dealerIndex,
        [
            new Tile(4), new Tile(8), new Tile(12),    // m234
            new Tile(16), new Tile(20), new Tile(24),  // m567
            new Tile(40), new Tile(44), new Tile(48),  // p234
            new Tile(76), new Tile(80), new Tile(84),  // s234
            new Tile(52),                               // p5 (雀頭候補、単騎待ち)
            new Tile(53),                               // p5 (打牌予定、14枚目)
        ]);
        var dahaiTile = new Tile(53);

        // Act: Dahai 内で EvaluateFuriten が呼ばれる
        var result = round.Dahai(dahaiTile);

        // Assert
        Assert.True(result.PlayerRoundStatusArray[dealerIndex].IsFuriten);
    }

    [Fact]
    public void 待ち牌が鳴かれた牌にある_IsFuritenがtrue()
    {
        // Arrange: 親の打牌 Tile(83) (kind 20 = s3) を子がチーで鳴く。
        // 親の打牌後の手牌は s3 カンチャン待ちテンパイ: m234 m567 p234 p55 s24 の 13 枚 (待ち={s3})
        var round = RoundTestHelper.CreateRound().Haipai().Tsumo();
        var dealerIndex = round.Turn;

        // 親手牌 14 枚 = s3 カンチャン待ちテンパイ(13) + 打牌用 s3(Tile83)
        round = RoundTestHelper.InjectHand(round, dealerIndex,
        [
            new Tile(4), new Tile(8), new Tile(12),    // m234
            new Tile(16), new Tile(20), new Tile(24),  // m567
            new Tile(40), new Tile(44), new Tile(48),  // p234
            new Tile(52), new Tile(53),                 // p5 p5 雀頭
            new Tile(76),                               // s2
            new Tile(84),                               // s4 (s3 カンチャン待ち)
            new Tile(83),                               // s3 (打牌予定、14枚目)
        ]);
        round = round.Dahai(new Tile(83));   // 親が s3 打牌 → 河に Tile(83)

        // 子がチー (s3 + s4 + s5 の順子)。チー後 Tile(83) は河から TilesCalledFromRiver に移動する
        var caller = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, caller,
        [
            new Tile(85), new Tile(88),                // s4 (caller用), s5
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(5), new Tile(6), new Tile(7),
            new Tile(11), new Tile(14), new Tile(15), new Tile(17),
        ]);
        round = round.Chi(caller, ImmutableList.Create(new Tile(85), new Tile(88)));

        // Act: 親の待ち kind 20 (s3) は TilesCalledFromRiver に記録されているためフリテン
        var result = round.EvaluateFuriten(dealerIndex);

        // Assert
        Assert.True(result.PlayerRoundStatusArray[dealerIndex].IsFuriten);
    }
}
