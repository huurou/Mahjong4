using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Tests.Players;

internal static class PlayersTestHelper
{
    internal sealed record TestPlayer(PlayerId PlayerId, string DisplayName) : Player(PlayerId, DisplayName);

    internal static TestPlayer CreateTestPlayer(int index)
    {
        return new TestPlayer(PlayerId.NewId(), $"P{index}");
    }

    internal static TestPlayer[] CreateTestPlayers(int count)
    {
        return [.. Enumerable.Range(0, count).Select(CreateTestPlayer)];
    }
}
