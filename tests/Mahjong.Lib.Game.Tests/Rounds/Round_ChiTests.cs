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
        // P0 が Tile(83) (kind 20 = 3索) を打牌。P1 が kind 21,22 の牌で 3-4-5索 の順子を作る。
        // P1 の配牌には kind 21 (Tile(86)) のみでチー不成立なので、テスト用に Tile(84), Tile(88) を手牌に注入する。
        var round = RoundTestHelper.CreateRound(0).Haipai().Tsumo();
        var discarded = new Tile(83);
        round = round.Dahai(discarded);
        var caller = new PlayerIndex(1);
        var handTiles = ImmutableList.Create(new Tile(84), new Tile(88)); // kind 21, 22
        round = round with
        {
            HandArray = round.HandArray.AddTile(caller, new Tile(84)).AddTile(caller, new Tile(88))
        };

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
}
