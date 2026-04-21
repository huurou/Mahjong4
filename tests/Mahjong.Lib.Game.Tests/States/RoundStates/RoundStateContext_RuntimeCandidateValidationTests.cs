using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.RoundStates;
using Mahjong.Lib.Game.Tests.Players;
using Mahjong.Lib.Game.Tests.Rounds;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.States.RoundStates;

public class RoundStateContext_RuntimeCandidateValidationTests
{
    [Fact]
    public async Task 候補外ChiResponse_却下され流局まで進行する()
    {
        // Arrange: player[1] が手牌にない Tile 組で ChiResponse → 候補外
        var players = new FakePlayer[]
        {
            FakePlayer.Create(0),
            new(PlayerId.NewId(), "F1", new PlayerIndex(1))
            {
                OnDahai = (_, _) => new ChiResponse([new Tile(135), new Tile(134)]),
            },
            FakePlayer.Create(2),
            FakePlayer.Create(3),
        };
        using var ctx = RoundStateContextRuntimeTestHelper.CreateDefaultContext(players);

        // Act
        var task = ctx.StartAsync(RoundTestHelper.CreateRound(), TestContext.Current.CancellationToken);
        var result = await RoundStateContextRuntimeTestHelper.AwaitRoundEndAsync(task, TimeSpan.FromSeconds(15));

        // Assert
        var ryu = Assert.IsType<RoundEndedByRyuukyokuEventArgs>(result);
        Assert.Equal(RyuukyokuType.KouhaiHeikyoku, ryu.Type);
    }
}
