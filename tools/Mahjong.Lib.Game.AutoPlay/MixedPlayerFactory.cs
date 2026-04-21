using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.AutoPlay;

/// <summary>
/// プレイヤーインデックスごとに異なる <see cref="IPlayerFactory"/> を委譲する複合 Factory。
/// 対局ごとに席配置をシャッフルして起家バイアスを均し、AI 比較の公平性を確保する。
/// </summary>
/// <remarks>
/// 並列対局に耐えるため完全にステートレス。席配置は <paramref name="baseSeed"/> と
/// <see cref="CreatePlayerList"/> の gameNumber から Fibonacci hashing で決定的に導出し、
/// 同一 (seed, gameNumber) に対して常に同じ割り当てを返す
/// </remarks>
public sealed class MixedPlayerFactory
{
    private readonly IPlayerFactory[] factories_;
    private readonly int baseSeed_;

    public MixedPlayerFactory(IPlayerFactory[] factories, int baseSeed)
    {
        if (factories.Length != PlayerIndex.PLAYER_COUNT)
        {
            throw new ArgumentException(
                $"factories は {PlayerIndex.PLAYER_COUNT} 席分必要です。実際:{factories.Length}席",
                nameof(factories)
            );
        }
        factories_ = factories;
        baseSeed_ = baseSeed;
    }

    public string DisplayName => string.Join(" / ", factories_.Select(x => x.DisplayName).Distinct());

    public PlayerList CreatePlayerList(int gameNumber)
    {
        var rng = new Random(DeriveSeed(baseSeed_, gameNumber));
        var assignment = new int[PlayerIndex.PLAYER_COUNT];
        for (var i = 0; i < assignment.Length; i++) { assignment[i] = i; }
        for (var i = assignment.Length - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (assignment[i], assignment[j]) = (assignment[j], assignment[i]);
        }

        var players = Enumerable.Range(0, PlayerIndex.PLAYER_COUNT)
            .Select(x => factories_[assignment[x]].Create(new PlayerIndex(x), PlayerId.NewId()))
            .ToArray();
        return new PlayerList(players);
    }

    private static int DeriveSeed(int baseSeed, int gameNumber)
    {
        unchecked
        {
            return (int)((uint)baseSeed * 0x9E3779B9u ^ (uint)gameNumber);
        }
    }
}
