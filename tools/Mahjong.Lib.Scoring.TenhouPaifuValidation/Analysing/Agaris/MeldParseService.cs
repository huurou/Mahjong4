using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;
using Microsoft.Extensions.Logging;

namespace Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Agaris;

/// <summary>
/// 天鳳の面子コードを解析するサービスクラス
/// </summary>
public partial class MeldParseService(ILogger<MeldParseService> logger)
{
    /// <summary>
    /// 面子コードを解析してCallオブジェクトを生成する
    /// </summary>
    /// <param name="meldCode">面子コード</param>
    /// <returns>解析されたCallオブジェクト</returns>
    public Call Parse(int meldCode)
    {
        LogParseStart(logger, meldCode);

        // meldCode & 0x0003: 誰から鳴いたか 0-鳴きなし 1-下家 2-対面 3-上家
        // meldCode & 0x0004: !0-順子 0-刻子・槓子
        // meldCode & 0x0008: !0-刻子 0-槓子
        // meldCode & 0x0010: !0-加槓 0-加槓でない

        Call result;
        if ((meldCode & 0x0004) != 0)
        {
            LogChiDetected(logger, meldCode & 0x0004);
            result = ParseChi(meldCode);
        }
        else if ((meldCode & 0x0010) != 0)
        {
            // 加槓は bit3=1 かつ bit4=1。ポン判定より先に判定する必要がある
            LogKakanDetected(logger, meldCode & 0x0010);
            result = ParseKakan(meldCode);
        }
        else if ((meldCode & 0x0008) != 0)
        {
            LogPonDetected(logger, meldCode & 0x0008);
            result = ParsePon(meldCode);
        }
        else if ((meldCode & 0x0003) != 0)
        {
            LogDaiminkanDetected(logger, meldCode & 0x0003);
            result = ParseDaiminkan(meldCode);
        }
        else
        {
            LogAnkanDetected(logger);
            result = ParseAnkan(meldCode);
        }

        LogParseCompleted(logger, meldCode, result.Type);
        return result;
    }

    /// <summary>
    /// チー（順子）の面子コードを解析する
    /// </summary>
    /// <param name="meldCode">面子コード</param>
    /// <returns>チーのCallオブジェクト</returns>
    private Call ParseChi(int meldCode)
    {
        var pattern = (meldCode & 0xFC00) >> 10;
        LogChiPattern(logger, pattern);
        var suit = pattern / 3 / 7;
        var start = pattern / 3 % 7;
        var baseKindValue = suit * 9 + start;
        LogChiResult(logger, suit, start, baseKindValue);
        return new Call(CallType.Chi, [TileKind.All[baseKindValue], TileKind.All[baseKindValue + 1], TileKind.All[baseKindValue + 2]]);
    }

    /// <summary>
    /// ポン（刻子）の面子コードを解析する
    /// </summary>
    /// <param name="meldCode">面子コード</param>
    /// <returns>ポンのCallオブジェクト</returns>
    private Call ParsePon(int meldCode)
    {
        var pattern = (meldCode & 0xFE00) >> 9;
        LogPonPattern(logger, pattern);
        var kindValue = pattern / 3;
        LogPonResult(logger, kindValue);
        return new Call(CallType.Pon, [.. Enumerable.Repeat(TileKind.All[kindValue], 3)]);
    }

    /// <summary>
    /// 加槓の面子コードを解析する
    /// </summary>
    /// <param name="meldCode">面子コード</param>
    /// <returns>加槓のCallオブジェクト</returns>
    private Call ParseKakan(int meldCode)
    {
        var pattern = (meldCode & 0xFE00) >> 9;
        LogKakanPattern(logger, pattern);
        var kindValue = pattern / 3;
        LogKakanResult(logger, kindValue);
        return new Call(CallType.Minkan, [.. Enumerable.Repeat(TileKind.All[kindValue], 4)]);
    }

