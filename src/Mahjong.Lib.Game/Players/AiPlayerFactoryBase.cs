namespace Mahjong.Lib.Game.Players;

/// <summary>
/// AI プレイヤーの <see cref="IPlayerFactory"/> 実装の共通基底。
/// 各 AI バージョンの Factory は本クラスを派生し、<see cref="CreatePlayer(PlayerId, PlayerIndex, Random)"/> のみを実装する。
///
/// 各席には seed と index を Fibonacci hashing (Knuth multiplicative) で決定的に合成した値で Random を初期化し、
/// 同一 seed に対する再現性を保ちつつ席間の内部状態が近接しないようにする。
/// <see cref="HashCode.Combine{T1, T2}(T1, T2)"/> はプロセス起動時のランダム salt を含むため再現性を壊すので使わない
/// </summary>
/// <typeparam name="TPlayer">生成する <see cref="Player"/> 派生型。CreatePlayer の戻り値型に使われる</typeparam>
public abstract class AiPlayerFactoryBase<TPlayer>(int seed, string displayName) 
    : IPlayerFactory where TPlayer : Player
{
    public string DisplayName { get; } = displayName;

    public Player Create(PlayerIndex index, PlayerId id)
    {
        return CreatePlayer(id, index, new Random(DeriveSeed(seed, index.Value)));
    }

    /// <summary>
    /// 派生 Factory が対応する AI の具象コンストラクタを呼ぶ。
    /// </summary>
    protected abstract TPlayer CreatePlayer(PlayerId id, PlayerIndex index, Random rng);

    private static int DeriveSeed(int seed, int index)
    {
        unchecked
        {
            return (int)((uint)seed * 0x9E3779B9u ^ (uint)index);
        }
    }
}
