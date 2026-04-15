using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.Rounds;

/// <summary>
/// 局数 RoundWindと合わせて東一局、南二局などを表す
/// 0-3の範囲で、0が一局、1が二局、2が三局、3が四局を表す
/// </summary>
public record RoundNumber
{
    private const int NUMBER_MIN = 0;
    private const int NUMBER_MAX = 3;

    /// <summary>
    /// 値
    /// 0-3の範囲で、0が一局、1が二局、2が三局、3が四局を表す
    /// </summary>
    public int Value { get; init; }

    public RoundNumber(int value)
    {
        if (value is < NUMBER_MIN or > NUMBER_MAX)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"局数は {NUMBER_MIN} から {NUMBER_MAX} の範囲内である必要があります。");
        }

        Value = value;
    }

    /// <summary>
    /// この局での親のプレイヤーインデックスを返します。
    /// </summary>
    public PlayerIndex ToDealer()
    {
        return new PlayerIndex(Value);
    }
}
