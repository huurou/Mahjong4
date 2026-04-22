using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_PaoRecordTests
{
    // 三元牌の牌種ID: 31=白, 32=發, 33=中
    // Tile.Id: 31*4=124-127 (白), 32*4=128-131 (發), 33*4=132-135 (中)

    private static Round SetupRoundWith2SangenPonsByP1(PlayerIndex from)
    {
        var round = RoundTestHelper.CreateRound(0).Haipai();
        var p1 = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, p1,
        [
            new Tile(124), new Tile(125), new Tile(126), // 白
            new Tile(128), new Tile(129), new Tile(130), // 發
            new Tile(132), new Tile(133), // 中2枚 (ポン前提)
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
        ]);
        var pon1 = new Call(
            CallType.Pon,
            [new Tile(124), new Tile(125), new Tile(127)],
            from,
            new Tile(127)
        );
        var pon2 = new Call(
            CallType.Pon,
            [new Tile(128), new Tile(129), new Tile(131)],
            from,
            new Tile(131)
        );
        round = round with
        {
            CallListArray = round.CallListArray.AddCall(p1, pon1).AddCall(p1, pon2),
        };
        return round;
    }

    [Fact]
    public void Pon_三元牌3種目でPaoResponsibleArrayに記録される()
    {
        // Arrange
        var from = new PlayerIndex(0);
        var p1 = new PlayerIndex(1);
        var round = SetupRoundWith2SangenPonsByP1(from);
        // P0 が中を河に捨てた状態を作る
        round = round with
        {
            Turn = from,
            RiverArray = round.RiverArray.AddTile(from, new Tile(135)),
        };

        // Act: P1 が P0 の中をポン
        var result = round.Pon(p1, [new Tile(132), new Tile(133)]);

        // Assert
        Assert.Equal(from, result.PaoResponsibleArray[p1]);
    }

    [Fact]
    public void Pon_三元牌2種目では記録されない()
    {
        // Arrange: ポン1つのみの状態から2つ目のポン
        var round = RoundTestHelper.CreateRound(0).Haipai();
        var p1 = new PlayerIndex(1);
        var from = new PlayerIndex(0);
        round = RoundTestHelper.InjectHand(round, p1,
        [
            new Tile(124), new Tile(125), new Tile(126),
            new Tile(128), new Tile(129), // 發2枚 ポン対象
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6),
        ]);
        var pon1 = new Call(
            CallType.Pon,
            [new Tile(124), new Tile(125), new Tile(127)],
            from,
            new Tile(127)
        );
        round = round with
        {
            CallListArray = round.CallListArray.AddCall(p1, pon1),
            Turn = from,
            RiverArray = round.RiverArray.AddTile(from, new Tile(131)),
        };

        // Act
        var result = round.Pon(p1, [new Tile(128), new Tile(129)]);

        // Assert
        Assert.Null(result.PaoResponsibleArray[p1]);
    }

    [Fact]
    public void Daiminkan_三元牌3種目でPaoResponsibleArrayに記録される()
    {
        // Arrange
        var from = new PlayerIndex(2);
        var p1 = new PlayerIndex(1);
        var round = SetupRoundWith2SangenPonsByP1(from);
        // P1 の手牌の中牌2枚を3枚に増やす
        round = RoundTestHelper.InjectHand(round, p1,
        [
            new Tile(124), new Tile(125), new Tile(126),
            new Tile(128), new Tile(129), new Tile(130),
            new Tile(132), new Tile(133), new Tile(134), // 中3枚
            new Tile(0), new Tile(1), new Tile(2),
        ]);
        round = round with
        {
            Turn = from,
            RiverArray = round.RiverArray.AddTile(from, new Tile(135)),
        };

        // Act
        var result = round.Daiminkan(p1, [new Tile(132), new Tile(133), new Tile(134)]);

        // Assert
        Assert.Equal(from, result.PaoResponsibleArray[p1]);
    }

    [Fact]
    public void Kakan_四槓子で4つ目の加槓_PaoResponsibleArrayに記録される()
    {
        // Arrange: P1 が既に 3 槓 (全て他家鳴き) 済み、4 つ目に加槓
        var round = RoundTestHelper.CreateRound(0).Haipai();
        var p1 = new PlayerIndex(1);
        var from = new PlayerIndex(0);

        // 手牌: 対応するポンの上に追加する牌 + 13枚調整
        round = RoundTestHelper.InjectHand(round, p1,
        [
            new Tile(86), // kind 21 加槓する牌
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
        ]);

        var kan1 = new Call(
            CallType.Daiminkan,
            [new Tile(40), new Tile(41), new Tile(42), new Tile(43)],
            from,
            new Tile(43)
        );
        var kan2 = new Call(
            CallType.Daiminkan,
            [new Tile(60), new Tile(61), new Tile(62), new Tile(63)],
            from,
            new Tile(63)
        );
        var kan3 = new Call(
            CallType.Daiminkan,
            [new Tile(80), new Tile(81), new Tile(82), new Tile(83)],
            from,
            new Tile(83)
        );
        var existingPon = new Call(
            CallType.Pon,
            [new Tile(84), new Tile(85), new Tile(87)],
            from,
            new Tile(87)
        );
        round = round with
        {
            CallListArray = round.CallListArray
                .AddCall(p1, kan1)
                .AddCall(p1, kan2)
                .AddCall(p1, kan3)
                .AddCall(p1, existingPon),
            Turn = p1,
        };

        // Act
        var result = round.Kakan(new Tile(86));

        // Assert
        Assert.Equal(from, result.PaoResponsibleArray[p1]);
    }

    [Fact]
    public void Ankan_三元牌3種目_責任者記録されない()
    {
        // Arrange: P1 が2ポン (白・發) 持ちで自手牌で中を暗槓する状況
        var round = RoundTestHelper.CreateRound(0).Haipai();
        var p1 = new PlayerIndex(1);
        var from = new PlayerIndex(0);
        round = RoundTestHelper.InjectHand(round, p1,
        [
            new Tile(132), new Tile(133), new Tile(134), new Tile(135), // 中4枚
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(8),
        ]);
        var pon1 = new Call(
            CallType.Pon,
            [new Tile(124), new Tile(125), new Tile(127)],
            from,
            new Tile(127)
        );
        var pon2 = new Call(
            CallType.Pon,
            [new Tile(128), new Tile(129), new Tile(131)],
            from,
            new Tile(131)
        );
        round = round with
        {
            CallListArray = round.CallListArray.AddCall(p1, pon1).AddCall(p1, pon2),
            Turn = p1,
        };

        // Act
        var result = round.Ankan(new Tile(132));

        // Assert: 暗槓では責任者なし
        Assert.Null(result.PaoResponsibleArray[p1]);
    }
}
