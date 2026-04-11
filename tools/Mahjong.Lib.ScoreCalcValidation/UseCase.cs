using Mahjong.Lib.ScoreCalcValidation.Analysing.AgariInfos;
using Mahjong.Lib.ScoreCalcValidation.Analysing.Agaris;
using Mahjong.Lib.ScoreCalcValidation.Analysing.Inits;
using Mahjong.Lib.ScoreCalcValidation.Analysing.Rounds;
using Mahjong.Lib.ScoreCalcValidation.Downloads;
using Mahjong.Lib.ScoreCalcValidation.Validating;
using Microsoft.Extensions.Logging;

namespace Mahjong.Lib.ScoreCalcValidation;

/// <summary>
/// 牌譜解析・点数計算検証のユースケースを提供するクラス
/// </summary>
public partial class UseCase(
    PaifuDownloadService paifuDownloadService,
    RoundDataExtractService roundDataExtractService,
    InitParseService initParseService,
    AgariParseService agariParseService,
    CalcValidateService calcValidateService,
    ILogger<UseCase> logger
)
{
    /// <summary>
    /// 指定日付の牌譜をダウンロード・解析し、全和了情報のリストを返します。
    /// </summary>
    /// <param name="logDate">対象日付（YYYYMMDD形式）</param>
    /// <returns>解析された和了情報のリスト</returns>
    public async Task<List<AgariInfo>> AnalysisPaifu(string logDate)
    {
        LogAnalysisStart(logger, logDate);

        var paifus = await paifuDownloadService.DownloadAsync(logDate);
        LogPaifuDownloadCompleted(logger, paifus.Count);

        var roundDatas = new List<RoundData>();
        foreach (var paifu in paifus)
        {
            var roundData = roundDataExtractService.Extract(paifu);
            roundDatas.AddRange(roundData);
        }
        LogRoundExtractCompleted(logger, roundDatas.Count);

        var agariInfos = new List<AgariInfo>();
        foreach (var roundData in roundDatas)
        {
            var init = initParseService.Parse(roundData.InitTag);
            foreach (var agariTag in roundData.AgariTags)
            {
                var agari = agariParseService.Parse(agariTag);
                agariInfos.Add(AgariInfoBuildService.Build(roundData.GameId, init, agari));
            }
        }

        LogAnalysisCompleted(logger, agariInfos.Count);

        return agariInfos;
    }

    /// <summary>
    /// 和了情報に対して点数計算の妥当性を検証します。
    /// </summary>
    /// <param name="agariInfo">検証対象の和了情報</param>
    /// <returns>検証結果</returns>
    public ValidateResult ValidateCalc(AgariInfo agariInfo)
    {
        LogValidateStart(logger, agariInfo.GameId);

        var result = calcValidateService.Validate(agariInfo);

        LogValidateCompleted(logger, agariInfo.GameId, result);

        return result;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "牌譜分析開始: {Date}")]
    private static partial void LogAnalysisStart(ILogger logger, string date);

    [LoggerMessage(Level = LogLevel.Information, Message = "牌譜ダウンロード完了: {PaifuCount}件")]
    private static partial void LogPaifuDownloadCompleted(ILogger logger, int paifuCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "ラウンドデータ抽出完了: {RoundCount}ラウンド")]
    private static partial void LogRoundExtractCompleted(ILogger logger, int roundCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "牌譜分析完了: {AgariCount}個のアガリ情報を生成")]
    private static partial void LogAnalysisCompleted(ILogger logger, int agariCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "点数計算検証開始: GameId={GameId}")]
    private static partial void LogValidateStart(ILogger logger, string gameId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "点数計算検証完了: GameId={GameId}, Result={Result}")]
    private static partial void LogValidateCompleted(ILogger logger, string gameId, ValidateResult result);
}
