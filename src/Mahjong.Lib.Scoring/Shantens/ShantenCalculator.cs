using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Shantens;

/// <summary>
/// 手牌のシャンテン数を計算するクラス
/// </summary>
public static class ShantenCalculator
{
    private const int TileKindCount = 34;

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
        Span<int> counts = stackalloc int[TileKindCount];
        counts.Clear();

        var tileCount = BuildCounts(tileKindList, counts);
        ValidateForms(useRegular, useChiitoitsu, useKokushi);

        var autoInferredMelds = (14 - tileCount) / 3;
        return CalcCore(counts, tileCount, autoInferredMelds, useRegular, useChiitoitsu, useKokushi);
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

        Span<int> counts = stackalloc int[TileKindCount];
        counts.Clear();

        var tileCount = BuildCounts(tileKindList, counts);
        ValidateForms(useRegular, useChiitoitsu, useKokushi);
        return CalcCore(counts, tileCount, knownCallMeldCount, useRegular, useChiitoitsu, useKokushi);
    }

    /// <summary>
    /// 34 牌種の枚数配列からシャンテン数を計算する
    /// </summary>
    /// <param name="counts">34 要素の牌種別枚数配列</param>
    /// <param name="useRegular">通常形で計算するかどうか</param>
    /// <param name="useChiitoitsu">七対子形で計算するかどうか</param>
    /// <param name="useKokushi">国士無双形で計算するかどうか</param>
    /// <returns>通常形、七対子形、国士無双形の中で最も小さいシャンテン数</returns>
    public static int Calc(Span<int> counts, bool useRegular = true, bool useChiitoitsu = true, bool useKokushi = true)
    {
        var tileCount = ValidateCounts(counts);
        ValidateForms(useRegular, useChiitoitsu, useKokushi);

        var autoInferredMelds = (14 - tileCount) / 3;
        return CalcCore(counts, tileCount, autoInferredMelds, useRegular, useChiitoitsu, useKokushi);
    }

    /// <summary>
    /// 34 牌種の枚数配列から、副露相当の確定面子数を明示指定してシャンテン数を計算する。
    /// 計算中は配列を in-place で変更し、復帰時に元へ戻す。
    /// </summary>
    /// <param name="counts">34 要素の牌種別枚数配列</param>
    /// <param name="knownCallMeldCount">副露・暗槓などで既に確定済みの面子数 (0 以上)</param>
    /// <param name="useRegular">通常形で計算するかどうか</param>
    /// <param name="useChiitoitsu">七対子形で計算するかどうか</param>
    /// <param name="useKokushi">国士無双形で計算するかどうか</param>
    /// <returns>通常形、七対子形、国士無双形の中で最も小さいシャンテン数</returns>
    public static int Calc(Span<int> counts, int knownCallMeldCount, bool useRegular = true, bool useChiitoitsu = true, bool useKokushi = true)
    {
        if (knownCallMeldCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(knownCallMeldCount), knownCallMeldCount, "確定面子数は 0 以上である必要があります。");
        }

        var tileCount = ValidateCounts(counts);
        ValidateForms(useRegular, useChiitoitsu, useKokushi);
        return CalcCore(counts, tileCount, knownCallMeldCount, useRegular, useChiitoitsu, useKokushi);
    }

    private static int BuildCounts(TileKindList tileKindList, Span<int> counts)
    {
        if (tileKindList.Count > 14)
        {
            throw new ArgumentException($"手牌の数が14個より多いです。tileKindList:{tileKindList}", nameof(tileKindList));
        }

        foreach (var tileKind in tileKindList)
        {
            counts[tileKind.Value]++;
            if (counts[tileKind.Value] > 4)
            {
                throw new ArgumentException($"同じ牌種が5枚以上含まれています。tileKindList:{tileKindList}", nameof(tileKindList));
            }
        }

        return tileKindList.Count;
    }

    private static int ValidateCounts(ReadOnlySpan<int> counts)
    {
        if (counts.Length != TileKindCount)
        {
            throw new ArgumentException($"牌種別枚数配列の要素数は {TileKindCount} である必要があります。counts.Length:{counts.Length}", nameof(counts));
        }

        var totalCount = 0;
        for (var i = 0; i < counts.Length; i++)
        {
            var count = counts[i];
            if (count is < 0 or > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(counts), count, $"牌種別枚数は 0 から 4 の範囲である必要があります。index:{i}");
            }

            totalCount += count;
        }

        if (totalCount > 14)
        {
            throw new ArgumentException($"手牌の数が14個より多いです。totalCount:{totalCount}", nameof(counts));
        }

        return totalCount;
    }

    private static void ValidateForms(bool useRegular, bool useChiitoitsu, bool useKokushi)
    {
        if (!useRegular && !useChiitoitsu && !useKokushi)
        {
            throw new ArgumentException("最低でも1つの形を指定してください。");
        }
    }

    private static int CalcCore(Span<int> counts, int tileCount, int knownCallMeldCount, bool useRegular, bool useChiitoitsu, bool useKokushi)
    {
        var bestShanten = int.MaxValue;
        if (useRegular)
        {
            bestShanten = Math.Min(bestShanten, CalcForRegular(counts, tileCount, knownCallMeldCount));
        }
        if (useChiitoitsu)
        {
            bestShanten = Math.Min(bestShanten, CalcForChiitoitsu(counts));
        }
        if (useKokushi)
        {
            bestShanten = Math.Min(bestShanten, CalcForKokushimusou(counts));
        }

        return bestShanten;
    }

    /// <summary>
    /// 通常形（4面子1雀頭）のシャンテン数を、確定面子数を明示して計算する
    /// </summary>
    private static int CalcForRegular(Span<int> counts, int tileCount, int callMeldCount)
    {
        var mentsuCount = callMeldCount;
        var toitsuCount = 0;
        var tatsuCount = 0;
        var honorKantsuCount = 0;
        var isolationCount = 0;
        var isolatedOnlyFromNumberKantsu = true;
        var numberKantsuMask = 0;

        for (var i = 0; i < 27; i++)
        {
            if (counts[i] == 4)
            {
                numberKantsuMask |= 1 << i;
            }
        }

        for (var i = 27; i < TileKindCount; i++)
        {
            switch (counts[i])
            {
                case 1:
                    isolationCount++;
                    isolatedOnlyFromNumberKantsu = false;
                    break;
                case 2:
                    toitsuCount++;
                    break;
                case 3:
                    mentsuCount++;
                    break;
                case 4:
                    mentsuCount++;
                    honorKantsuCount++;
                    isolationCount++;
                    isolatedOnlyFromNumberKantsu = false;
                    break;
            }
        }

        if (honorKantsuCount != 0 && tileCount % 3 == 2)
        {
            honorKantsuCount--;
        }

        var bestShanten = int.MaxValue;
        ScanNumber(counts, 0, mentsuCount, toitsuCount, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
        return bestShanten;
    }

    /// <summary>
    /// 七対子形のシャンテン数を計算する
    /// </summary>
    /// <param name="counts">シャンテン数を計算する牌種別枚数配列</param>
    /// <returns>七対子形のシャンテン数</returns>
    private static int CalcForChiitoitsu(ReadOnlySpan<int> counts)
    {
        var toitsuCount = 0;
        var kindCount = 0;
        for (var i = 0; i < counts.Length; i++)
        {
            if (counts[i] == 0)
            {
                continue;
            }

            kindCount++;
            if (counts[i] >= 2)
            {
                toitsuCount++;
            }
        }

        return 6 - toitsuCount + Math.Max(0, 7 - kindCount);
    }

    /// <summary>
    /// 国士無双形のシャンテン数を計算する
    /// </summary>
    /// <param name="counts">シャンテン数を計算する牌種別枚数配列</param>
    /// <returns>国士無双形のシャンテン数</returns>
    private static int CalcForKokushimusou(ReadOnlySpan<int> counts)
    {
        var yaochuuCount = 0;
        var hasYaochuuToitsu = false;
        for (var i = 0; i < counts.Length; i++)
        {
            if (!IsYaochuIndex(i) || counts[i] == 0)
            {
                continue;
            }

            yaochuuCount++;
            if (counts[i] >= 2)
            {
                hasYaochuuToitsu = true;
            }
        }

        return 13 - yaochuuCount - (hasYaochuuToitsu ? 1 : 0);
    }

    private static bool IsYaochuIndex(int index)
    {
        return index is 0 or 8 or 9 or 17 or 18 or 26 or >= 27 and <= 33;
    }

    private static void ScanNumber(
        Span<int> counts,
        int current,
        int mentsuCount,
        int toitsuCount,
        int tatsuCount,
        int honorKantsuCount,
        int isolationCount,
        bool isolatedOnlyFromNumberKantsu,
        int numberKantsuMask,
        ref int bestShanten)
    {
        while (current < 27 && counts[current] == 0)
        {
            current++;
        }

        if (current >= 27)
        {
            UpdateBestShanten(mentsuCount, toitsuCount, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
            return;
        }

        switch (counts[current])
        {
            case 1:
                ScanNumber1(counts, current, mentsuCount, toitsuCount, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
                break;
            case 2:
                ScanNumber2(counts, current, mentsuCount, toitsuCount, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
                break;
            case 3:
                ScanNumber3(counts, current, mentsuCount, toitsuCount, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
                break;
            default:
                ScanNumber4(counts, current, mentsuCount, toitsuCount, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
                break;
        }
    }

    private static void ScanNumber1(
        Span<int> counts,
        int current,
        int mentsuCount,
        int toitsuCount,
        int tatsuCount,
        int honorKantsuCount,
        int isolationCount,
        bool isolatedOnlyFromNumberKantsu,
        int numberKantsuMask,
        ref int bestShanten)
    {
        var offset = current % 9;
        if (offset <= 5 && counts[current + 1] == 1 && counts[current + 2] > 0 && counts[current + 3] != 4)
        {
            counts[current]--;
            counts[current + 1]--;
            counts[current + 2]--;
            ScanNumber(counts, current, mentsuCount + 1, toitsuCount, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
            counts[current]++;
            counts[current + 1]++;
            counts[current + 2]++;
            return;
        }

        counts[current]--;
        ScanNumber(counts, current, mentsuCount, toitsuCount, tatsuCount, honorKantsuCount, isolationCount + 1, isolatedOnlyFromNumberKantsu && IsNumberKantsuIndex(numberKantsuMask, current), numberKantsuMask, ref bestShanten);
        counts[current]++;

        if (offset <= 6 && counts[current + 2] > 0)
        {
            if (counts[current + 1] > 0)
            {
                counts[current]--;
                counts[current + 1]--;
                counts[current + 2]--;
                ScanNumber(counts, current, mentsuCount + 1, toitsuCount, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
                counts[current]++;
                counts[current + 1]++;
                counts[current + 2]++;
            }

            counts[current]--;
            counts[current + 2]--;
            ScanNumber(counts, current, mentsuCount, toitsuCount, tatsuCount + 1, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
            counts[current]++;
            counts[current + 2]++;
        }

        if (offset <= 7 && counts[current + 1] > 0)
        {
            counts[current]--;
            counts[current + 1]--;
            ScanNumber(counts, current, mentsuCount, toitsuCount, tatsuCount + 1, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
            counts[current]++;
            counts[current + 1]++;
        }
    }

    private static void ScanNumber2(
        Span<int> counts,
        int current,
        int mentsuCount,
        int toitsuCount,
        int tatsuCount,
        int honorKantsuCount,
        int isolationCount,
        bool isolatedOnlyFromNumberKantsu,
        int numberKantsuMask,
        ref int bestShanten)
    {
        counts[current] -= 2;
        ScanNumber(counts, current, mentsuCount, toitsuCount + 1, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
        counts[current] += 2;

        var offset = current % 9;
        if (offset <= 6 && counts[current + 1] > 0 && counts[current + 2] > 0)
        {
            counts[current]--;
            counts[current + 1]--;
            counts[current + 2]--;
            ScanNumber(counts, current, mentsuCount + 1, toitsuCount, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
            counts[current]++;
            counts[current + 1]++;
            counts[current + 2]++;
        }
    }

    private static void ScanNumber3(
        Span<int> counts,
        int current,
        int mentsuCount,
        int toitsuCount,
        int tatsuCount,
        int honorKantsuCount,
        int isolationCount,
        bool isolatedOnlyFromNumberKantsu,
        int numberKantsuMask,
        ref int bestShanten)
    {
        counts[current] -= 3;
        ScanNumber(counts, current, mentsuCount + 1, toitsuCount, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
        counts[current] += 3;

        counts[current] -= 2;
        var offset = current % 9;
        if (offset <= 6 && counts[current + 1] > 0 && counts[current + 2] > 0)
        {
            counts[current]--;
            counts[current + 1]--;
            counts[current + 2]--;
            ScanNumber(counts, current, mentsuCount + 1, toitsuCount + 1, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
            counts[current]++;
            counts[current + 1]++;
            counts[current + 2]++;
        }
        else
        {
            if (offset <= 6 && counts[current + 2] > 0)
            {
                counts[current]--;
                counts[current + 2]--;
                ScanNumber(counts, current, mentsuCount, toitsuCount + 1, tatsuCount + 1, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
                counts[current]++;
                counts[current + 2]++;
            }

            if (offset <= 7 && counts[current + 1] > 0)
            {
                counts[current]--;
                counts[current + 1]--;
                ScanNumber(counts, current, mentsuCount, toitsuCount + 1, tatsuCount + 1, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
                counts[current]++;
                counts[current + 1]++;
            }
        }
        counts[current] += 2;

        if (offset <= 6 && counts[current + 1] >= 2 && counts[current + 2] >= 2)
        {
            counts[current] -= 2;
            counts[current + 1] -= 2;
            counts[current + 2] -= 2;
            ScanNumber(counts, current, mentsuCount + 2, toitsuCount, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
            counts[current] += 2;
            counts[current + 1] += 2;
            counts[current + 2] += 2;
        }
    }

    private static void ScanNumber4(
        Span<int> counts,
        int current,
        int mentsuCount,
        int toitsuCount,
        int tatsuCount,
        int honorKantsuCount,
        int isolationCount,
        bool isolatedOnlyFromNumberKantsu,
        int numberKantsuMask,
        ref int bestShanten)
    {
        var offset = current % 9;

        counts[current] -= 3;
        if (offset <= 6 && counts[current + 2] > 0)
        {
            if (counts[current + 1] > 0)
            {
                counts[current]--;
                counts[current + 1]--;
                counts[current + 2]--;
                ScanNumber(counts, current, mentsuCount + 2, toitsuCount, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
                counts[current]++;
                counts[current + 1]++;
                counts[current + 2]++;
            }

            counts[current]--;
            counts[current + 2]--;
            ScanNumber(counts, current, mentsuCount + 1, toitsuCount, tatsuCount + 1, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
            counts[current]++;
            counts[current + 2]++;
        }

        if (offset <= 7 && counts[current + 1] > 0)
        {
            counts[current]--;
            counts[current + 1]--;
            ScanNumber(counts, current, mentsuCount + 1, toitsuCount, tatsuCount + 1, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
            counts[current]++;
            counts[current + 1]++;
        }

        counts[current]--;
        ScanNumber(counts, current, mentsuCount + 1, toitsuCount, tatsuCount, honorKantsuCount, isolationCount + 1, isolatedOnlyFromNumberKantsu && IsNumberKantsuIndex(numberKantsuMask, current), numberKantsuMask, ref bestShanten);
        counts[current]++;
        counts[current] += 3;

        counts[current] -= 2;
        if (offset <= 6 && counts[current + 2] > 0)
        {
            if (counts[current + 1] > 0)
            {
                counts[current]--;
                counts[current + 1]--;
                counts[current + 2]--;
                ScanNumber(counts, current, mentsuCount + 1, toitsuCount + 1, tatsuCount, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
                counts[current]++;
                counts[current + 1]++;
                counts[current + 2]++;
            }

            counts[current]--;
            counts[current + 2]--;
            ScanNumber(counts, current, mentsuCount, toitsuCount + 1, tatsuCount + 1, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
            counts[current]++;
            counts[current + 2]++;
        }

        if (offset <= 7 && counts[current + 1] > 0)
        {
            counts[current]--;
            counts[current + 1]--;
            ScanNumber(counts, current, mentsuCount, toitsuCount + 1, tatsuCount + 1, honorKantsuCount, isolationCount, isolatedOnlyFromNumberKantsu, numberKantsuMask, ref bestShanten);
            counts[current]++;
            counts[current + 1]++;
        }
        counts[current] += 2;
    }

    private static bool IsNumberKantsuIndex(int numberKantsuMask, int index)
    {
        return (numberKantsuMask & (1 << index)) != 0;
    }

    private static void UpdateBestShanten(
        int mentsuCount,
        int toitsuCount,
        int tatsuCount,
        int honorKantsuCount,
        int isolationCount,
        bool isolatedOnlyFromNumberKantsu,
        int numberKantsuMask,
        ref int bestShanten)
    {
        var shanten = 8 - mentsuCount * 2 - toitsuCount - tatsuCount;
        var mentsuKouho = mentsuCount + tatsuCount;
        if (toitsuCount != 0)
        {
            mentsuKouho += toitsuCount - 1;
        }
        else if (isolationCount > 0 && numberKantsuMask != 0 && isolatedOnlyFromNumberKantsu)
        {
            // 同種の数牌4枚を刻子+孤立牌として扱っただけの形は単騎待ち完成形とみなさない。
            shanten++;
        }

        if (mentsuKouho > 4)
        {
            shanten += mentsuKouho - 4;
        }

        if (shanten != ShantenConstants.SHANTEN_AGARI && shanten < honorKantsuCount)
        {
            shanten = honorKantsuCount;
        }

        if (shanten < bestShanten)
        {
            bestShanten = shanten;
        }
    }
}
