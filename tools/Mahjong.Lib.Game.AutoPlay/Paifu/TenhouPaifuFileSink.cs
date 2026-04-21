using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Paifu;
using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.AutoPlay.Paifu;

/// <summary>
/// 天鳳 JSON 牌譜 (tenhou.net/6 互換) を 1 対局 = 1 ファイルで書き出す薄いファクトリ
/// </summary>
/// <remarks>
/// <para>ファイル名: <c>tenhou6_{yyyyMMdd}_{HHmmssfff}(ローカル時刻)_{32 桁 hex}.jsonl</c></para>
/// <para>先頭をローカル日時の固定幅文字列 (<c>yyyyMMdd_HHmmssfff</c>) にすることで Windows Explorer の自然順ソート
/// (先頭連続数字を数値抽出する挙動) でも辞書順ソートでも時系列順が保たれる。日付と時刻を "_" で分けて可読性も確保する。
/// 末尾の 32 桁 hex は並列 worker の同時生成衝突回避用 (<see cref="Guid.NewGuid"/>)。</para>
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
        // 先頭をローカル日時 (yyyyMMdd_HHmmssfff 固定幅) にすることで Windows Explorer の自然順ソート
        // (先頭連続数字を数値抽出する挙動) でも辞書順ソートでも時系列順が保たれる。日付と時刻を "_" で
        // 分けて一覧の可読性も確保する。末尾の 32 桁 hex は並列 worker 同時生成の衝突回避用
        var now = DateTime.Now;
        var fileName = $"tenhou6_{now:yyyyMMdd_HHmmssfff}_{Guid.NewGuid():N}.jsonl";
        var path = Path.Combine(outputDirectory, fileName);
        var writer = new StreamWriter(path);
        var recorder = new TenhouJsonPaifuRecorder(writer, players, rules, title);
        return (writer, recorder);
    }
}
