using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Scoring.Yakus;

public abstract record Yaku : IComparable<Yaku>
{
    #region シングルトンプロパティ

    // 状況による役
    /// <summary>
    /// 立直
    /// </summary>
    public static Riichi Riichi { get; } = new();
    /// <summary>
    /// ダブル立直
    /// </summary>
    public static DoubleRiichi DoubleRiichi { get; } = new();
    /// <summary>
    /// 門前清自摸和
    /// </summary>
    public static Tsumo Tsumo { get; } = new();
    /// <summary>
    /// 一発
    /// </summary>
    public static Ippatsu Ippatsu { get; } = new();
    /// <summary>
    /// 槍槓
    /// </summary>
    public static Chankan Chankan { get; } = new();
    /// <summary>
    /// 嶺上開花
    /// </summary>
    public static Rinshan Rinshan { get; } = new();
    /// <summary>
    /// 海底撈月
    /// </summary>
    public static Haitei Haitei { get; } = new();
    /// <summary>
    /// 河底撈魚
    /// </summary>
    public static Houtei Houtei { get; } = new();
    /// <summary>
    /// 流し満貫
    /// </summary>
    public static Nagashimangan Nagashimangan { get; } = new();
    /// <summary>
    /// 人和
    /// </summary>
    public static Renhou Renhou { get; } = new();

    // 1翻
    /// <summary>
    /// 平和
    /// </summary>
    public static Pinfu Pinfu { get; } = new();
    /// <summary>
    /// 断么九
    /// </summary>
    public static Tanyao Tanyao { get; } = new();
    /// <summary>
    /// 一盃口
    /// </summary>
    public static Iipeikou Iipeikou { get; } = new();
    /// <summary>
    /// 白
    /// </summary>
    public static Haku Haku { get; } = new();
    /// <summary>
    /// 發
    /// </summary>
    public static Hatsu Hatsu { get; } = new();
    /// <summary>
    /// 中
    /// </summary>
    public static Chun Chun { get; } = new();
    /// <summary>
    /// 自風牌・東
    /// </summary>
    public static PlayerWindEast PlayerWindEast { get; } = new();
    /// <summary>
    /// 自風牌・南
    /// </summary>
    public static PlayerWindSouth PlayerWindSouth { get; } = new();
    /// <summary>
    /// 自風牌・西
    /// </summary>
    public static PlayerWindWest PlayerWindWest { get; } = new();
    /// <summary>
    /// 自風牌・北
    /// </summary>
    public static PlayerWindNorth PlayerWindNorth { get; } = new();
    /// <summary>
    /// 場風牌・東
    /// </summary>
    public static RoundWindEast RoundWindEast { get; } = new();
    /// <summary>
    /// 場風牌・南
    /// </summary>
    public static RoundWindSouth RoundWindSouth { get; } = new();
    /// <summary>
    /// 場風牌・西
    /// </summary>
    public static RoundWindWest RoundWindWest { get; } = new();
    /// <summary>
    /// 場風牌・北
    /// </summary>
    public static RoundWindNorth RoundWindNorth { get; } = new();

    // 2翻
    /// <summary>
    /// 三色同順
    /// </summary>
    public static Sanshoku Sanshoku { get; } = new();
    /// <summary>
    /// 一気通貫
    /// </summary>
    public static Ittsuu Ittsuu { get; } = new();
    /// <summary>
    /// 混全帯么九
    /// </summary>
    public static Chanta Chanta { get; } = new();
    /// <summary>
    /// 混老頭
    /// </summary>
    public static Honroutou Honroutou { get; } = new();
    /// <summary>
    /// 対々和
    /// </summary>
    public static Toitoihou Toitoihou { get; } = new();
    /// <summary>
    /// 三暗刻
    /// </summary>
    public static Sanankou Sanankou { get; } = new();
    /// <summary>
    /// 三槓子
    /// </summary>
    public static Sankantsu Sankantsu { get; } = new();
    /// <summary>
    /// 三色同刻
    /// </summary>
    public static Sanshokudoukou Sanshokudoukou { get; } = new();
    /// <summary>
    /// 七対子
    /// </summary>
    public static Chiitoitsu Chiitoitsu { get; } = new();
    /// <summary>
    /// 小三元
    /// </summary>
    public static Shousangen Shousangen { get; } = new();

    // 3翻
    /// <summary>
    /// 混一色
    /// </summary>
    public static Honitsu Honitsu { get; } = new();
    /// <summary>
    /// 純全帯么九
    /// </summary>
    public static Junchan Junchan { get; } = new();
    /// <summary>
    /// 二盃口
    /// </summary>
    public static Ryanpeikou Ryanpeikou { get; } = new();

