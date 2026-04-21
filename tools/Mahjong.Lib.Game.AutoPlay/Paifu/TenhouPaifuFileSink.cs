using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Paifu;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.AutoPlay.Paifu;

/// <summary>
/// 天鳳 JSON 牌譜 (tenhou.net/6 互換) を 1 対局 = 1 ファイルで書き出す薄いファクトリ
/// </summary>
/// <remarks>
/// <para>ファイル名: <c>tenhou6_{yyyyMMdd}_{HHmmssfff}.jsonl</c></para>
/// <para><see cref="Create"/> で (StreamWriter, Recorder) のタプルを返す。
/// 呼び出し側は使い終わったら StreamWriter を Dispose する (Recorder は writer を所有しない)</para>
/// </remarks>
public static class TenhouPaifuFileSink
{
    public static (StreamWriter Writer, TenhouJsonPaifuRecorder Recorder) Create(
        string outputDirectory,
        PlayerList players,
        GameRules rules,
        IReadOnlyList<string>? title = null
    )
    {
        Directory.CreateDirectory(outputDirectory);
        var now = DateTime.Now;
        var fileName = $"tenhou6_{now:yyyyMMdd}_{now:HHmmssfff}.jsonl";
        var path = Path.Combine(outputDirectory, fileName);
        var writer = new StreamWriter(path);
        var recorder = new TenhouJsonPaifuRecorder(writer, players, rules, title);
        return (writer, recorder);
    }
}
