using Mahjong.Lib.HandCalculating;
using Mahjong.Lib.ScoreCalcValidation.Analysing.AgariInfos;

namespace Mahjong.Lib.ScoreCalcValidation.Validating;

/// <summary>
/// 点数計算検証の結果
/// </summary>
/// <param name="IsSuccess">計算結果と牌譜の期待値が全て一致した場合 true</param>
/// <param name="AgariInfo">検証対象の和了情報</param>
/// <param name="HandResult"><see cref="HandCalculator"/> の計算結果</param>
public record ValidateResult(bool IsSuccess, AgariInfo AgariInfo, HandResult HandResult);
