using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Tests.Players;

internal static class PlayersTestHelper
{
    internal sealed class TestPlayer(PlayerId playerId, string displayName, PlayerIndex playerIndex) : Player(playerId, displayName, playerIndex)
    {
        public override Task<OkResponse> OnGameStartAsync(GameStartNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }

        public override Task<OkResponse> OnRoundStartAsync(RoundStartNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }

        public override Task<OkResponse> OnRoundEndAsync(RoundEndNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }

        public override Task<OkResponse> OnGameEndAsync(GameEndNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }

        public override Task<OkResponse> OnHaipaiAsync(HaipaiNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }

        public override Task<OkResponse> OnOtherPlayerTsumoAsync(OtherPlayerTsumoNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }

        public override Task<OkResponse> OnCallAsync(CallNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }

        public override Task<DahaiResponse> OnAfterCallAsync(AfterCallNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<OkResponse> OnOtherPlayerAfterCallAsync(OtherPlayerAfterCallNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }

        public override Task<OkResponse> OnDoraRevealAsync(DoraRevealNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }

        public override Task<OkResponse> OnWinAsync(WinNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }

        public override Task<OkResponse> OnRyuukyokuAsync(RyuukyokuNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }

        public override Task<AfterTsumoResponse> OnTsumoAsync(TsumoNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<PlayerResponse> OnDahaiAsync(DahaiNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<PlayerResponse> OnKanAsync(KanNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<AfterKanTsumoResponse> OnKanTsumoAsync(KanTsumoNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<OkResponse> OnOtherPlayerKanTsumoAsync(OtherPlayerKanTsumoNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }
    }

    internal static TestPlayer CreateTestPlayer(int index)
    {
        return new TestPlayer(PlayerId.NewId(), $"P{index}", new PlayerIndex(index));
    }

    internal static TestPlayer[] CreateTestPlayers(int count)
    {
        return [.. Enumerable.Range(0, count).Select(CreateTestPlayer)];
    }
}
