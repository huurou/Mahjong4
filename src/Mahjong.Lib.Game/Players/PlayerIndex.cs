namespace Mahjong.Lib.Game.Players;

/// <summary>
/// プレイヤーに割り振られたインデックス
/// </summary>
public record PlayerIndex
{
    public const int INDEX_MIN = 0;
    public const int INDEX_MAX = 3;
    public const int PLAYER_COUNT = 4;

    /// <summary>
    /// 値
    /// </summary>
    public int Value { get; init; }

    public PlayerIndex(int value)
    {
        if (value is < INDEX_MIN or > INDEX_MAX)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"プレイヤーインデックスは {INDEX_MIN} から {INDEX_MAX} の範囲内である必要があります。");
        }

        Value = value;
    }

    /// <summary>
    /// 次のプレイヤー(反時計回り)を返します。
    /// </summary>
    public PlayerIndex Next()
    {
        return new PlayerIndex((Value + 1) % PLAYER_COUNT);
    }
}
