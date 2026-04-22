using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Shantens;

/// <summary>
/// 手牌のシャンテン数を計算するクラス
/// </summary>
public static class ShantenCalculator
{
    /// <summary>
    /// 手牌のシャンテン数を計算する
    /// </summary>
    /// <param name="tileKindList">シャンテン数を計算する手牌</param>
    /// <param name="useRegular">通常形で計算するかどうか</param>
    /// <param name="useChiitoitsu">七対子形で計算するかどうか</param>
    /// <param name="useKokushi">国士無双形で計算するかどうか</param>
    /// <returns>通常形、七対子形、国士無双形の中で最も小さいシャンテン数</returns>
    /// <exception cref="ArgumentException">手牌の数が14枚を超える場合</exception>
    /// <exception cref="ArgumentException">同じ牌種が5枚以上含まれている場合</exception>
    /// <exception cref="ArgumentException">useRegular、useChiitoitsu、useKokushi のすべてが false の場合</exception>
    public static int Calc(TileKindList tileKindList, bool useRegular = true, bool useChiitoitsu = true, bool useKokushi = true)
    {
        ValidateTileKindList(tileKindList);
        ValidateForms(useRegular, useChiitoitsu, useKokushi);

        List<int> shantens = [];
        if (useRegular)
        {
            shantens.Add(CalcForRegular(tileKindList));
        }
        if (useChiitoitsu)
        {
            shantens.Add(CalcForChiitoitsu(tileKindList));
        }
        if (useKokushi)
        {
            shantens.Add(CalcForKokushimusou(tileKindList));
        }

        return shantens.Min();
    }

    /// <summary>
    /// 手牌のシャンテン数を、副露相当の確定面子数を明示指定して計算する。
    /// 役別シャンテン (断么九なら么九除去後・一色手なら他スート除去後など) のように手牌を
    /// フィルタした状態では、既定の <see cref="Calc(TileKindList, bool, bool, bool)"/> の
    /// 「(14 - 手牌枚数) / 3 で自動推定」ロジックが実態とずれるため、この overload では
    /// 呼び出し側が確定面子数を明示する。
    /// </summary>
    /// <param name="tileKindList">シャンテン数を計算する手牌 (副露相当分は含めない)</param>
    /// <param name="knownCallMeldCount">副露・暗槓などで既に確定済みの面子数 (0 以上)</param>
    /// <param name="useRegular">通常形で計算するかどうか</param>
    /// <param name="useChiitoitsu">七対子形で計算するかどうか</param>
    /// <param name="useKokushi">国士無双形で計算するかどうか</param>
    /// <returns>通常形、七対子形、国士無双形の中で最も小さいシャンテン数</returns>
    public static int Calc(TileKindList tileKindList, int knownCallMeldCount, bool useRegular = true, bool useChiitoitsu = true, bool useKokushi = true)
    {
        if (knownCallMeldCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(knownCallMeldCount), knownCallMeldCount, "確定面子数は 0 以上である必要があります。");
        }
        ValidateTileKindList(tileKindList);
        ValidateForms(useRegular, useChiitoitsu, useKokushi);

        List<int> shantens = [];
        if (useRegular)
        {
            shantens.Add(CalcForRegularWithMeldCount(tileKindList, knownCallMeldCount));
        }
        if (useChiitoitsu)
        {
            shantens.Add(CalcForChiitoitsu(tileKindList));
        }
        if (useKokushi)
        {
            shantens.Add(CalcForKokushimusou(tileKindList));
        }

        return shantens.Min();
    }

    private static void ValidateTileKindList(TileKindList tileKindList)
    {
        if (tileKindList.Count > 14)
        {
            throw new ArgumentException($"手牌の数が14個より多いです。tileKindList:{tileKindList}", nameof(tileKindList));
        }
        if (TileKind.All.Any(x => tileKindList.CountOf(x) > 4))
        {
            throw new ArgumentException($"同じ牌種が5枚以上含まれています。tileKindList:{tileKindList}", nameof(tileKindList));
        }
    }

    private static void ValidateForms(bool useRegular, bool useChiitoitsu, bool useKokushi)
    {
        if (!useRegular && !useChiitoitsu && !useKokushi)
        {
            throw new ArgumentException("最低でも1つの形を指定してください。");
        }
    }

    /// <summary>
    /// 通常形（4面子1雀頭）のシャンテン数を計算する (確定面子数は手牌枚数から自動推定)
    /// </summary>
    /// <param name="tileKindList">シャンテン数を計算する牌種別リスト</param>
    /// <returns>通常形のシャンテン数</returns>
    private static int CalcForRegular(TileKindList tileKindList)
    {
        // 14枚から不足している3枚組を、既に確定済みの面子数として加算する（副露・暗槓相当）
        var autoInferredMelds = (14 - tileKindList.Count) / 3;
        return CalcForRegularWithMeldCount(tileKindList, autoInferredMelds);
    }

    /// <summary>
    /// 通常形（4面子1雀頭）のシャンテン数を、確定面子数を明示して計算する
    /// </summary>
    private static int CalcForRegularWithMeldCount(TileKindList tileKindList, int callMeldCount)
    {
        var context = new ShantenContext(tileKindList);
        context = context.ScanHonor();
        context = context with
        {
            MentsuCount = context.MentsuCount + callMeldCount,
            KantsuNumbers = [.. TileKind.Numbers.Where(x => context.TileKindList.CountOf(x) == 4)],
        };
        return context.ScanNumber();
    }

    /// <summary>
    /// 七対子形のシャンテン数を計算する
    /// </summary>
    /// <param name="tileKindList">シャンテン数を計算する牌種別リスト</param>
    /// <returns>七対子形のシャンテン数</returns>
    private static int CalcForChiitoitsu(TileKindList tileKindList)
    {
        var toitsuCount = tileKindList.Distinct().Count(x => tileKindList.CountOf(x) >= 2);
        var kindCount = tileKindList.Distinct().Count();
        return 6 - toitsuCount + Math.Max(0, 7 - kindCount);
    }

    /// <summary>
    /// 国士無双形のシャンテン数を計算する
    /// </summary>
    /// <param name="tileKindList">シャンテン数を計算する牌種別リスト</param>
    /// <returns>国士無双形のシャンテン数</returns>
    private static int CalcForKokushimusou(TileKindList tileKindList)
    {
        var yaochuuToitsuCount = TileKind.All.Count(x => x.IsYaochu && tileKindList.CountOf(x) >= 2);
        var yaochuuCount = tileKindList.Distinct().Count(x => x.IsYaochu);
        return 13 - yaochuuCount - (yaochuuToitsuCount != 0 ? 1 : 0);
    }
}
