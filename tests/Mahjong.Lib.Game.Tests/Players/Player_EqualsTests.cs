using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.Tests.Players;

public class Player_EqualsTests
{
    [Fact]
    public void 同じPlayerIdと同じDisplayName_等価になる()
    {
        // Arrange
        var id = PlayerId.NewId();
        var a = new PlayersTestHelper.TestPlayer(id, "A");
        var b = new PlayersTestHelper.TestPlayer(id, "A");

        // Act & Assert
        Assert.Equal(a, b);
        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void 異なるPlayerId_非等価になる()
    {
        // Arrange
        var a = new PlayersTestHelper.TestPlayer(PlayerId.NewId(), "A");
        var b = new PlayersTestHelper.TestPlayer(PlayerId.NewId(), "A");

        // Act & Assert
        Assert.NotEqual(a, b);
        Assert.False(a.Equals(b));
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void 異なるDisplayName_非等価になる()
    {
        // Arrange
        var id = PlayerId.NewId();
        var a = new PlayersTestHelper.TestPlayer(id, "A");
        var b = new PlayersTestHelper.TestPlayer(id, "B");

        // Act & Assert
        Assert.NotEqual(a, b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Null比較_非等価になる()
    {
        // Arrange
        var a = new PlayersTestHelper.TestPlayer(PlayerId.NewId(), "A");
        Player? b = null;

        // Act & Assert
        Assert.False(a.Equals(b));
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Null同士の演算子比較_等価になる()
    {
        // Arrange
        Player? a = null;
        Player? b = null;

        // Act & Assert
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void 異なる型でも同じPlayerIdとDisplayName_等価になる()
    {
        // Arrange
        var id = PlayerId.NewId();
        var a = new PlayersTestHelper.TestPlayer(id, "A");
        var b = new OtherTestPlayer(id, "A");

        // Act & Assert
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void PlayerIdがnull_ArgumentNullExceptionが発生する()
    {
        // Act
        var ex = Record.Exception(() => new PlayersTestHelper.TestPlayer(null!, "A"));

        // Assert
        Assert.IsType<ArgumentNullException>(ex);
    }

    [Fact]
    public void DisplayNameがnull_ArgumentNullExceptionが発生する()
    {
        // Act
        var ex = Record.Exception(() => new PlayersTestHelper.TestPlayer(PlayerId.NewId(), null!));

        // Assert
        Assert.IsType<ArgumentNullException>(ex);
    }

    private sealed class OtherTestPlayer(PlayerId playerId, string displayName) : Player(playerId, displayName)
    {
        public override Task<OkResponse> OnGameStartAsync(GameStartNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<OkResponse> OnRoundStartAsync(RoundStartNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<OkResponse> OnRoundEndAsync(RoundEndNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<OkResponse> OnGameEndAsync(GameEndNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<OkResponse> OnHaipaiAsync(HaipaiNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<OkResponse> OnOtherPlayerTsumoAsync(OtherPlayerTsumoNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<OkResponse> OnCallAsync(CallNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<OkResponse> OnDoraRevealAsync(DoraRevealNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<OkResponse> OnWinAsync(WinNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<OkResponse> OnRyuukyokuAsync(RyuukyokuNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<AfterTsumoResponse> OnTsumoAsync(TsumoNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<AfterDahaiResponse> OnDahaiAsync(DahaiNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<AfterKanResponse> OnKanAsync(KanNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override Task<AfterKanTsumoResponse> OnKanTsumoAsync(KanTsumoNotification notification, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
