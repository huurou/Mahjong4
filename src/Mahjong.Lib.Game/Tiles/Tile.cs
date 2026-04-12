using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Game.Tiles;

public record Tile
{
    public const int ID_MIN = 0;
    public const int ID_MAX = 135;

    public int Id { get; init; }

    public TileKind Kind => new(Id / 4);

    public Tile(int id)
    {
        if (id is < ID_MIN or > ID_MAX)
        {
            throw new ArgumentOutOfRangeException(nameof(id), $"牌Idは {ID_MIN} から {ID_MAX} の範囲内である必要があります。");
        }

        Id = id;
    }
}