    /// <summary>
    /// 大明槓の面子コードを解析する
    /// </summary>
    /// <param name="meldCode">面子コード</param>
    /// <returns>大明槓のCallオブジェクト</returns>
    private Call ParseDaiminkan(int meldCode)
    {
        var pattern = (meldCode & 0xFF00) >> 8;
        LogDaiminkanPattern(logger, pattern);
        var kindValue = pattern / 4;
        LogDaiminkanResult(logger, kindValue);
        return new Call(CallType.Minkan, [.. Enumerable.Repeat(TileKind.All[kindValue], 4)]);
    }

    /// <summary>
    /// 暗槓の面子コードを解析する
    /// </summary>
    /// <param name="meldCode">面子コード</param>
    /// <returns>暗槓のCallオブジェクト</returns>
    private Call ParseAnkan(int meldCode)
    {
        var pattern = (meldCode & 0xFF00) >> 8;
        LogAnkanPattern(logger, pattern);
        var kindValue = pattern / 4;
        LogAnkanResult(logger, kindValue);
        return new Call(CallType.Ankan, [.. Enumerable.Repeat(TileKind.All[kindValue], 4)]);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "面子コード解析開始: {MeldCode}")]
    private static partial void LogParseStart(ILogger logger, int meldCode);

    [LoggerMessage(Level = LogLevel.Debug, Message = "面子コード解析完了: {MeldCode} -> {CallType}")]
    private static partial void LogParseCompleted(ILogger logger, int meldCode, CallType callType);

    [LoggerMessage(Level = LogLevel.Trace, Message = "チー判定: meldCode & 0x0004 = {Value}")]
    private static partial void LogChiDetected(ILogger logger, int value);

    [LoggerMessage(Level = LogLevel.Trace, Message = "ポン判定: meldCode & 0x0008 = {Value}")]
    private static partial void LogPonDetected(ILogger logger, int value);

    [LoggerMessage(Level = LogLevel.Trace, Message = "加槓判定: meldCode & 0x0010 = {Value}")]
    private static partial void LogKakanDetected(ILogger logger, int value);

    [LoggerMessage(Level = LogLevel.Trace, Message = "大明槓判定: meldCode & 0x0003 = {Value}")]
    private static partial void LogDaiminkanDetected(ILogger logger, int value);

    [LoggerMessage(Level = LogLevel.Trace, Message = "暗槓判定")]
    private static partial void LogAnkanDetected(ILogger logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "チー解析: pattern={Pattern}")]
    private static partial void LogChiPattern(ILogger logger, int pattern);

    [LoggerMessage(Level = LogLevel.Debug, Message = "チー解析完了: suit={Suit} start={Start} baseKind={BaseKind}")]
    private static partial void LogChiResult(ILogger logger, int suit, int start, int baseKind);

    [LoggerMessage(Level = LogLevel.Trace, Message = "ポン解析: pattern={Pattern}")]
    private static partial void LogPonPattern(ILogger logger, int pattern);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ポン解析完了: kindValue={KindValue}")]
    private static partial void LogPonResult(ILogger logger, int kindValue);

    [LoggerMessage(Level = LogLevel.Trace, Message = "加槓解析: pattern={Pattern}")]
    private static partial void LogKakanPattern(ILogger logger, int pattern);

    [LoggerMessage(Level = LogLevel.Debug, Message = "加槓解析完了: kindValue={KindValue}")]
    private static partial void LogKakanResult(ILogger logger, int kindValue);

    [LoggerMessage(Level = LogLevel.Trace, Message = "大明槓解析: pattern={Pattern}")]
    private static partial void LogDaiminkanPattern(ILogger logger, int pattern);

    [LoggerMessage(Level = LogLevel.Debug, Message = "大明槓解析完了: kindValue={KindValue}")]
    private static partial void LogDaiminkanResult(ILogger logger, int kindValue);

    [LoggerMessage(Level = LogLevel.Trace, Message = "暗槓解析: pattern={Pattern}")]
    private static partial void LogAnkanPattern(ILogger logger, int pattern);

    [LoggerMessage(Level = LogLevel.Debug, Message = "暗槓解析完了: kindValue={KindValue}")]
    private static partial void LogAnkanResult(ILogger logger, int kindValue);
}
