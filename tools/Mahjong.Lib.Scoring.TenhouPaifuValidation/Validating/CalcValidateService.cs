using Mahjong.Lib.Scoring.Games;
using Mahjong.Lib.Scoring.HandCalculating;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.AgariInfos;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Agaris;
using Mahjong.Lib.Scoring.Yakus;
using Microsoft.Extensions.Logging;

namespace Mahjong.Lib.Scoring.TenhouPaifuValidation.Validating;

/// <summary>
/// <see cref="HandCalculator"/> の計算結果を牌譜の期待値と突き合わせて検証するサービス
/// </summary>
public partial class CalcValidateService(ILogger<CalcValidateService> logger)
{
    /// <summary>
    /// 和了情報を <see cref="HandCalculator"/> にかけ、符・翻・点数・役リストを牌譜の期待値と比較します。
    /// </summary>
    /// <param name="agariInfo">検証対象の和了情報</param>
    /// <returns>検証結果（一致/不一致 と計算結果）</returns>
    public ValidateResult Validate(AgariInfo agariInfo)
    {
        // 天鳳の標準ルール（喰タン・赤ドラあり）
        var gameRules = new GameRules
        {
            KuitanEnabled = true,
            DoubleYakumanEnabled = false,
            KazoeLimit = KazoeLimit.Limited,
            KiriageEnabled = false,
            PinzumoEnabled = true,
            RenhouAsYakumanEnabled = true,
            DaisharinEnabled = false,
        };
        var handResult = HandCalculator.Calc(agariInfo.TileKindList, agariInfo.WinTile, agariInfo.CallList, agariInfo.DoraIndicators, agariInfo.UradoraIndicators, agariInfo.WinSituation, gameRules);
        var isSuccess = true;
        // 牌譜には満貫以上の和了は30符で記載される模様
        if (agariInfo.ManganType == ManganType.None && handResult.Fu != agariInfo.Fu)
        {
            LogFuMismatch(logger, handResult.Fu, agariInfo.Fu);
            isSuccess = false;
        }
        if (handResult.Han != agariInfo.Han)
        {
            LogHanMismatch(logger, handResult.Han, agariInfo.Han);
            isSuccess = false;
        }
        var totalScore =
            agariInfo.WinSituation.IsTsumo && agariInfo.WinSituation.IsDealer ? handResult.Score.Main * 3
            : agariInfo.WinSituation.IsTsumo ? handResult.Score.Main + handResult.Score.Sub * 2
            : handResult.Score.Main;
        if (totalScore != agariInfo.TotalScore)
        {
            LogScoreMismatch(logger, totalScore, agariInfo.TotalScore);
            isSuccess = false;
        }
        if (handResult.YakuList != agariInfo.YakuList)
        {
            LogYakuListMismatch(logger, handResult.YakuList, agariInfo.YakuList);
            isSuccess = false;
        }
        return new ValidateResult(isSuccess, agariInfo, handResult);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "符の不一致: 計算結果 {Calculated}, 期待値 {Expected}")]
    private static partial void LogFuMismatch(ILogger logger, int calculated, int expected);

    [LoggerMessage(Level = LogLevel.Information, Message = "翻の不一致: 計算結果 {Calculated}, 期待値 {Expected}")]
    private static partial void LogHanMismatch(ILogger logger, int calculated, int expected);

    [LoggerMessage(Level = LogLevel.Information, Message = "点数の不一致: 計算結果 {Calculated}, 期待値 {Expected}")]
    private static partial void LogScoreMismatch(ILogger logger, int calculated, int expected);

    [LoggerMessage(Level = LogLevel.Information, Message = "役の不一致: 計算結果 [{Calculated}], 期待値 [{Expected}]")]
    private static partial void LogYakuListMismatch(ILogger logger, YakuList calculated, YakuList expected);
}
