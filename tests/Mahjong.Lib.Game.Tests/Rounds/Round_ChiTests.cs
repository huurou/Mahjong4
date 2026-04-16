using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class Round_ChiTests
{
    [Fact]
    public void Chi_河の最後が除かれて副露が追加されTurnが変わる()
    {
        // Arrange
        // P0 が Tile(83) (kind 20 = 3索) を打牌 → P1 が手牌 Tile(84),Tile(88) と合わせて 3-4-5索 の順子を作る
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        var discarded = new Tile(83);
        round = round.Dahai(discarded, RoundTestHelper.NoOpTenpaiChecker);
        var caller = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, caller,
        [
            new Tile(84), new Tile(88),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16),
        ]);
        var handTiles = ImmutableList.Create(new Tile(84), new Tile(88));

        // Act
        var result = round.Chi(caller, handTiles);

        // Assert
        Assert.DoesNotContain(discarded, result.RiverArray[round.Turn]);
        Assert.Single(result.CallListArray[caller]);
        var call = result.CallListArray[caller].First();
        Assert.Equal(CallType.Chi, call.Type);
        Assert.Equal(discarded, call.CalledTile);
        Assert.Equal(caller, result.Turn);
    }

    [Fact]
    public void 手牌にない牌を指定_例外を投げる()
    {
        // Arrange
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        round = round.Dahai(new Tile(83), RoundTestHelper.NoOpTenpaiChecker);
        var caller = new PlayerIndex(1);
        // P1 の手牌には Tile(84) も Tile(88) も含めない
        round = RoundTestHelper.InjectHand(round, caller,
        [
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16), new Tile(17),
            new Tile(20),
        ]);
        var handTiles = ImmutableList.Create(new Tile(84), new Tile(88));

        // Act
        var ex = Record.Exception(() => round.Chi(caller, handTiles));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 手牌の同種枚数を超える同一牌を指定_例外を投げる()
    {
        // Arrange
        // 手牌には Tile(84) が1枚しか無いが、handTiles に Tile(84) を2枚指定するケース
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        round = round.Dahai(new Tile(83), RoundTestHelper.NoOpTenpaiChecker);
        var caller = new PlayerIndex(1);
        round = RoundTestHelper.InjectHand(round, caller,
        [
            new Tile(84),
            new Tile(0), new Tile(1), new Tile(2), new Tile(3),
            new Tile(4), new Tile(5), new Tile(6), new Tile(7),
            new Tile(12), new Tile(13), new Tile(16), new Tile(17),
        ]);
        var handTiles = ImmutableList.Create(new Tile(84), new Tile(84));

        // Act
        var ex = Record.Exception(() => round.Chi(caller, handTiles));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }
}
