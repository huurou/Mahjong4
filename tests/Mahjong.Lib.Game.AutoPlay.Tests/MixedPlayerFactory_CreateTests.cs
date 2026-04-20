using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.AutoPlay.Tests;

public class MixedPlayerFactory_CreateTests
{
    [Fact]
    public void 要素数が4未満_ArgumentException()
    {
        // Arrange
        var factories = new IPlayerFactory[]
        {
            new FakePlayerFactory("A"),
            new FakePlayerFactory("B"),
            new FakePlayerFactory("C"),
        };

        // Act
        var ex = Record.Exception(() => new MixedPlayerFactory(factories, new Random(0)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 要素数が4超過_ArgumentException()
    {
        // Arrange
        var factories = new IPlayerFactory[]
        {
            new FakePlayerFactory("A"),
            new FakePlayerFactory("B"),
            new FakePlayerFactory("C"),
            new FakePlayerFactory("D"),
            new FakePlayerFactory("E"),
        };

        // Act
        var ex = Record.Exception(() => new MixedPlayerFactory(factories, new Random(0)));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 同一seed_複数対局で同じ割り当てシーケンスを再現する()
    {
        // Arrange
        static IPlayerFactory[] Build()
        {
            return [
                new FakePlayerFactory("A"),
                new FakePlayerFactory("B"),
                new FakePlayerFactory("C"),
                new FakePlayerFactory("D"),
            ];
        }

        var first = new MixedPlayerFactory(Build(), new Random(42));
        var second = new MixedPlayerFactory(Build(), new Random(42));

        // Act: 3 対局ぶん (12 回) の Create を並走させて DisplayName シーケンスを比較
        var firstNames = new List<string>();
        var secondNames = new List<string>();
        for (var game = 0; game < 3; game++)
        {
            for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
            {
                var index = new PlayerIndex(i);
                firstNames.Add(first.Create(index, PlayerId.NewId()).DisplayName);
                secondNames.Add(second.Create(index, PlayerId.NewId()).DisplayName);
            }
        }

        // Assert
        Assert.Equal(firstNames, secondNames);
    }

    private static readonly string[] sourceArray_ = ["A", "B", "C", "D"];

    [Fact]
    public void 一対局4回のCreate_異なるDisplayNameが各席に一つずつ割り当てられる()
    {
        // Arrange
        IPlayerFactory[] factories =
        [
            new FakePlayerFactory("A"),
            new FakePlayerFactory("B"),
            new FakePlayerFactory("C"),
            new FakePlayerFactory("D"),
        ];
        var mixed = new MixedPlayerFactory(factories, new Random(1));

        // Act
        var names = Enumerable.Range(0, PlayerIndex.PLAYER_COUNT)
            .Select(x => mixed.Create(new PlayerIndex(x), PlayerId.NewId()).DisplayName)
            .ToList();

        // Assert: 4 種類の DisplayName が順不同でちょうど 1 回ずつ出現する
        Assert.Equal(sourceArray_.OrderBy(x => x), names.OrderBy(x => x));
    }

    [Fact]
    public void DisplayName_複数factoryの表示名をスラッシュで結合する()
    {
        // Arrange
        IPlayerFactory[] factories =
        [
            new FakePlayerFactory("A"),
            new FakePlayerFactory("B"),
            new FakePlayerFactory("C"),
            new FakePlayerFactory("D"),
        ];
        var mixed = new MixedPlayerFactory(factories, new Random(0));

        // Act
        var displayName = mixed.DisplayName;

        // Assert
        Assert.Equal("A / B / C / D", displayName);
    }

    [Fact]
    public void DisplayName_重複する表示名はDistinctで除去される()
    {
        // Arrange (A / A / B / B の混在対局)
        IPlayerFactory[] factories =
        [
            new FakePlayerFactory("A"),
            new FakePlayerFactory("A"),
            new FakePlayerFactory("B"),
            new FakePlayerFactory("B"),
        ];
        var mixed = new MixedPlayerFactory(factories, new Random(0));

        // Act
        var displayName = mixed.DisplayName;

        // Assert
        Assert.Equal("A / B", displayName);
    }

    private sealed class FakePlayerFactory(string name) : IPlayerFactory
    {
        public string DisplayName { get; } = name;

        public Player Create(PlayerIndex index, PlayerId id)
        {
            return new FakePlayer(id, DisplayName, index);
        }
    }

    private sealed class FakePlayer(PlayerId id, string name, PlayerIndex index) : Player(id, name, index)
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

        public override Task<OkResponse> OnOtherPlayerKanTsumoAsync(OtherPlayerKanTsumoNotification notification, CancellationToken ct = default)
        {
            return Task.FromResult(new OkResponse());
        }

        public override Task<AfterTsumoResponse> OnTsumoAsync(TsumoNotification notification, CancellationToken ct = default)
        {
            throw new NotSupportedException();
        }

        public override Task<PlayerResponse> OnDahaiAsync(DahaiNotification notification, CancellationToken ct = default)
        {
            throw new NotSupportedException();
        }

        public override Task<PlayerResponse> OnKanAsync(KanNotification notification, CancellationToken ct = default)
        {
            throw new NotSupportedException();
        }

        public override Task<AfterKanTsumoResponse> OnKanTsumoAsync(KanTsumoNotification notification, CancellationToken ct = default)
        {
            throw new NotSupportedException();
        }
    }
}
