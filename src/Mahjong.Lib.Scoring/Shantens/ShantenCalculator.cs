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
        if (tileKindList.Count > 14)
        {
            throw new ArgumentException($"手牌の数が14個より多いです。tileKindList:{tileKindList}", nameof(tileKindList));
        }
        if (TileKind.All.Any(x => tileKindList.CountOf(x) > 4))
        {
            throw new ArgumentException($"同じ牌種が5枚以上含まれています。tileKindList:{tileKindList}", nameof(tileKindList));
        }
        if (!useRegular && !useChiitoitsu && !useKokushi)
        {
            throw new ArgumentException("最低でも1つの形を指定してください。");
        }

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
    /// 通常形（4面子1雀頭）のシャンテン数を計算する
    /// </summary>
    /// <param name="tileKindList">シャンテン数を計算する牌種別リスト</param>
    /// <returns>通常形のシャンテン数</returns>
    private static int CalcForRegular(TileKindList tileKindList)
    {
        var context = new ShantenContext(tileKindList);
        context = context.ScanHonor();
        context = context with
        {
            // 14枚から不足している3枚組を、既に確定済みの面子数として加算する（副露・暗槓相当）
            MentsuCount = context.MentsuCount + (14 - context.TileKindList.Count) / 3,
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
