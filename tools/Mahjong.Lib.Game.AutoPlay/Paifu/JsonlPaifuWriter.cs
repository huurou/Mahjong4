using System.Text;
using System.Text.Json;

namespace Mahjong.Lib.Game.AutoPlay.Paifu;

/// <summary>
/// <see cref="PaifuEntry"/> を JSONL (1 エントリ 1 行) で書き出す
/// </summary>
public sealed class JsonlPaifuWriter : IDisposable
{
    private static JsonSerializerOptions Options { get; } = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly StreamWriter writer_;
    private readonly Lock lock_ = new();
    private bool disposed_;

    public JsonlPaifuWriter(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        writer_ = new StreamWriter(filePath, append: false, Encoding.UTF8);
    }

    public void Write(PaifuEntry entry)
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
        var record = new Dictionary<string, object?> { ["t"] = entry.Type };
        foreach (var kv in entry.Fields)
        {
            record[kv.Key] = kv.Value;
        }
        var json = JsonSerializer.Serialize(record, Options);
        lock (lock_)
        {
            writer_.WriteLine(json);
        }
    }

    public void Flush()
    {
        ObjectDisposedException.ThrowIf(disposed_, this);
        lock (lock_)
        {
            writer_.Flush();
        }
    }

    public void Dispose()
    {
        if (disposed_) { return; }

        lock (lock_)
        {
            if (disposed_) { return; }

            disposed_ = true;
            writer_.Dispose();
        }
    }
}
