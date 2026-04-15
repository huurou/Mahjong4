namespace Mahjong.Lib.Game.Rounds;

/// <summary>
/// 供託リーチ棒の本数
/// </summary>
public record KyoutakuRiichiCount
{
    /// <summary>
    /// 値
    /// </summary>
    public int Value { get; init; }

    public KyoutakuRiichiCount(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "立直の数は0以上である必要があります。");
        }

        Value = value;
    }
}
