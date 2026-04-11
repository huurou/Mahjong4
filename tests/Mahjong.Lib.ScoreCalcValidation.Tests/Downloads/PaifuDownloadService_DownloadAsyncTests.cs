using Mahjong.Lib.ScoreCalcValidation.Downloads;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace Mahjong.Lib.ScoreCalcValidation.Tests.Downloads;

public class PaifuDownloadService_DownloadAsyncTests : IDisposable
{
    private readonly string tempRoot_;

    public PaifuDownloadService_DownloadAsyncTests()
    {
        tempRoot_ = Path.Combine(Path.GetTempPath(), "TenhouPaifuTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot_);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempRoot_))
        {
            Directory.Delete(tempRoot_, recursive: true);
        }
        GC.SuppressFinalize(this);
    }

    private PaifuDownloadService CreateService(StubHandler handler)
    {
        return new(new HttpClient(handler), NullLogger<PaifuDownloadService>.Instance, tempRoot_, TimeSpan.Zero);
    }

    private static byte[] Gzip(string s)
    {
        using var ms = new MemoryStream();
        using (var gz = new GZipStream(ms, CompressionMode.Compress, leaveOpen: true))
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            gz.Write(bytes, 0, bytes.Length);
        }
        return ms.ToArray();
    }

    private static string BuildLogHtml(params string[] gameIds)
    {
        var sb = new StringBuilder();
        foreach (var id in gameIds)
        {
            sb.Append($"00:00 | 01 | 四鳳南喰赤 | player1 | <a href=\"http://tenhou.net/0/?log={id}\">表示</a>\n");
        }
        return sb.ToString();
    }

