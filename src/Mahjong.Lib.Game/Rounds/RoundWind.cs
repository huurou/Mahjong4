namespace Mahjong.Lib.Game.Rounds;

/// <summary>
/// 場風 RoundNumberと合わせて東一局、南二局などを表す
/// 0-3の範囲で、0が東、1が南、2が西、3が北を表す
/// </summary>
public record RoundWind
{
    private const int WIND_MIN = 0;
    private const int WIND_MAX = 3;

    #region シングルトンプロパティ

    /// <summary>
    /// 東
    /// </summary>
    public static RoundWind East { get; } = new RoundWind(0);

    /// <summary>
    /// 南
    /// </summary>
    public static RoundWind South { get; } = new RoundWind(1);

    /// <summary>
    /// 西
    /// </summary>
    public static RoundWind West { get; } = new RoundWind(2);

    /// <summary>
    /// 北
    /// </summary>
    public static RoundWind North { get; } = new RoundWind(3);

    #endregion シングルトンプロパティ

    /// <summary>
    /// 値
    /// 0-3の範囲で、0が東、1が南、2が西、3が北を表す
    /// </summary>
    internal int Value { get; init; }

    public RoundWind(int value)
    {
        if (value is < WIND_MIN or > WIND_MAX)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"場風は {WIND_MIN} から {WIND_MAX} の範囲内である必要があります。");
        }
        Value = value;
    }
}
