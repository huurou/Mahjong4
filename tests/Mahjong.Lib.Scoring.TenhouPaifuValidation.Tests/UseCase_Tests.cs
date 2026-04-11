using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Agaris;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Inits;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Rounds;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Downloads;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Validating;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace Mahjong.Lib.Scoring.TenhouPaifuValidation.Tests;

public class UseCase_Tests : IDisposable
{
    private readonly string tempRoot_;
    private readonly UseCase useCase_;

    public UseCase_Tests()
    {
        tempRoot_ = Path.Combine(Path.GetTempPath(), "UseCaseTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot_);

        var meldParseService = new MeldParseService(NullLogger<MeldParseService>.Instance);
        var downloadService = new PaifuDownloadService(
            new HttpClient(new AlwaysNotFoundHandler()),
            NullLogger<PaifuDownloadService>.Instance,
            tempRoot_,
            TimeSpan.Zero);

        useCase_ = new UseCase(
            downloadService,
            new RoundDataExtractService(NullLogger<RoundDataExtractService>.Instance),
            new InitParseService(NullLogger<InitParseService>.Instance),
            new AgariParseService(meldParseService, NullLogger<AgariParseService>.Instance),
            new CalcValidateService(NullLogger<CalcValidateService>.Instance),
            NullLogger<UseCase>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempRoot_))
        {
            Directory.Delete(tempRoot_, recursive: true);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AnalysisPaifu_牌譜がない場合_空リストを返す()
    {
        // Arrange: 全てのログ取得が 404 失敗 → 空

        // Act
        var actual = await useCase_.AnalysisPaifu("20260101");

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public async Task AnalysisPaifu_キャッシュされた牌譜_AgariInfoが生成される()
    {
        // Arrange: log キャッシュに四麻行のみ置き、paifu キャッシュに実牌譜を置く
        var gameId = "2026010100gm-00a9-0000-testxx001";
        var paifuXml = await File.ReadAllTextAsync(Path.Combine("TestData", "PaifuWithoutLineBreaks.xml"), TestContext.Current.CancellationToken);

        var logDir = Path.Combine(tempRoot_, "log");
        Directory.CreateDirectory(logDir);
        var logLine = $"00:00 | 01 | 四鳳南喰赤 | p1 | <a href=\"http://tenhou.net/0/?log={gameId}\">x</a>\n";
        // 24時間分のうち 1 ファイルだけ有効内容にして、他は空でキャッシュヒット扱い
        for (var h = 0; h < 24; h++)
        {
            var path = Path.Combine(logDir, $"20260101{h:D2}.html");
            await File.WriteAllTextAsync(path, h == 0 ? logLine : string.Empty, TestContext.Current.CancellationToken);
        }

        var paifuDir = Path.Combine(tempRoot_, "paifu");
        Directory.CreateDirectory(paifuDir);
        await File.WriteAllTextAsync(Path.Combine(paifuDir, $"{gameId}.xml"), paifuXml, TestContext.Current.CancellationToken);

        // Act
        var actual = await useCase_.AnalysisPaifu("20260101");

        // Assert: 少なくとも1件以上の和了情報が得られる
        Assert.NotEmpty(actual);
    }

    [Fact]
    public async Task ValidateCalc_AgariInfoの点数計算結果が返る()
    {
        // Arrange
        var gameId = "2026010100gm-00a9-0000-validate01";
        var paifuXml = await File.ReadAllTextAsync(Path.Combine("TestData", "PaifuWithoutLineBreaks.xml"), TestContext.Current.CancellationToken);

        var logDir = Path.Combine(tempRoot_, "log");
        Directory.CreateDirectory(logDir);
        for (var h = 0; h < 24; h++)
        {
            var path = Path.Combine(logDir, $"20260101{h:D2}.html");
            var content = h == 0 ? $"00:00 | 01 | 四鳳南喰赤 | p1 | <a href=\"http://tenhou.net/0/?log={gameId}\">x</a>\n" : string.Empty;
            await File.WriteAllTextAsync(path, content, TestContext.Current.CancellationToken);
        }

        var paifuDir = Path.Combine(tempRoot_, "paifu");
        Directory.CreateDirectory(paifuDir);
        await File.WriteAllTextAsync(Path.Combine(paifuDir, $"{gameId}.xml"), paifuXml, TestContext.Current.CancellationToken);

        var agariInfos = await useCase_.AnalysisPaifu("20260101");

        // Act
        var result = useCase_.ValidateCalc(agariInfos[0]);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.HandResult);
    }

    private sealed class AlwaysNotFoundHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