    [Fact]
    public async Task 正常系_四麻ログと牌譜がダウンロードされる()
    {
        // Arrange
        var logContent = BuildLogHtml("2026010100gm-00a9-0000-abcd1234");
        var handler = new StubHandler((request, _) =>
        {
            if (request.RequestUri!.ToString().Contains("sc/raw/dat/scc"))
            {
                var gz = Gzip(logContent);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(gz),
                };
                return Task.FromResult(response);
            }
            if (request.RequestUri!.ToString().Contains("/0/log/"))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<mjloggm/>"),
                };
                return Task.FromResult(response);
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });
        var service = CreateService(handler);

        // Act
        var paifus = await service.DownloadAsync("20260101");

        // Assert
        Assert.Equal(24, paifus.Count);
        Assert.All(paifus, p => Assert.Equal("<mjloggm/>", p.Content));
    }

    [Fact]
    public async Task ログダウンロード失敗時_その時間帯はスキップされる()
    {
        // Arrange
        var handler = new StubHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));
        var service = CreateService(handler);

        // Act
        var paifus = await service.DownloadAsync("20260101");

        // Assert: 全ての時間帯で失敗 → 空
        Assert.Empty(paifus);
    }

    [Fact]
    public async Task 牌譜キャッシュヒット_HTTP要求はログのみ()
    {
        // Arrange
        var gameId = "2026010100gm-00a9-0000-cached01";
        var logContent = BuildLogHtml(gameId);
        var paifuCacheDir = Path.Combine(tempRoot_, "paifu");
        Directory.CreateDirectory(paifuCacheDir);
        await File.WriteAllTextAsync(Path.Combine(paifuCacheDir, $"{gameId}.xml"), "<cached/>", TestContext.Current.CancellationToken);

        var paifuRequestCount = 0;
        var handler = new StubHandler((request, _) =>
        {
            if (request.RequestUri!.ToString().Contains("sc/raw/dat/scc"))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Gzip(logContent)),
                });
            }
            paifuRequestCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<fresh/>"),
            });
        });
        var service = CreateService(handler);

        // Act
        var paifus = await service.DownloadAsync("20260101");

        // Assert
        Assert.All(paifus, p => Assert.Equal(gameId, p.GameId));
        Assert.All(paifus, p => Assert.Equal("<cached/>", p.Content));
        Assert.Equal(0, paifuRequestCount);
    }

    [Fact]
    public async Task ログキャッシュヒット_HTTP要求は発生しない()
    {
        // Arrange
        var gameId = "2026010100gm-00a9-0000-cached02";
        var logContent = BuildLogHtml(gameId);
        var logCacheDir = Path.Combine(tempRoot_, "log");
        Directory.CreateDirectory(logCacheDir);
        for (var h = 0; h < 24; h++)
        {
            var path = Path.Combine(logCacheDir, $"20260101{h:D2}.html");
            await File.WriteAllTextAsync(path, logContent, Encoding.UTF8, TestContext.Current.CancellationToken);
        }
        var paifuCacheDir = Path.Combine(tempRoot_, "paifu");
        Directory.CreateDirectory(paifuCacheDir);
        await File.WriteAllTextAsync(Path.Combine(paifuCacheDir, $"{gameId}.xml"), "<cached/>", TestContext.Current.CancellationToken);

        var httpCalled = false;
        var handler = new StubHandler((_, _) =>
        {
            httpCalled = true;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        var service = CreateService(handler);

        // Act
        var paifus = await service.DownloadAsync("20260101");

        // Assert
        Assert.False(httpCalled);
        Assert.Equal(24, paifus.Count);
    }

    [Fact]
    public async Task 四麻以外のログ行_ゲームIDとして抽出されない()
    {
        // Arrange
        // 三麻行を含むログ（先頭スペース付き ' 四' で始まらないため無視される）
        var logContent =
            "00:00 | 01 | 三鳳南喰赤 | p1 | <a href=\"http://tenhou.net/0/?log=ignored\">x</a>\n" +
            BuildLogHtml("2026010100gm-00a9-0000-yonma001");
        var handler = new StubHandler((request, _) =>
        {
            if (request.RequestUri!.ToString().Contains("sc/raw/dat/scc"))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Gzip(logContent)),
                });
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<mjloggm/>"),
            });
        });
        var service = CreateService(handler);

        // Act
        var paifus = await service.DownloadAsync("20260101");

        // Assert: 四麻のみ抽出
        Assert.All(paifus, p => Assert.Equal("2026010100gm-00a9-0000-yonma001", p.GameId));
        Assert.Equal(24, paifus.Count);
    }

    [Fact]
    public async Task ログ3列未満の行_例外が発生する()
    {
        // Arrange: 区切り文字 | が2列しかない行
        var invalidLog = "00:00 | 01\n";
        var handler = new StubHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Gzip(invalidLog)),
            }));
        var service = CreateService(handler);

        // Act
        var exception = await Record.ExceptionAsync(() => service.DownloadAsync("20260101"));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task 四麻行にlog属性がない場合_例外が発生する()
    {
        // Arrange: 四麻行だが log=xxxx が無い
        var invalidLog = "00:00 | 01 | 四鳳南喰赤 | p1 | (no link)\n";
        var handler = new StubHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Gzip(invalidLog)),
            }));
        var service = CreateService(handler);

        // Act
        var exception = await Record.ExceptionAsync(() => service.DownloadAsync("20260101"));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task 牌譜ダウンロードが失敗ステータス_例外が発生する()
    {
        // Arrange: ログは成功、牌譜で NotFound
        var logContent = BuildLogHtml("2026010100gm-00a9-0000-notfound0");
        var handler = new StubHandler((request, _) =>
        {
            if (request.RequestUri!.ToString().Contains("sc/raw/dat/scc"))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Gzip(logContent)),
                });
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });
        var service = CreateService(handler);

        // Act
        var exception = await Record.ExceptionAsync(() => service.DownloadAsync("20260101"));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void 公開コンストラクタ_LocalAppData配下にキャッシュを作成する()
    {
        // Arrange & Act: 公開コンストラクタの到達性のみ検証（副作用はパス生成のみ）
        var service = new PaifuDownloadService(new HttpClient(new StubHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)))), NullLogger<PaifuDownloadService>.Instance);

        // Assert
        Assert.NotNull(service);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return handler(request, cancellationToken);
        }
    }
}
