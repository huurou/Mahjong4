using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Mahjong.Lib.Scoring.Tiles;

/// <summary>
/// 牌種別
/// 0-33の値を取り、0-8が萬子・9-17が筒子・18-26が索子・27-33が字牌を表す
/// </summary>
public sealed record TileKind : IComparable<TileKind>
{
    /// <summary>
    /// TileKindの値の最小値
    /// </summary>
    public const int MIN_VALUE = 0;

    /// <summary>
    /// TileKindの値の最大値
    /// </summary>
    public const int MAX_VALUE = 33;

    #region シングルトンプロパティ

    /// <summary>
    /// 一萬
    /// </summary>
    public static TileKind Man1 { get; } = new(0);
    /// <summary>
    /// 二萬
    /// </summary>
    public static TileKind Man2 { get; } = new(1);
    /// <summary>
    /// 三萬
    /// </summary>
    public static TileKind Man3 { get; } = new(2);
    /// <summary>
    /// 四萬
    /// </summary>
    public static TileKind Man4 { get; } = new(3);
    /// <summary>
    /// 五萬
    /// </summary>
    public static TileKind Man5 { get; } = new(4);
    /// <summary>
    /// 六萬
    /// </summary>
    public static TileKind Man6 { get; } = new(5);
    /// <summary>
    /// 七萬
    /// </summary>
    public static TileKind Man7 { get; } = new(6);
    /// <summary>
    /// 八萬
    /// </summary>
    public static TileKind Man8 { get; } = new(7);
    /// <summary>
    /// 九萬
    /// </summary>
    public static TileKind Man9 { get; } = new(8);

    /// <summary>
    /// 一筒
    /// </summary>
    public static TileKind Pin1 { get; } = new(9);
    /// <summary>
    /// 二筒
    /// </summary>
    public static TileKind Pin2 { get; } = new(10);
    /// <summary>
    /// 三筒
    /// </summary>
    public static TileKind Pin3 { get; } = new(11);
    /// <summary>
    /// 四筒
    /// </summary>
    public static TileKind Pin4 { get; } = new(12);
    /// <summary>
    /// 五筒
    /// </summary>
    public static TileKind Pin5 { get; } = new(13);
    /// <summary>
    /// 六筒
    /// </summary>
    public static TileKind Pin6 { get; } = new(14);
    /// <summary>
    /// 七筒
    /// </summary>
    public static TileKind Pin7 { get; } = new(15);
    /// <summary>
    /// 八筒
    /// </summary>
    public static TileKind Pin8 { get; } = new(16);
    /// <summary>
    /// 九筒
    /// </summary>
    public static TileKind Pin9 { get; } = new(17);

    /// <summary>
    /// 一索
    /// </summary>
    public static TileKind Sou1 { get; } = new(18);
    /// <summary>
    /// 二索
    /// </summary>
    public static TileKind Sou2 { get; } = new(19);
    /// <summary>
    /// 三索
    /// </summary>
    public static TileKind Sou3 { get; } = new(20);
    /// <summary>
    /// 四索
    /// </summary>
    public static TileKind Sou4 { get; } = new(21);
    /// <summary>
    /// 五索
    /// </summary>
    public static TileKind Sou5 { get; } = new(22);
    /// <summary>
    /// 六索
    /// </summary>
    public static TileKind Sou6 { get; } = new(23);
    /// <summary>
    /// 七索
    /// </summary>
    public static TileKind Sou7 { get; } = new(24);
    /// <summary>
    /// 八索
    /// </summary>
    public static TileKind Sou8 { get; } = new(25);
    /// <summary>
    /// 九索
    /// </summary>
    public static TileKind Sou9 { get; } = new(26);

    /// <summary>
    /// 東
    /// </summary>
    public static TileKind Ton { get; } = new(27);
    /// <summary>
    /// 南
    /// </summary>
    public static TileKind Nan { get; } = new(28);
    /// <summary>
    /// 西
    /// </summary>
    public static TileKind Sha { get; } = new(29);
    /// <summary>
    /// 北
    /// </summary>
    public static TileKind Pei { get; } = new(30);

