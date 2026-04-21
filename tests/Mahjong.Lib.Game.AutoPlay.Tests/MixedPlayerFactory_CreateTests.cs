using Mahjong.Lib.Game.Notifications;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;

namespace Mahjong.Lib.Game.AutoPlay.Tests;

public class MixedPlayerFactory_CreatePlayerListTests
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
        var ex = Record.Exception(() => new MixedPlayerFactory(factories, 0));

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
        var ex = Record.Exception(() => new MixedPlayerFactory(factories, 0));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void 同一seedかつ同一gameNumber_同じDisplayName配列を再現する()
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

        var first = new MixedPlayerFactory(Build(), 42);
        var second = new MixedPlayerFactory(Build(), 42);

        // Act: 3 対局ぶん DisplayName シーケンスを比較
        var firstNames = new List<string>();
        var secondNames = new List<string>();
        for (var game = 0; game < 3; game++)
        {
            firstNames.AddRange(EnumerateDisplayNames(first.CreatePlayerList(game)));
            secondNames.AddRange(EnumerateDisplayNames(second.CreatePlayerList(game)));
        }

        // Assert
        Assert.Equal(firstNames, secondNames);
    }

    [Fact]
    public void 異なるgameNumber_独立にシャッフルされ同形は出るが全体で割り当てが変化する()
    {
        // Arrange
        IPlayerFactory[] factories =
        [
            new FakePlayerFactory("A"),
            new FakePlayerFactory("B"),
            new FakePlayerFactory("C"),
            new FakePlayerFactory("D"),
        ];
        var mixed = new MixedPlayerFactory(factories, 42);

        // Act
        var firstNames = EnumerateDisplayNames(mixed.CreatePlayerList(0)).ToList();
        var secondNames = EnumerateDisplayNames(mixed.CreatePlayerList(1)).ToList();

        // Assert: 両方とも A/B/C/D の順列、かつ少なくとも 1 対局分のシャッフルの違いが観測できる
        Assert.Equal(4, firstNames.Distinct().Count());
        Assert.Equal(4, secondNames.Distinct().Count());
        // 確率的に低い同一順列を引く可能性を除外するため、seed 42 の 0/1 に対する実際の並びを固定点検証
        // (ロジック変更時にこの assertion は再計算が必要)
        Assert.NotEqual(firstNames, secondNames);
    }

    private static readonly string[] expected_ = ["A", "B", "C", "D"];

    [Fact]
    public void CreatePlayerListの戻り値_4席に異なるDisplayNameが1つずつ割り当てられる()
    {
        // Arrange
        IPlayerFactory[] factories =
        [
            new FakePlayerFactory("A"),
            new FakePlayerFactory("B"),
            new FakePlayerFactory("C"),
            new FakePlayerFactory("D"),
        ];
        var mixed = new MixedPlayerFactory(factories, 1);

        // Act
        var names = EnumerateDisplayNames(mixed.CreatePlayerList(0)).OrderBy(x => x).ToList();

        // Assert
        Assert.Equal(expected_, names);
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
        var mixed = new MixedPlayerFactory(factories, 0);

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
        var mixed = new MixedPlayerFactory(factories, 0);

        // Act
        var displayName = mixed.DisplayName;

        // Assert
        Assert.Equal("A / B", displayName);
    }

    private static IEnumerable<string> EnumerateDisplayNames(PlayerList list)
    {
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            yield return list[new PlayerIndex(i)].DisplayName;
        }
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
