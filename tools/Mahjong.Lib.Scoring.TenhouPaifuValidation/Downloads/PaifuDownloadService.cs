using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Mahjong.Lib.Scoring.TenhouPaifuValidation.Downloads;

/// <summary>
/// 牌譜ダウンロードサービス
/// </summary>
public partial class PaifuDownloadService
{
    private readonly HttpClient client_;
    private readonly ILogger<PaifuDownloadService> logger_;
    private readonly TimeSpan downloadDelay_;
    /// <summary>
    /// TenhouPaifuフォルダパス
    /// </summary>
    private readonly string tenhouPaifuDirPath_;

    /// <summary>
    /// logフォルダパス
    /// </summary>
    private string LogDirPath
    {
        get
        {
            var path = Path.Combine(tenhouPaifuDirPath_, "log");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }

    /// <summary>
    /// paifuフォルダパス
    /// </summary>
    private string PaifuDirPath
    {
        get
        {
            var path = Path.Combine(tenhouPaifuDirPath_, "paifu");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }

    /// <summary>
    /// 通常利用向けコンストラクタ。キャッシュは %LocalAppData%\TenhouPaifu 配下に作成する。
    /// </summary>
    public PaifuDownloadService(HttpClient client, ILogger<PaifuDownloadService> logger)
        : this(client, logger, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TenhouPaifu"), TimeSpan.FromSeconds(1))
    {
    }

    /// <summary>
    /// テスト用コンストラクタ。キャッシュルートとダウンロード間の待機時間を差し替えられる。
    /// </summary>
    internal PaifuDownloadService(HttpClient client, ILogger<PaifuDownloadService> logger, string tenhouPaifuDirPath, TimeSpan downloadDelay)
    {
        client_ = client;
        logger_ = logger;
        tenhouPaifuDirPath_ = tenhouPaifuDirPath;
        downloadDelay_ = downloadDelay;
    }

    /// <summary>
    /// 指定された日付の牌譜をダウンロードする
    /// </summary>
    /// <param name="date">日付（YYYYMMDD形式）</param>
    /// <returns>ダウンロードした牌譜のリスト</returns>
    public async Task<List<Paifu>> DownloadAsync(string date)
    {
        LogDownloadStart(logger_, date);

        var paifus = new List<Paifu>();
        foreach (var hour in Enumerable.Range(0, 24).Select(x => $"{x:D2}"))
        {
            var dateHour = $"{date}{hour}";
            string log;
            try
            {
                log = await DownloadLogAsync(dateHour);
            }
            catch (Exception ex)
            {
                LogLogDownloadFailed(logger_, ex, dateHour);
                continue;
            }
            var gameIds = ExtractGameId(log);
            foreach (var gameId in gameIds)
            {
                var content = await DownloadPaifu(gameId);
                paifus.Add(new Paifu(gameId, content));
            }
        }

        LogDownloadCompleted(logger_, date, paifus.Count);

        return paifus;
    }

    /// <summary>
    /// 指定された日付のログをダウンロードする
    /// </summary>
    /// <param name="dateHour">日付+時間（YYYYMMDDHH形式）</param>
    /// <returns>ダウンロードしたログの内容</returns>
    /// <exception cref="ArgumentException">日付形式が正しくない場合</exception>
    /// <exception cref="InvalidOperationException">ダウンロードに失敗した場合</exception>
    private async Task<string> DownloadLogAsync(string dateHour)
    {
        LogLogDownloadStart(logger_, dateHour);

        var path = Path.Combine(LogDirPath, $"{dateHour}.html");
        if (File.Exists(path))
        {
            LogLogCacheHit(logger_, path);
            return await File.ReadAllTextAsync(path, Encoding.UTF8);
        }

        var url = $"https://tenhou.net/sc/raw/dat/scc{dateHour}.html.gz";
        LogLogDownloadUrl(logger_, url);

        using var response = await client_.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"ログのダウンロードに失敗しました。url:{url}");
        }
        await Task.Delay(downloadDelay_);

        using var contentStream = await response.Content.ReadAsStreamAsync();
        //gzip圧縮されているので解凍する
        using var decompressedStream = new GZipStream(contentStream, CompressionMode.Decompress);
        using var streamReader = new StreamReader(decompressedStream);
        var log = await streamReader.ReadToEndAsync();
        await File.WriteAllTextAsync(path, log);

        LogLogDownloadCompleted(logger_, dateHour, log.Length);

        return log;
    }

    /// <summary>
    /// ログからゲームIDを抽出する（四麻のみ対象）
    /// </summary>
    /// <param name="log">ログの内容</param>
    /// <returns>ゲームIDのリスト</returns>
    /// <exception cref="InvalidOperationException">ログの解析に失敗した場合</exception>
    private List<string> ExtractGameId(string log)
    {
        LogGameIdExtractStart(logger_);

        var gameIds = new List<string>();
        var lines = log.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        LogLogLineCount(logger_, lines.Length);

        foreach (var line in lines)
        {
            var parts = line.Split('|');
            if (parts.Length < 3)
            {
                throw new InvalidOperationException("ログの3列目が見つかりませんでした。");
            }

            // 四麻のみを対象とする
            if (parts[2].StartsWith(" 四"))
            {
                // log=の後のゲームIDを抽出
                var logMatch = GameIdRegex().Match(line);
                if (!logMatch.Success)
                {
                    throw new InvalidOperationException($"対局Idが見つかりませんでした。line:{line}");
                }

                var gameId = logMatch.Groups[1].Value;
                gameIds.Add(gameId);
                LogGameIdExtracted(logger_, gameId);
            }
        }

        LogGameIdExtractCompleted(logger_, gameIds.Count);
        return gameIds;
    }

    /// <summary>
    /// 指定されたゲームIDの牌譜をダウンロードする
    /// </summary>
    /// <param name="gameId">ゲームID</param>
    /// <returns>ダウンロードした牌譜の内容</returns>
    /// <exception cref="InvalidOperationException">ダウンロードに失敗した場合</exception>
    private async Task<string> DownloadPaifu(string gameId)
    {
        LogPaifuDownloadStart(logger_, gameId);

        var path = Path.Combine(PaifuDirPath, $"{gameId}.xml");
        if (File.Exists(path))
        {
            LogPaifuCacheHit(logger_, gameId);
            return await File.ReadAllTextAsync(path);
        }

        var url = $"http://tenhou.net/0/log/?{gameId}";
        LogPaifuDownloadUrl(logger_, url);

        using var response = await client_.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"牌譜のダウンロードに失敗しました。url:{url} statusCode:{response.StatusCode}");
        }