    /// <summary>
    /// 白
    /// </summary>
    public static TileKind Haku { get; } = new(31);
    /// <summary>
    /// 發
    /// </summary>
    public static TileKind Hatsu { get; } = new(32);
    /// <summary>
    /// 中
    /// </summary>
    public static TileKind Chun { get; } = new(33);

    #endregion シングルトンプロパティ

    /// <summary>
    /// 牌種別の値 (0-33の範囲)
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// 牌種別の数値 一萬・一筒・一索なら1 二萬・二筒・二索なら2
    /// </summary>
    public int Number => Value % 9 + 1;

    /// <summary>
    /// 全種類の牌のリスト
    /// </summary>
    public static ReadOnlyCollection<TileKind> All { get; } = [
        Man1, Man2, Man3, Man4, Man5, Man6, Man7, Man8, Man9,
        Pin1, Pin2, Pin3, Pin4, Pin5, Pin6, Pin7, Pin8, Pin9,
        Sou1, Sou2, Sou3, Sou4, Sou5, Sou6, Sou7, Sou8, Sou9,
        Ton, Nan, Sha, Pei, Haku, Hatsu, Chun,
    ];

    /// <summary>
    /// 数牌のリスト
    /// </summary>
    public static ReadOnlyCollection<TileKind> Numbers { get; } = [.. All.Where(x => x.IsNumber)];
    /// <summary>
    /// 萬子のリスト
    /// </summary>
    public static ReadOnlyCollection<TileKind> Mans { get; } = [.. All.Where(x => x.IsMan)];
    /// <summary>
    /// 筒子のリスト
    /// </summary>
    public static ReadOnlyCollection<TileKind> Pins { get; } = [.. All.Where(x => x.IsPin)];
    /// <summary>
    /// 索子のリスト
    /// </summary>
    public static ReadOnlyCollection<TileKind> Sous { get; } = [.. All.Where(x => x.IsSou)];
    /// <summary>
    /// 字牌のリスト
    /// </summary>
    public static ReadOnlyCollection<TileKind> Honors { get; } = [.. All.Where(x => x.IsHonor)];
    /// <summary>
    /// 風牌のリスト
    /// </summary>
    public static ReadOnlyCollection<TileKind> Winds { get; } = [.. All.Where(x => x.IsWind)];
    /// <summary>
    /// 三元牌のリスト
    /// </summary>
    public static ReadOnlyCollection<TileKind> Dragons { get; } = [.. All.Where(x => x.IsDragon)];
    /// <summary>
    /// 中張牌のリスト
    /// </summary>
    public static ReadOnlyCollection<TileKind> Chunchans { get; } = [.. All.Where(x => x.IsChunchan)];
    /// <summary>
    /// 么九牌のリスト
    /// </summary>
    public static ReadOnlyCollection<TileKind> Yaochus { get; } = [.. All.Where(x => x.IsYaochu)];
    /// <summary>
    /// 老頭牌のリスト
    /// </summary>
    public static ReadOnlyCollection<TileKind> Routous { get; } = [.. All.Where(x => x.IsRoutou)];

    /// <summary>
    /// 数牌かどうか
    /// </summary>
    public bool IsNumber => Value is >= 0 and <= 26;
    /// <summary>
    /// 萬子かどうか
    /// </summary>
    public bool IsMan => Value is >= 0 and <= 8;
    /// <summary>
    /// 筒子かどうか
    /// </summary>
    public bool IsPin => Value is >= 9 and <= 17;
    /// <summary>
    /// 索子かどうか
    /// </summary>
    public bool IsSou => Value is >= 18 and <= 26;
    /// <summary>
    /// 字牌かどうか
    /// </summary>
    public bool IsHonor => Value is >= 27 and <= 33;
    /// <summary>
    /// 風牌かどうか
    /// </summary>
    public bool IsWind => Value is >= 27 and <= 30;
    /// <summary>
    /// 三元牌かどうか
    /// </summary>
    public bool IsDragon => Value is >= 31 and <= 33;
    /// <summary>
    /// 中張牌かどうか
    /// </summary>
    public bool IsChunchan => Value is >= 1 and <= 7 or >= 10 and <= 16 or >= 19 and <= 25;
    /// <summary>
    /// 么九牌かどうか
    /// </summary>
    public bool IsYaochu => !IsChunchan;
    /// <summary>
    /// 老頭牌かどうか
    /// </summary>
    public bool IsRoutou => Value is 0 or 8 or 9 or 17 or 18 or 26;

