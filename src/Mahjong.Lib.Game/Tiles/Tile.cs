namespace Mahjong.Lib.Game.Tiles;

public record Tile
{
    public const int ID_MIN = 0;
    public const int ID_MAX = 135;

    public int Id { get; init; }

    /// <summary>
    /// 牌種ID 0-33の範囲で、同じ牌種の4枚は同じ値を返す
    /// </summary>
    public int Kind => Id / 4;

    /// <summary>
    /// 幺九牌 (1/9 数牌 および 字牌) かを判定します
    /// 流し満貫条件判定などで用います
    /// </summary>
    // 0=一萬, 8=九萬, 9=一筒, 17=九筒, 18=一索, 26=九索, 27-33=字牌
    public bool IsYaochuu => Kind is 0 or 8 or 9 or 17 or 18 or 26 or >= 27;

    public Tile(int id)
    {
        if (id is < ID_MIN or > ID_MAX)
        {
            throw new ArgumentOutOfRangeException(nameof(id), $"牌Idは {ID_MIN} から {ID_MAX} の範囲内である必要があります。");
        }

        Id = id;
    }
}
