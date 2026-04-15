namespace Mahjong.Lib.Game;

public static class IdUtility
{
    /// <summary>
    /// prefix_uuidv7形式のIDを生成（例: REQ_019d4bcf-d05b-776a-b4aa-ad3c1e1618e9）
    /// </summary>
    /// <param name="prefix">先頭プレフィックス（例: "REQ"）</param>
    public static string NewId(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix)) { throw new ArgumentException("prefix is required.", nameof(prefix)); }

        var uuid = Guid.CreateVersion7();
        return $"{prefix}_{uuid:D}";
    }

    /// <summary>
    /// prefix_uuidv7形式のIDを検証する。
    /// プレフィックス一致 + UUID部分がUUIDv7であることを確認する。
    /// </summary>
    /// <param name="value">検証対象の値</param>
    /// <param name="expectedPrefix">期待するプレフィックス（例: "AREA"）</param>
    public static void ValidateId(string value, string expectedPrefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var prefixWithSeparator = $"{expectedPrefix}_";
        if (!value.StartsWith(prefixWithSeparator, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Id must start with '{prefixWithSeparator}'. Got: '{value}'");
        }

        var uuidPart = value[prefixWithSeparator.Length..];
        if (!Guid.TryParseExact(uuidPart, "D", out var guid))
        {
            throw new ArgumentException($"Id must contain a valid UUID after prefix. Got: '{value}'");
        }

        if (!IsUuidV7(guid))
        {
            throw new ArgumentException($"Id must contain a UUIDv7 (version=7, variant=RFC4122). Got: '{value}'");
        }

        static bool IsUuidV7(Guid guid)
        {
            return guid.Version == 7 &&
                // RFC 4122 / RFC 9562 系は 10xx => 0x8, 0x9, 0xA, 0xB
                guid.Variant is 0x8 or 0x9 or 0xA or 0xB;
        }
    }
}
