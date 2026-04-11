using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Mahjong.Lib.ScoreCalcValidation.Analysing.Agaris;

/// <summary>
/// 天鳳牌譜の AGARI ノードを解析して <see cref="Agari"/> オブジェクトに変換するサービス
/// </summary>
public partial class AgariParseService(MeldParseService meldParseService, ILogger<AgariParseService> logger)
{
    /// <summary>
    /// AGARI タグ文字列を解析して <see cref="Agari"/> を生成します。
    /// </summary>
    /// <param name="agariTag">解析対象の AGARI タグ文字列</param>
    /// <returns>解析された和了情報</returns>
    public Agari Parse(string agariTag)
    {
        LogParseStart(logger, agariTag);

        // 解析しやすくするため改行を全てスペースに置換する
        agariTag = agariTag.Replace("\r\n", " ").Replace("\n", " ");
        LogLineBreakReplaced(logger, agariTag);

        var handString = HaiRegex().Match(agariTag).Groups["hand"].Value;
        LogHandString(logger, handString);
        var hand = new TileKindList(handString.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => TileKind.All[int.Parse(x) / 4]));
        LogHandParsed(logger, hand.Count);

        var meldCodesString = MRegex().Match(agariTag).Groups["meldCodes"].Value;
        LogMeldCodesString(logger, meldCodesString);
        var meldCodes = meldCodesString.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
        var calls = new CallList(meldCodes.Select(meldParseService.Parse));
        LogCallsParsed(logger, calls.Count);

        var winTileString = MachiRegex().Match(agariTag).Groups["winTile"].Value;
        LogWinTileString(logger, winTileString);
        var winTile = TileKind.All[int.Parse(winTileString) / 4];
        LogWinTile(logger, winTile);

        var tenGroups = TenRegex().Match(agariTag).Groups;
        var fu = int.Parse(tenGroups["fu"].Value);
        var score = int.Parse(tenGroups["score"].Value);
        var manganType = (ManganType)int.Parse(tenGroups["manganType"].Value);
        LogScoreInfo(logger, fu, score, manganType);

        var yakuInfosString = YakuRegex().Match(agariTag).Groups["yakuInfos"].Value;
        LogYakuInfosString(logger, yakuInfosString);
        var yakuInfosArray = yakuInfosString.Split(",", StringSplitOptions.RemoveEmptyEntries);
        var yakuInfos = new List<YakuInfo>();
        for (var i = 0; i < yakuInfosArray.Length / 2; i++)
        {
            var yakuInfo = new YakuInfo(int.Parse(yakuInfosArray[i * 2]), int.Parse(yakuInfosArray[i * 2 + 1]));
            if (yakuInfo.Han != 0)
            {
                yakuInfos.Add(yakuInfo);
            }
        }
        yakuInfos = [.. yakuInfos.OrderBy(x => x.Number)];
        LogYakuParsed(logger, yakuInfos.Count);

        var yakumansString = YakumanRegex().Match(agariTag).Groups["yakumans"].Value;
        LogYakumansString(logger, yakumansString);
        var yakumans = new List<int>();
        foreach (var yakuman in yakumansString.Split(",", StringSplitOptions.RemoveEmptyEntries))
        {
            yakumans.Add(int.Parse(yakuman));
        }
        LogYakumanParsed(logger, yakumans.Count);

