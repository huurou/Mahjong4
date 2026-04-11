using Mahjong.Lib.Scoring.TenhouPaifuValidation;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Agaris;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Inits;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Analysing.Rounds;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Downloads;
using Mahjong.Lib.Scoring.TenhouPaifuValidation.Validating;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddHttpClient<PaifuDownloadService>();
services.AddSingleton<UseCase>();
services.AddSingleton<RoundDataExtractService>();
services.AddSingleton<InitParseService>();
services.AddSingleton<AgariParseService>();
services.AddSingleton<MeldParseService>();
services.AddSingleton<CalcValidateService>();

var provider = services.BuildServiceProvider();
var useCase = provider.GetRequiredService<UseCase>();

string? logDate;
do
{
    Console.Write("log指定date YYYYMMDD形式を入力してください > ");
    logDate = Console.ReadLine();
}
while (string.IsNullOrEmpty(logDate));

Console.WriteLine("牌譜の解析中...");

var agariInfos = await useCase.AnalysisPaifu(logDate);

Console.WriteLine("牌譜の解析完了");

if (agariInfos.Count == 0)
{
    Console.WriteLine("検証対象の和了情報がありません。");
    return;
}

var validCount = 0;
var invalidResults = new List<ValidateResult>();
for (var i = 0; i < agariInfos.Count; i++)
{
    var result = useCase.ValidateCalc(agariInfos[i]);
    if (result.IsSuccess)
    {
        validCount++;
    }
    else
    {
        invalidResults.Add(result);
    }
}
Console.WriteLine("牌譜の検証完了");
Console.WriteLine($"検証成功率: {(double)validCount / agariInfos.Count * 100:0.000}% ({validCount}/{agariInfos.Count})");
if (invalidResults.Count == 0)
{
    Console.WriteLine("全ての牌譜で検証に成功しました。");
}
else
{
    Console.WriteLine("検証失敗サンプル[0]:");
    Console.WriteLine(invalidResults[0].AgariInfo);
    Console.WriteLine(invalidResults[0].HandResult);
}
