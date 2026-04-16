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

    /// <summary>
    /// 指定本数だけリーチ棒を加算した新しいインスタンスを返します。
    /// </summary>
    public KyoutakuRiichiCount Add(int count)
    {
        return this with { Value = Value + count };
    }

    /// <summary>
    /// リーチ棒を0にリセットした新しいインスタンスを返します。
    /// </summary>
    public KyoutakuRiichiCount Clear()
    {
        return this with { Value = 0 };
    }
}
