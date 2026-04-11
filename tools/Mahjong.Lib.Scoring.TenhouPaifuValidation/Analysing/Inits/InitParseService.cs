using Mahjong.Lib.Scoring.Games;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Inits;

/// <summary>
/// 天鳳牌譜の INIT ノードを解析して <see cref="Init"/> に変換するサービス
/// </summary>
public partial class InitParseService(ILogger<InitParseService> logger)
{
    /// <summary>
    /// INIT タグ文字列を解析して <see cref="Init"/> を生成します。
    /// </summary>
    /// <param name="initTag">解析対象の INIT タグ文字列</param>
    /// <returns>解析された局開始情報</returns>
    public Init Parse(string initTag)
    {
        LogParseStart(logger, initTag);

        // 解析しやすくするため改行を全てスペースに置換する
        initTag = initTag.Replace("\r\n", " ").Replace("\n", " ");
        LogLineBreakReplaced(logger, initTag);

        var seedGroups = SeedRegex().Match(initTag).Groups;
        var kyokuString = seedGroups["kyoku"].Value;
        LogKyokuString(logger, kyokuString);
        var kyoku = int.Parse(kyokuString);

        var roundWind = (Wind)(kyoku / 4);
        LogRoundWindParsed(logger, roundWind);

        var honbaString = seedGroups["honba"].Value;
        LogHonbaString(logger, honbaString);
        var honba = int.Parse(honbaString);

        var oyaString = OyaRegex().Match(initTag).Groups["oya"].Value;
        LogOyaString(logger, oyaString);
        var oya = int.Parse(oyaString);
        LogOyaParsed(logger, oya);

        var result = new Init(kyoku, honba, roundWind, oya);
        LogParseCompleted(logger, roundWind, oya);

        return result;
    }

    // 局情報
    [GeneratedRegex(@"seed=""(?<kyoku>\d+),(?<honba>\d+),.*?""")]
    private partial Regex SeedRegex();

    // 親番
    [GeneratedRegex(@"oya=""(?<oya>\d)""")]
    private partial Regex OyaRegex();

    [LoggerMessage(Level = LogLevel.Trace, Message = "局開始解析開始: {InitTag}")]
    private static partial void LogParseStart(ILogger logger, string initTag);

    [LoggerMessage(Level = LogLevel.Trace, Message = "改行置換後: {InitTag}")]
    private static partial void LogLineBreakReplaced(ILogger logger, string initTag);

    [LoggerMessage(Level = LogLevel.Trace, Message = "局順文字列: {KyokuString}")]
    private static partial void LogKyokuString(ILogger logger, string kyokuString);

    [LoggerMessage(Level = LogLevel.Trace, Message = "場風解析完了: {RoundWind}")]
    private static partial void LogRoundWindParsed(ILogger logger, Wind roundWind);

    [LoggerMessage(Level = LogLevel.Trace, Message = "本場文字列: {HonbaString}")]
    private static partial void LogHonbaString(ILogger logger, string honbaString);

    [LoggerMessage(Level = LogLevel.Trace, Message = "親番文字列: {OyaString}")]
    private static partial void LogOyaString(ILogger logger, string oyaString);

    [LoggerMessage(Level = LogLevel.Trace, Message = "親番解析完了: プレイヤー{Oya}")]
    private static partial void LogOyaParsed(ILogger logger, int oya);

    [LoggerMessage(Level = LogLevel.Trace, Message = "局開始解析完了: RoundWind:{RoundWind} Oya:{Oya}")]
    private static partial void LogParseCompleted(ILogger logger, Wind roundWind, int oya);
}
