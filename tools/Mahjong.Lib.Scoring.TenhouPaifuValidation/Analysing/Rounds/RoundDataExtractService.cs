using Mahjong.Lib.Scoring.TenhouPaifuValidation.Downloads;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Rounds;

/// <summary>
/// 局データ抽出サービス
/// 牌譜からINITタグとAGARIタグを抽出する
/// </summary>
public partial class RoundDataExtractService(ILogger<RoundDataExtractService> logger)
{
    /// <summary>
    /// 牌譜から INIT / AGARI / RYUUKYOKU タグを抽出し、局単位の <see cref="RoundData"/> リストに変換します。
    /// </summary>
    /// <param name="paifu">解析対象の牌譜</param>
    /// <returns>抽出された局データのリスト（流局で終了する局は除く）</returns>
    public List<RoundData> Extract(Paifu paifu)
    {
        LogExtractStart(logger, paifu.Content.Length);

        List<TagInfo> tagInfos = [
            .. InitRegex().Matches(paifu.Content).Select(x=>new TagInfo(x.Index, TagType.Init, x.Value)),
            .. AgariRegex().Matches(paifu.Content).Select(x=>new TagInfo(x.Index, TagType.Agari, x.Value)),
            .. RyuukyokuRegex().Matches(paifu.Content).Select(x=>new TagInfo(x.Index, TagType.Ryuukyoku, x.Value)),
        ];
        tagInfos.Sort((a, b) => a.Position.CompareTo(b.Position));

        LogTagsExtracted(
            logger,
            tagInfos.Count(x => x.Type == TagType.Init),
            tagInfos.Count(x => x.Type == TagType.Agari),
            tagInfos.Count(x => x.Type == TagType.Ryuukyoku)
        );
        var roundDatas = new List<RoundData>();
        RoundData? currentRound = null;
        foreach (var tagInfo in tagInfos)
        {
            if (tagInfo.Type == TagType.Init)
            {
                if (currentRound is not null)
                {
                    LogRoundEnded(logger, currentRound.AgariTags.Count);
                    roundDatas.Add(currentRound);
                }
                currentRound = new RoundData(paifu.GameId, tagInfo.Content, []);
                LogRoundStarted(logger, tagInfo.Content);
            }
            else if (tagInfo.Type == TagType.Agari)
            {
                if (currentRound is null)
                {
                    throw new InvalidOperationException("AGARIタグがINITタグの前にあります。");
                }
                currentRound.AgariTags.Add(tagInfo.Content);
                LogAgariAdded(logger, currentRound.AgariTags.Count);
            }
            else
            {
                // 流局タグで終了する局は飛ばす
                if (currentRound is not null)
                {
                    LogRyuukyokuDiscarded(logger);
                }
                currentRound = null;
            }
        }
        if (currentRound is not null)
        {
            LogFinalRoundEnded(logger, currentRound.AgariTags.Count);
            roundDatas.Add(currentRound);
        }

        LogExtractCompleted(logger, roundDatas.Count);
        return roundDatas;
    }

    [GeneratedRegex(@"<INIT[^>]*>", RegexOptions.Compiled)]
    private partial Regex InitRegex();

    [GeneratedRegex(@"<AGARI[^>]*/>", RegexOptions.Compiled)]
    private partial Regex AgariRegex();

    [GeneratedRegex(@"<RYUUKYOKU[^>]*/>", RegexOptions.Compiled)]
    private partial Regex RyuukyokuRegex();

    [LoggerMessage(Level = LogLevel.Debug, Message = "局データ抽出開始: 牌譜サイズ {Size} 文字")]
    private static partial void LogExtractStart(ILogger logger, int size);

    [LoggerMessage(Level = LogLevel.Debug, Message = "タグ抽出完了: INIT {InitCount}, AGARI {AgariCount}, RYUUKYOKU {RyuukyokuCount}")]
    private static partial void LogTagsExtracted(ILogger logger, int initCount, int agariCount, int ryuukyokuCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "局終了: AGARI数 {AgariCount}")]
    private static partial void LogRoundEnded(ILogger logger, int agariCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "新局開始: {InitTag}")]
    private static partial void LogRoundStarted(ILogger logger, string initTag);

    [LoggerMessage(Level = LogLevel.Debug, Message = "AGARI追加: 累計 {Count}個")]
    private static partial void LogAgariAdded(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "流局により局を破棄")]
    private static partial void LogRyuukyokuDiscarded(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "最終局終了: AGARI数 {AgariCount}")]
    private static partial void LogFinalRoundEnded(ILogger logger, int agariCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "局データ抽出完了: {RoundCount}局抽出")]
    private static partial void LogExtractCompleted(ILogger logger, int roundCount);

    /// <summary>
    /// タグ情報
    /// </summary>
    /// <param name="Position">タグの開始位置</param>
    /// <param name="Type">タグ種別</param>
    /// <param name="Content">タグの中身</param>
    private record TagInfo(int Position, TagType Type, string Content);

    /// <summary>
    /// タグ種別
    /// </summary>
    private enum TagType
    {
        /// <summary>
        /// INITタグ
        /// </summary>
        Init,
        /// <summary>
        /// AGARIタグ
        /// </summary>
        Agari,
        /// <summary>
        /// RYUUKYOKUタグ
        /// </summary>
        Ryuukyoku,
    }
}