        var paifu = await response.Content.ReadAsStringAsync();
        await File.WriteAllTextAsync(path, paifu);

        LogPaifuDownloadCompleted(logger_, gameId, paifu.Length);

        await Task.Delay(downloadDelay_);

        return paifu;
    }

    /// <summary>
    /// ゲームID抽出用の正規表現
    /// </summary>
    /// <returns>ゲームID抽出用の正規表現</returns>
    [GeneratedRegex(@"log=([a-z0-9\-]+)")]
    private static partial Regex GameIdRegex();

    [LoggerMessage(Level = LogLevel.Information, Message = "牌譜ダウンロード開始: {Date}")]
    private static partial void LogDownloadStart(ILogger logger, string date);

    [LoggerMessage(Level = LogLevel.Information, Message = "牌譜ダウンロード完了: {DateHour}, ダウンロード数: {Count}")]
    private static partial void LogDownloadCompleted(ILogger logger, string dateHour, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "ログのダウンロードに失敗しました: {DateHour}")]
    private static partial void LogLogDownloadFailed(ILogger logger, Exception ex, string dateHour);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ログダウンロード開始: {DateHour}")]
    private static partial void LogLogDownloadStart(ILogger logger, string dateHour);

    [LoggerMessage(Level = LogLevel.Debug, Message = "キャッシュされたログファイルを使用: {Path}")]
    private static partial void LogLogCacheHit(ILogger logger, string path);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ログダウンロードURL: {Url}")]
    private static partial void LogLogDownloadUrl(ILogger logger, string url);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ログダウンロード完了: {DateHour}, ファイルサイズ: {Size} bytes")]
    private static partial void LogLogDownloadCompleted(ILogger logger, string dateHour, int size);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ゲームID抽出開始")]
    private static partial void LogGameIdExtractStart(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ログ行数: {LineCount}")]
    private static partial void LogLogLineCount(ILogger logger, int lineCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ゲームID抽出: {GameId}")]
    private static partial void LogGameIdExtracted(ILogger logger, string gameId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "ゲームID抽出完了: {Count}件")]
    private static partial void LogGameIdExtractCompleted(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "牌譜ダウンロード開始: {GameId}")]
    private static partial void LogPaifuDownloadStart(ILogger logger, string gameId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "キャッシュされた牌譜ファイルを使用: {GameId}")]
    private static partial void LogPaifuCacheHit(ILogger logger, string gameId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "牌譜ダウンロードURL: {Url}")]
    private static partial void LogPaifuDownloadUrl(ILogger logger, string url);

    [LoggerMessage(Level = LogLevel.Debug, Message = "牌譜ダウンロード完了: {GameId}, サイズ: {Size} bytes")]
    private static partial void LogPaifuDownloadCompleted(ILogger logger, string gameId, int size);
}
