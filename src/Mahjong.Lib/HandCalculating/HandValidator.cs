using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Shantens;
using Mahjong.Lib.Tiles;
using System.Diagnostics.CodeAnalysis;

namespace Mahjong.Lib.HandCalculating;

/// <summary>
/// 手牌と和了状況のバリデーションを行います
/// </summary>
internal static class HandValidator
{
    /// <summary>
    /// エラー判定
    /// エラーがあればHandResultを返す。
    /// </summary>
    internal static bool HasError(TileKindList tileKindList, TileKind? winTile, CallList callList, WinSituation winSituation, [NotNullWhen(true)] out HandResult? handResult)
    {
        handResult = null;

        // 流し満貫は手牌形を問わないので特殊役バリデーションのみ実施する
        if (winSituation.IsNagashimangan)
        {
            return HasSpecialYakuError(callList, winSituation, winTile, out handResult);
        }

        // 副露・手牌の枚数整合性チェック
        if (HasTileCountError(tileKindList, callList, out handResult))
        {
            return true;
        }

        // 手牌がアガリ形かチェック
        if (!IsValidWinningHand(tileKindList))
        {
            handResult = HandResult.Error("手牌がアガリ形ではありません。");
            return true;
        }

        // 和了牌の整合性チェック
        if (HasWinTileError(tileKindList, winTile, winSituation, out handResult))
        {
            return true;
        }

        // 立直・一発の整合性チェック
        if (HasRiichiError(callList, winSituation, out handResult))
        {
            return true;
        }

        // ツモ・ロンの整合性チェック
        if (HasTsumoRonError(winSituation, out handResult))
        {
            return true;
        }

        // 特殊役の整合性チェック
        if (HasSpecialYakuError(callList, winSituation, winTile, out handResult))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 副露と手牌の枚数整合性をチェックします
    /// </summary>
    private static bool HasTileCountError(TileKindList tileKindList, CallList callList, [NotNullWhen(true)] out HandResult? handResult)
    {
        handResult = null;

        // 抜き(Nuki)は面子ではなく1枚の抜き牌扱い。上限・枚数計算からは除外する
        var mentsuCallCount = callList.Count(x => !x.IsNuki);
        if (mentsuCallCount > 4)
        {
            handResult = HandResult.Error("副露は4つまでしか指定できません。");
            return true;
        }

        // 副露分(Nuki除く)を除いた手牌は (14 - 3 * 面子副露数) 枚になる（槓も1面子=3枚分として扱う）
        var expectedHandCount = 14 - 3 * mentsuCallCount;
        if (tileKindList.Count != expectedHandCount)
        {
            handResult = HandResult.Error($"手牌の枚数が不正です。期待値: {expectedHandCount}, 実際: {tileKindList.Count}");
            return true;
        }

        // 同種の牌が5枚以上含まれていないかチェック（槓を含む副露分も合算）
        var counts = new Dictionary<TileKind, int>();
        foreach (var tile in tileKindList)
        {
            counts[tile] = counts.GetValueOrDefault(tile, 0) + 1;
        }
        foreach (var call in callList)
        {
            foreach (var tile in call.TileKindList)
            {
                counts[tile] = counts.GetValueOrDefault(tile, 0) + 1;
            }
        }
        if (counts.Any(x => x.Value > 4))
        {
            handResult = HandResult.Error("同種の牌が4枚を超えています。");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 手牌がアガリ形かどうかを判定します
    /// </summary>
    private static bool IsValidWinningHand(TileKindList tileKindList)
    {
        return ShantenCalculator.Calc(tileKindList) == ShantenConstants.SHANTEN_AGARI;
    }

    /// <summary>
    /// 和了牌に関するエラーをチェックします
    /// </summary>
    private static bool HasWinTileError(TileKindList tileKindList, TileKind? winTile, WinSituation winSituation, [NotNullWhen(true)] out HandResult? handResult)
    {
        handResult = null;

        // 天和以外で和了牌がnullの場合はエラー
        if (!winSituation.IsTenhou && winTile is null)
        {
            handResult = HandResult.Error("和了牌が指定されていません。");
            return true;
        }

        if (!winSituation.IsTenhou && winTile is not null && !tileKindList.Contains(winTile))
        {
            handResult = HandResult.Error("手牌にアガリ牌がありません。");
            return true;
        }

        if (winSituation.IsTenhou && winTile is not null)
        {
            handResult = HandResult.Error("天和の時は和了牌にnullを指定してください。");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 立直・一発に関するエラーをチェックします
    /// </summary>
    private static bool HasRiichiError(CallList callList, WinSituation winSituation, [NotNullWhen(true)] out HandResult? handResult)
    {
        handResult = null;

        if (winSituation.IsIppatsu && callList.HasOpen)
        {
            handResult = HandResult.Error("一発と非面前は両立できません。");
            return true;
        }

        if (winSituation.IsRiichi && callList.HasOpen)
        {
            handResult = HandResult.Error("立直と非面前は両立できません。");
            return true;
        }

        if (winSituation.IsDoubleRiichi && callList.HasOpen)
        {
            handResult = HandResult.Error("ダブル立直と非面前は両立できません。");
            return true;
        }

        if (winSituation.IsIppatsu && !winSituation.IsRiichi && !winSituation.IsDoubleRiichi)
        {
            handResult = HandResult.Error("一発は立直orダブル立直時にしか成立しません。");
            return true;
        }

        return false;
    }

    /// <summary>
    /// ツモ・ロンに関するエラーをチェックします
    /// </summary>
    private static bool HasTsumoRonError(WinSituation winSituation, [NotNullWhen(true)] out HandResult? handResult)
    {
        handResult = null;

        if (winSituation.IsChankan && winSituation.IsTsumo)
        {
            handResult = HandResult.Error("槍槓とツモアガリは両立できません。");
            return true;
        }

        if (winSituation.IsRinshan && !winSituation.IsTsumo)
        {
            handResult = HandResult.Error("嶺上開花とロンアガリは両立できません。");
            return true;
        }

        if (winSituation.IsHaitei && !winSituation.IsTsumo)
        {
            handResult = HandResult.Error("海底撈月とロンアガリは両立できません。");
            return true;
        }

        if (winSituation.IsHoutei && winSituation.IsTsumo)
        {
            handResult = HandResult.Error("河底撈魚とツモアガリは両立できません。");
            return true;
        }

        if (winSituation.IsHaitei && winSituation.IsRinshan)
        {
            handResult = HandResult.Error("海底撈月と嶺上開花は両立できません。");
            return true;
        }

        if (winSituation.IsHoutei && winSituation.IsChankan)
        {
            handResult = HandResult.Error("河底撈魚と槍槓は両立できません。");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 特殊役（天和・地和・人和）に関するエラーをチェックします
    /// </summary>
    private static bool HasSpecialYakuError(CallList callList, WinSituation winSituation, TileKind? winTile, [NotNullWhen(true)] out HandResult? handResult)
    {
        handResult = null;

        // 天和のエラーチェック
        if (HasTenhouError(callList, winSituation, out handResult))
        {
            return true;
        }

        // 地和のエラーチェック
        if (HasChiihouError(callList, winSituation, out handResult))
        {
            return true;
        }

        // 人和のエラーチェック
        if (HasRenhouError(callList, winSituation, out handResult))
        {
            return true;
        }

        // 流し満貫のエラーチェック
        if (winSituation.IsNagashimangan && winTile is not null)
        {
            handResult = HandResult.Error("流し満貫の時は和了牌にnullを指定してください。");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 天和に関するエラーをチェックします
    /// </summary>
    private static bool HasTenhouError(CallList callList, WinSituation winSituation, [NotNullWhen(true)] out HandResult? handResult)
    {
        handResult = null;

        if (winSituation.IsTenhou && !winSituation.IsDealer)
        {
            handResult = HandResult.Error("天和はプレイヤーが親の時のみ有効です。");
            return true;
        }

        if (winSituation.IsTenhou && !winSituation.IsTsumo)
        {
            handResult = HandResult.Error("天和とロンアガリは両立できません。");
            return true;
        }

        if (winSituation.IsTenhou && callList.Count != 0)
        {
            handResult = HandResult.Error("副露を伴う天和は無効です。");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 地和に関するエラーをチェックします
    /// </summary>
    private static bool HasChiihouError(CallList callList, WinSituation winSituation, [NotNullWhen(true)] out HandResult? handResult)
    {
        handResult = null;

        if (winSituation.IsChiihou && winSituation.IsDealer)
        {
            handResult = HandResult.Error("地和はプレイヤーが子の時のみ有効です。");
            return true;
        }

        if (winSituation.IsChiihou && !winSituation.IsTsumo)
        {
            handResult = HandResult.Error("地和とロンアガリは両立できません。");
            return true;
        }

        if (winSituation.IsChiihou && callList.Count != 0)
        {
            handResult = HandResult.Error("副露を伴う地和は無効です。");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 人和に関するエラーをチェックします
    /// </summary>
    private static bool HasRenhouError(CallList callList, WinSituation winSituation, [NotNullWhen(true)] out HandResult? handResult)
    {
        handResult = null;

        if (winSituation.IsRenhou && winSituation.IsDealer)
        {
            handResult = HandResult.Error("人和はプレイヤーが子の時のみ有効です。");
            return true;
        }

        if (winSituation.IsRenhou && winSituation.IsTsumo)
        {
            handResult = HandResult.Error("人和とロンアガリは両立できません。");
            return true;
        }

        if (winSituation.IsRenhou && callList.Count != 0)
        {
            handResult = HandResult.Error("副露を伴う人和は無効です。");
            return true;
        }

        return false;
    }
}