        var doraIndicatorsString = DoraHaiRegex().Match(agariTag).Groups["doraIndicators"].Value;
        LogDoraIndicatorsString(logger, doraIndicatorsString);
        var doraIndicators = new TileKindList(doraIndicatorsString.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => TileKind.All[int.Parse(x) / 4]));

        var uradoraIndicatorsString = DoraHaiUraRegex().Match(agariTag).Groups["uradoraIndicators"].Value;
        LogUradoraIndicatorsString(logger, uradoraIndicatorsString);
        var uradoraIndicators = new TileKindList(uradoraIndicatorsString.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => TileKind.All[int.Parse(x) / 4]));
        LogDoraParsed(logger, doraIndicators.Count, uradoraIndicators.Count);

        var whoString = WhoRegex().Match(agariTag).Groups["who"].Value;
        var fromWhoString = FromWhoRegex().Match(agariTag).Groups["fromWho"].Value;
        var isTsumo = whoString == fromWhoString;
        LogWhoInfo(logger, whoString, fromWhoString, isTsumo);

        var akadoraCount = yakuInfos.FirstOrDefault(x => x.Number == AKADORA_NUMBER)?.Han ?? 0;

        var who = int.Parse(whoString);

        var result = new Agari(hand, calls, winTile, fu, score, manganType, yakuInfos, yakumans, doraIndicators, uradoraIndicators, isTsumo, akadoraCount, who);
        LogParseCompleted(logger, who, score, manganType);

        return result;
    }

    private const int AKADORA_NUMBER = 54;

    // 手牌
    [GeneratedRegex(@"hai=""(?<hand>.*?)""")]
    private static partial Regex HaiRegex();

    // 副露牌
    [GeneratedRegex(@"m=""(?<meldCodes>.*?)""")]
    private static partial Regex MRegex();

    // 和了牌
    [GeneratedRegex(@"machi=""(?<winTile>\d+)""")]
    private static partial Regex MachiRegex();

    // 符 和了打点 満貫情報
    [GeneratedRegex(@"ten=""(?<fu>\d+),(?<score>\d+),(?<manganType>\d+)""")]
    private static partial Regex TenRegex();

    // 役 翻
    [GeneratedRegex(@"yaku=""(?<yakuInfos>.*?)""")]
    private static partial Regex YakuRegex();

    // 役満
    [GeneratedRegex(@"yakuman=""(?<yakumans>.*?)""")]
    private static partial Regex YakumanRegex();

    // ドラ表示牌
    [GeneratedRegex(@"doraHai=""(?<doraIndicators>.*?)""")]
    private static partial Regex DoraHaiRegex();

    // 裏ドラ表示牌
    [GeneratedRegex(@"doraHaiUra=""(?<uradoraIndicators>.*?)""")]
    private static partial Regex DoraHaiUraRegex();

    // 和了者番号
    [GeneratedRegex(@"who=""(?<who>\d)""")]
    private static partial Regex WhoRegex();

    // 放銃者番号
    [GeneratedRegex(@"fromWho=""(?<fromWho>\d)""")]
    private static partial Regex FromWhoRegex();

    [LoggerMessage(Level = LogLevel.Trace, Message = "和了解析開始: {AgariTag}")]
    private static partial void LogParseStart(ILogger logger, string agariTag);

    [LoggerMessage(Level = LogLevel.Trace, Message = "改行置換後: {AgariTag}")]
    private static partial void LogLineBreakReplaced(ILogger logger, string agariTag);

    [LoggerMessage(Level = LogLevel.Trace, Message = "手牌文字列: {HandString}")]
    private static partial void LogHandString(ILogger logger, string handString);

    [LoggerMessage(Level = LogLevel.Trace, Message = "手牌解析完了: {HandCount}枚")]
    private static partial void LogHandParsed(ILogger logger, int handCount);

    [LoggerMessage(Level = LogLevel.Trace, Message = "副露コード文字列: {MeldCodesString}")]
    private static partial void LogMeldCodesString(ILogger logger, string meldCodesString);

    [LoggerMessage(Level = LogLevel.Trace, Message = "副露解析完了: {CallsCount}個")]
    private static partial void LogCallsParsed(ILogger logger, int callsCount);

    [LoggerMessage(Level = LogLevel.Trace, Message = "和了牌文字列: {WinTileString}")]
    private static partial void LogWinTileString(ILogger logger, string winTileString);

    [LoggerMessage(Level = LogLevel.Trace, Message = "和了牌: {WinTile}")]
    private static partial void LogWinTile(ILogger logger, TileKind winTile);

    [LoggerMessage(Level = LogLevel.Trace, Message = "得点情報: {Fu}符 {Score}点 満貫タイプ:{ManganType}")]
    private static partial void LogScoreInfo(ILogger logger, int fu, int score, ManganType manganType);

    [LoggerMessage(Level = LogLevel.Trace, Message = "役情報文字列: {YakuInfosString}")]
    private static partial void LogYakuInfosString(ILogger logger, string yakuInfosString);

    [LoggerMessage(Level = LogLevel.Trace, Message = "役解析完了: {YakuCount}個")]
    private static partial void LogYakuParsed(ILogger logger, int yakuCount);

    [LoggerMessage(Level = LogLevel.Trace, Message = "役満文字列: {YakumansString}")]
    private static partial void LogYakumansString(ILogger logger, string yakumansString);

    [LoggerMessage(Level = LogLevel.Trace, Message = "役満解析完了: {YakumanCount}個")]
    private static partial void LogYakumanParsed(ILogger logger, int yakumanCount);

    [LoggerMessage(Level = LogLevel.Trace, Message = "ドラ表示牌文字列: {DoraIndicatorsString}")]
    private static partial void LogDoraIndicatorsString(ILogger logger, string doraIndicatorsString);

    [LoggerMessage(Level = LogLevel.Trace, Message = "裏ドラ表示牌文字列: {UradoraIndicatorsString}")]
    private static partial void LogUradoraIndicatorsString(ILogger logger, string uradoraIndicatorsString);

    [LoggerMessage(Level = LogLevel.Trace, Message = "ドラ解析完了: ドラ{DoraCount}個 裏ドラ{UradoraCount}個")]
    private static partial void LogDoraParsed(ILogger logger, int doraCount, int uradoraCount);

    [LoggerMessage(Level = LogLevel.Trace, Message = "和了者: {Who}, 放銃者: {FromWho}, ツモ: {IsTsumo}")]
    private static partial void LogWhoInfo(ILogger logger, string who, string fromWho, bool isTsumo);

    [LoggerMessage(Level = LogLevel.Trace, Message = "和了解析完了: プレイヤー{Who} {Score}点 {ManganType}")]
    private static partial void LogParseCompleted(ILogger logger, int who, int score, ManganType manganType);
}
