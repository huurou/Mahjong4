using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Agaris;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Inits;
using Mahjong.Lib.Scoring.Yakus;

namespace Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.AgariInfos;

/// <summary>
/// Init・Agari・対局Idから <see cref="AgariInfo"/> を組み立てるサービス
/// </summary>
public class AgariInfoBuildService
{
    private const int RIICHI_NUMBER = 1;
    private const int IPPATSU_NUMBER = 2;
    private const int CHANKAN_NUMBER = 3;
    private const int RINSHAN_NUMBER = 4;
    private const int HAITEI_NUMBER = 5;
    private const int HOUTEI_NUMBER = 6;
    private const int DOUBLE_RIICHI_NUMBER = 21;
    private const int TENHOU_NUMBER = 37;
    private const int CHIIHOU_NUMBER = 38;
    private const int RENHOU_NUMBER = 36;

    /// <summary>
    /// Init・Agari・対局Idから <see cref="AgariInfo"/> を生成します。
    /// </summary>
    /// <param name="GameId">対局Id</param>
    /// <param name="init">局開始情報</param>
    /// <param name="agari">和了情報</param>
    /// <returns>点数計算検証用の和了情報</returns>
    public static AgariInfo Build(string GameId, Init init, Agari agari)
    {
        var han = agari.YakuInfos.Sum(x => x.Han) + agari.Yakumans.Count * 13;
        var yakus = new List<Yaku>();
        foreach (var yakuInfo in agari.YakuInfos)
        {
            var yaku = FromNumber(yakuInfo.Number);
            if (yaku == Yaku.Dora || yaku == Yaku.Uradora || yaku == Yaku.Akadora)
            {
                // ドラは翻数分役リストに追加する
                yakus.AddRange(Enumerable.Repeat(yaku, yakuInfo.Han));
            }
            else
            {
                yakus.Add(yaku);
            }
        }
        yakus.AddRange(agari.Yakumans.Select(FromNumber));
        var winSituation = new WinSituation()
        {
            IsTsumo = agari.IsTsumo,
            IsRiichi = agari.YakuInfos.Any(x => x.Number == RIICHI_NUMBER),
            IsIppatsu = agari.YakuInfos.Any(x => x.Number == IPPATSU_NUMBER),
            IsChankan = agari.YakuInfos.Any(x => x.Number == CHANKAN_NUMBER),
            IsRinshan = agari.YakuInfos.Any(x => x.Number == RINSHAN_NUMBER),
            IsHaitei = agari.YakuInfos.Any(x => x.Number == HAITEI_NUMBER),
            IsHoutei = agari.YakuInfos.Any(x => x.Number == HOUTEI_NUMBER),
            IsDoubleRiichi = agari.YakuInfos.Any(x => x.Number == DOUBLE_RIICHI_NUMBER),
            IsTenhou = agari.Yakumans.Any(x => x == TENHOU_NUMBER),
            IsChiihou = agari.Yakumans.Any(x => x == CHIIHOU_NUMBER),
            IsRenhou = agari.Yakumans.Any(x => x == RENHOU_NUMBER),
            PlayerWind = (Wind)((agari.Who + 4 - init.Oya) % 4),
            RoundWind = init.RoundWind,
            AkadoraCount = agari.AkadoraCount,
        };
        return new AgariInfo(
            GameId,
            init.Kyoku,
            init.Honba,
            agari.Hand,
            agari.WinTile,
            agari.CallList,
            agari.DoraIndicators,
            agari.UradoraIndicators,
            winSituation,
            agari.Fu,
            han,
            agari.Score,
            [.. yakus],
            agari.ManganType
        );
    }

    /// <summary>
    /// 天鳳の役番号を対応する <see cref="Yaku"/> シングルトンに変換します。
    /// </summary>
    /// <param name="number">天鳳の役番号</param>
    /// <returns>対応する役インスタンス</returns>
    private static Yaku FromNumber(int number)
    {
        return number switch
        {
            0 => Yaku.Tsumo,
            1 => Yaku.Riichi,
            2 => Yaku.Ippatsu,
            3 => Yaku.Chankan,
            4 => Yaku.Rinshan,
            5 => Yaku.Haitei,
            6 => Yaku.Houtei,
            7 => Yaku.Pinfu,
            8 => Yaku.Tanyao,
            9 => Yaku.Iipeikou,
            10 => Yaku.PlayerWindEast,
            11 => Yaku.PlayerWindSouth,
            12 => Yaku.PlayerWindWest,
            13 => Yaku.PlayerWindNorth,
            14 => Yaku.RoundWindEast,
            15 => Yaku.RoundWindSouth,
            16 => Yaku.RoundWindWest,
            17 => Yaku.RoundWindNorth,
            18 => Yaku.Haku,
            19 => Yaku.Hatsu,
            20 => Yaku.Chun,
            21 => Yaku.DoubleRiichi,
            22 => Yaku.Chiitoitsu,
            23 => Yaku.Chanta,
            24 => Yaku.Ittsuu,
            25 => Yaku.Sanshoku,
            26 => Yaku.Sanshokudoukou,
            27 => Yaku.Sankantsu,
            28 => Yaku.Toitoihou,
            29 => Yaku.Sanankou,
            30 => Yaku.Shousangen,
            31 => Yaku.Honroutou,
            32 => Yaku.Ryanpeikou,
            33 => Yaku.Junchan,
            34 => Yaku.Honitsu,
            35 => Yaku.Chinitsu,
            36 => Yaku.RenhouYakuman,
            37 => Yaku.Tenhou,
            38 => Yaku.Chiihou,
            39 => Yaku.Daisangen,
            40 => Yaku.Suuankou,
            41 => Yaku.SuuankouTanki,
            42 => Yaku.Tsuuiisou,
            43 => Yaku.Ryuuiisou,
            44 => Yaku.Chinroutou,
            45 => Yaku.Chuurenpoutou,
            46 => Yaku.JunseiChuurenpoutou,
            47 => Yaku.Kokushimusou,
            48 => Yaku.Kokushimusou13menmachi,
            49 => Yaku.Daisuushii,
            50 => Yaku.Shousuushii,
            51 => Yaku.Suukantsu,
            52 => Yaku.Dora,
            53 => Yaku.Uradora,
            54 => Yaku.Akadora,
            _ => throw new ArgumentException($"Unknown yaku number: {number}")
        };
    }
}
