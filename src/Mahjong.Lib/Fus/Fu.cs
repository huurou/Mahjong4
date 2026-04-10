namespace Mahjong.Lib.Fus;

/// <summary>
/// 符を表現するクラス
/// </summary>
/// <param name="Type">符種別</param>
public record Fu(FuType Type, int Number) : IComparable<Fu>
{
    #region シングルトンプロパティ

    /// <summary>
    /// 副底
    /// </summary>
    public static Fu Futei { get; } = new(FuType.Futei, 0);
    /// <summary>
    /// 面前加符
    /// </summary>
    public static Fu Menzen { get; } = new(FuType.Menzen, 1);
    /// <summary>
    /// 七対子符
    /// </summary>
    public static Fu Chiitoitsu { get; } = new(FuType.Chiitoitsu, 2);
    /// <summary>
    /// 副底(食い平和) ロンあがり専用
    /// </summary>
    public static Fu FuteiOpenPinfu { get; } = new(FuType.FuteiOpenPinfu, 3);
    /// <summary>
    /// ツモ符
    /// </summary>
    public static Fu Tsumo { get; } = new(FuType.Tsumo, 4);
    /// <summary>
    /// カンチャン待ち
    /// </summary>
    public static Fu Kanchan { get; } = new(FuType.Kanchan, 5);
    /// <summary>
    /// ペンチャン待ち
    /// </summary>
    public static Fu Penchan { get; } = new(FuType.Penchan, 6);
    /// <summary>
    /// 単騎待ち
    /// </summary>
    public static Fu Tanki { get; } = new(FuType.Tanki, 7);
    /// <summary>
    /// 自風の雀頭
    /// </summary>
    public static Fu JantouPlayerWind { get; } = new(FuType.JantouPlayerWind, 8);
    /// <summary>
    /// 場風の雀頭
    /// </summary>
    public static Fu JantouRoundWind { get; } = new(FuType.JantouRoundWind, 9);
    /// <summary>
    /// 三元牌の雀頭
    /// </summary>
    public static Fu JantouDragon { get; } = new(FuType.JantouDragon, 10);
    /// <summary>
    /// 中張牌の明刻
    /// </summary>
    public static Fu MinkoChunchan { get; } = new(FuType.MinkoChunchan, 11);
    /// <summary>
    /// 么九牌の明刻
    /// </summary>
    public static Fu MinkoYaochu { get; } = new(FuType.MinkoYaochu, 12);
    /// <summary>
    /// 中張牌の暗刻
    /// </summary>
    public static Fu AnkoChunchan { get; } = new(FuType.AnkoChunchan, 13);
    /// <summary>
    /// 么九牌の暗刻
    /// </summary>
    public static Fu AnkoYaochu { get; } = new(FuType.AnkoYaochu, 14);
    /// <summary>
    /// 中張牌の明槓
    /// </summary>
    public static Fu MinkanChunchan { get; } = new(FuType.MinkanChunchan, 15);
    /// <summary>
    /// 么九牌の明槓
    /// </summary>
    public static Fu MinkanYaochu { get; } = new(FuType.MinkanYaochu, 16);
    /// <summary>
    /// 中張牌の暗槓
    /// </summary>
    public static Fu AnkanChunchan { get; } = new(FuType.AnkanChunchan, 17);
    /// <summary>
    /// 么九牌の暗槓
    /// </summary>
    public static Fu AnkanYaochu { get; } = new(FuType.AnkanYaochu, 18);

    #endregion シングルトンプロパティ

    /// <summary>
    /// 符の値を取得します。
    /// </summary>
    public int Value => Type switch
    {
        FuType.Futei => 20,
        FuType.Menzen => 10,
        FuType.Chiitoitsu => 25,
        FuType.FuteiOpenPinfu => 30,
        FuType.Tsumo => 2,
        FuType.Kanchan => 2,
        FuType.Penchan => 2,
        FuType.Tanki => 2,
        FuType.JantouPlayerWind => 2,
        FuType.JantouRoundWind => 2,
        FuType.JantouDragon => 2,
        FuType.MinkoChunchan => 2,
        FuType.MinkoYaochu => 4,
        FuType.AnkoChunchan => 4,
        FuType.AnkoYaochu => 8,
        FuType.MinkanChunchan => 8,
        FuType.MinkanYaochu => 16,
        FuType.AnkanChunchan => 16,
        FuType.AnkanYaochu => 32,
        _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, ""),
    };

    public int CompareTo(Fu? other)
    {
        if (other is null) { return 1; }
        var numberComparison = Number.CompareTo(other.Number);
        return numberComparison != 0 ? numberComparison : Type.CompareTo(other.Type);
    }

    public static bool operator <(Fu? left, Fu? right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    public static bool operator <=(Fu? left, Fu? right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    public static bool operator >(Fu? left, Fu? right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    public static bool operator >=(Fu? left, Fu? right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// 符のテキスト表現を取得する
    /// </summary>
    /// <returns>「名前:値符」の形式の文字列</returns>
    public sealed override string ToString()
    {
        return $"{Type.ToStr()}:{Value}符";
    }
}