    /// <summary>
    /// 牌種別のコンストラクタ
    /// </summary>
    /// <param name="value">牌種別の値 (0-33の範囲)</param>
    /// <exception cref="ArgumentOutOfRangeException">値が0-33の範囲外の場合</exception>
    internal TileKind(int value)
    {
        if (value is < MIN_VALUE or > MAX_VALUE) { throw new ArgumentOutOfRangeException(nameof(value), value, $"TileKindの値は{MIN_VALUE}から{MAX_VALUE}の範囲である必要があります。"); }

        Value = value;
    }

    /// <summary>
    /// 現在の牌種別から指定された距離だけ離れた牌種別を取得します。
    /// </summary>
    /// <param name="distance">移動する距離（正の値は大きい数字の牌種別へ、負の値は小さい数字の牌種別へ）</param>
    /// <param name="tileKind">指定された距離だけ離れた牌種別。字牌だったりスートが異なる場合はnull</param>
    /// <returns>指定された距離の牌が存在する場合はtrue、存在しない場合はfalse</returns>
    public bool TryGetAtDistance(int distance, [NotNullWhen(true)] out TileKind? tileKind)
    {
        if (IsHonor)
        {
            tileKind = null;
            return false;
        }

        var newValue = Value + distance;
        // 9で割った商が同じ時、同じスートの牌種別
        if (newValue >= MIN_VALUE && newValue <= MAX_VALUE && Value / 9 == newValue / 9)
        {
            tileKind = new TileKind(newValue);
            return true;
        }

        tileKind = null;
        return false;
    }

    /// <summary>
    /// ドラ表示牌から実際のドラを取得する
    /// </summary>
    /// <param name="doraIndicator">ドラ表示牌</param>
    /// <returns>実際のドラ</returns>
    public static TileKind GetActualDora(TileKind doraIndicator)
    {
        var value = doraIndicator.Value;
        var newValue = value switch
        {
            <= 26 => value / 9 * 9 + (value % 9 + 1) % 9, // 数牌: スート内で循環
            <= 30 => 27 + (value - 27 + 1) % 4,            // 風牌: 東南西北で循環
            <= 33 => 31 + (value - 31 + 1) % 3,            // 三元牌: 白發中で循環
            _ => throw new ArgumentOutOfRangeException(nameof(doraIndicator), doraIndicator, "不明なドラ表示牌です")
        };
        return new TileKind(newValue);
    }

    public int CompareTo(TileKind? other)
    {
        return other is not null ? Value.CompareTo(other.Value) : 1;
    }

    public static bool operator <(TileKind? left, TileKind? right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    public static bool operator <=(TileKind? left, TileKind? right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    public static bool operator >(TileKind? left, TileKind? right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    public static bool operator >=(TileKind? left, TileKind? right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }

    public sealed override string ToString()
    {
        return Value switch
        {
            0 => "一",
            1 => "二",
            2 => "三",
            3 => "四",
            4 => "五",
            5 => "六",
            6 => "七",
            7 => "八",
            8 => "九",
            9 => "(1)",
            10 => "(2)",
            11 => "(3)",
            12 => "(4)",
            13 => "(5)",
            14 => "(6)",
            15 => "(7)",
            16 => "(8)",
            17 => "(9)",
            18 => "1",
            19 => "2",
            20 => "3",
            21 => "4",
            22 => "5",
            23 => "6",
            24 => "7",
            25 => "8",
            26 => "9",
            27 => "東",
            28 => "南",
            29 => "西",
            30 => "北",
            31 => "白",
            32 => "發",
            33 => "中",
            _ => throw new ArgumentOutOfRangeException(nameof(Value), Value, $"無効な牌種別の値です: {Value}")
        };
    }
}
