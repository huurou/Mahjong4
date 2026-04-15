namespace Mahjong.Lib.Game.Rounds;

/// <summary>
/// 本場
/// </summary>
/// <param name="Value">値</param>
public record Honba
{
    public int Value { get; init; }

    public Honba(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "本場は0以上である必要があります。");
        }

        Value = value;
    }
}
