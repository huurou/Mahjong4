using Mahjong.Lib.Scoring.Yakus;
using Mahjong.Lib.Scoring.Yakus.Impl;

namespace Mahjong.Lib.Game.Games;

/// <summary>
/// 役リストに対する包 (責任払い) 対象役満判定
/// 包対象は大三元 / 大四喜 (Daisuushii 単役および DaisuushiiDouble) / 四槓子 のみ
/// </summary>
internal static class YakuPaoExtensions
{
    public static bool HasPaoEligibleYaku(this YakuList yakuList)
    {
        return yakuList.Any(x => x is Daisangen or Daisuushii or DaisuushiiDouble or Suukantsu);
    }
}