    // 6翻
    /// <summary>
    /// 清一色
    /// </summary>
    public static Chinitsu Chinitsu { get; } = new();

    // 役満
    /// <summary>
    /// 国士無双
    /// </summary>
    public static Kokushimusou Kokushimusou { get; } = new();
    /// <summary>
    /// 国士無双十三面待ち
    /// </summary>
    public static Kokushimusou13menmachi Kokushimusou13menmachi { get; } = new();
    /// <summary>
    /// 九蓮宝燈
    /// </summary>
    public static Chuurenpoutou Chuurenpoutou { get; } = new();
    /// <summary>
    /// 純正九蓮宝燈
    /// </summary>
    public static JunseiChuurenpoutou JunseiChuurenpoutou { get; } = new();
    /// <summary>
    /// 四暗刻
    /// </summary>
    public static Suuankou Suuankou { get; } = new();
    /// <summary>
    /// 四暗刻単騎待ち
    /// </summary>
    public static SuuankouTanki SuuankouTanki { get; } = new();
    /// <summary>
    /// 大三元
    /// </summary>
    public static Daisangen Daisangen { get; } = new();
    /// <summary>
    /// 小四喜
    /// </summary>
    public static Shousuushii Shousuushii { get; } = new();
    /// <summary>
    /// 大四喜
    /// </summary>
    public static Daisuushii Daisuushii { get; } = new();
    /// <summary>
    /// 緑一色
    /// </summary>
    public static Ryuuiisou Ryuuiisou { get; } = new();
    /// <summary>
    /// 四槓子
    /// </summary>
    public static Suukantsu Suukantsu { get; } = new();
    /// <summary>
    /// 字一色
    /// </summary>
    public static Tsuuiisou Tsuuiisou { get; } = new();
    /// <summary>
    /// 清老頭
    /// </summary>
    public static Chinroutou Chinroutou { get; } = new();
    /// <summary>
    /// 大車輪
    /// </summary>
    public static Daisharin Daisharin { get; } = new();

    // ダブル役満
    /// <summary>
    /// 大四喜
    /// </summary>
    public static DaisuushiiDouble DaisuushiiDouble { get; } = new();
    /// <summary>
    /// 国士無双十三面待ち
    /// </summary>
    public static Kokushimusou13menmachiDouble Kokushimusou13menmachiDouble { get; } = new();
    /// <summary>
    /// 四暗刻単騎待ち
    /// </summary>
    public static SuuankouTankiDouble SuuankouTankiDouble { get; } = new();
    /// <summary>
    /// 純正九蓮宝燈
    /// </summary>
    public static JunseiChuurenpoutouDouble JunseiChuurenpoutouDouble { get; } = new();

    // 状況による役満
    /// <summary>
    /// 天和
    /// </summary>
    public static Tenhou Tenhou { get; } = new();
    /// <summary>
    /// 地和
    /// </summary>
    public static Chiihou Chiihou { get; } = new();
    /// <summary>
    /// 人和
    /// </summary>
    public static RenhouYakuman RenhouYakuman { get; } = new();

    // ドラ
    /// <summary>
    /// ドラ
    /// </summary>
    public static Dora Dora { get; } = new();
    /// <summary>
    /// 裏ドラ
    /// </summary>
    public static Uradora Uradora { get; } = new();
    /// <summary>
    /// 赤ドラ
    /// </summary>
    public static Akadora Akadora { get; } = new();

    #endregion シングルトンプロパティ

    /// <summary>
    /// 役番号 天鳳に合わせる
    /// 参考: https://github.com/NegativeMjark/tenhou-log.git
    /// </summary>
    public abstract int Number { get; }
    /// <summary>
    /// 役名
    /// </summary>
    public abstract string Name { get; }
    /// <summary>
    /// 非門前時の翻数 非面前では成立しない場合は0
    /// </summary>
    public abstract int HanOpen { get; }
    /// <summary>
    /// 面前時の翻数
    /// </summary>
    public abstract int HanClosed { get; }
    /// <summary>
    /// 役満かどうか
    /// </summary>
    public abstract bool IsYakuman { get; }

    public int CompareTo(Yaku? other)
    {
        if (other is null) { return 1; }
        var result = Number.CompareTo(other.Number);
        return result != 0 ? result : HanClosed.CompareTo(other.HanClosed);
    }

    public static bool operator <(Yaku? left, Yaku? right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    public static bool operator <=(Yaku? left, Yaku? right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    public static bool operator >(Yaku? left, Yaku? right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    public static bool operator >=(Yaku? left, Yaku? right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }

    public sealed override string ToString()
    {
        return Name;
    }
}
