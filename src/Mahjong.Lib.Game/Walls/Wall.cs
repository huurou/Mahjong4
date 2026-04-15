using Mahjong.Lib.Game.Tiles;
using System.Collections;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Walls;

/// <summary>
/// 山牌を表現するクラス
/// </summary>
/// <remarks>
/// 136枚の牌を保持する。論理レイアウトは天鳳互換:
/// - yama[0..13] = 王牌14枚 (下段偶数/上段奇数、7幢構成)
///   - yama[0,1]/yama[2,3] = 嶺上牌ブロック (取り順: 1→0→3→2)
///   - yama[5]=初期ドラ、yama[7,9,11,13]=カンドラ1〜4
///   - yama[4]=初期裏ドラ、yama[6,8,10,12]=カン裏ドラ1〜4
/// - yama[14..83] = ツモ山70枚 (末尾から取る、yama[83]が親の第1ツモ)
/// - yama[84..135] = 配牌52枚 (末尾から取る)
/// </remarks>
public record Wall : IEnumerable<Tile>
{
    private const int TILE_COUNT = 136;
    private const int DEAD_WALL_SIZE = 14;
    private const int LIVE_WALL_SIZE = 122;
    private const int RINSHAN_MAX = 4;

    private static readonly int[] RINSHAN_INDICES = [1, 0, 3, 2];

    private readonly ImmutableArray<Tile> tiles_;

    /// <summary>
    /// 配牌・ツモで取った枚数 (0〜122)
    /// </summary>
    public int DrawnCount { get; init; }

    /// <summary>
    /// 嶺上牌から取った枚数 (0〜4)
    /// </summary>
    public int RinshanDrawnCount { get; init; }

    /// <summary>
    /// ドラ表示牌の表示枚数 (1〜5、初期値1)
    /// </summary>
    public int DoraRevealedCount { get; init; } = 1;

    /// <summary>
    /// ツモ可能な残り牌数
    /// (王牌は常に14枚維持のため、嶺上牌を取るたびにツモ山末尾から1枚が王牌にスライドする)
    /// </summary>
    public int LiveRemaining => LIVE_WALL_SIZE - DrawnCount - RinshanDrawnCount;

    /// <summary>
    /// 嶺上牌の残り枚数
    /// </summary>
    public int RinshanRemaining => RINSHAN_MAX - RinshanDrawnCount;

    /// <summary>
    /// 槓が可能か (嶺上牌が残っており、かつ王牌を補充するツモ山も残っていること)
    /// </summary>
    public bool CanKan => RinshanRemaining > 0 && LiveRemaining > 0;

    public Wall(IEnumerable<Tile> tiles)
    {
        tiles_ = [.. tiles];
        if (tiles_.Length != TILE_COUNT)
        {
            throw new ArgumentException($"山牌は {TILE_COUNT} 枚である必要があります。実際:{tiles_.Length}枚", nameof(tiles));
        }
    }

    /// <summary>
    /// 山から1枚取ります (配牌・ツモ共通)。
    /// </summary>
    public Wall Draw(out Tile tile)
    {
        if (DrawnCount >= LIVE_WALL_SIZE)
        {
            throw new InvalidOperationException("ツモ山に牌がありません。");
        }

        tile = tiles_[TILE_COUNT - 1 - DrawnCount];
        return this with { DrawnCount = DrawnCount + 1 };
    }

    /// <summary>
    /// 山からcount枚をまとめて取ります。
    /// </summary>
    public Wall Draw(int count, out ImmutableList<Tile> tiles)
    {
        if (DrawnCount + count > LIVE_WALL_SIZE)
        {
            throw new InvalidOperationException("ツモ山に牌がありません。");
        }

        var builder = ImmutableList.CreateBuilder<Tile>();
        for (var i = 0; i < count; i++)
        {
            builder.Add(tiles_[TILE_COUNT - 1 - DrawnCount - i]);
        }
        tiles = builder.ToImmutable();
        return this with { DrawnCount = DrawnCount + count };
    }

    /// <summary>
    /// 嶺上牌から1枚取ります。
    /// </summary>
    public Wall DrawRinshan(out Tile tile)
    {
        if (RinshanDrawnCount >= RINSHAN_MAX)
        {
            throw new InvalidOperationException("嶺上牌がありません。");
        }
        if (LiveRemaining <= 0)
        {
            throw new InvalidOperationException("ツモ山が空のため槓できません。");
        }

        tile = tiles_[RINSHAN_INDICES[RinshanDrawnCount]];
        return this with { RinshanDrawnCount = RinshanDrawnCount + 1 };
    }

    /// <summary>
    /// 新ドラを1枚表示します。
    /// </summary>
    public Wall RevealDora()
    {
        if (DoraRevealedCount >= 5)
        {
            throw new InvalidOperationException("ドラ表示牌はこれ以上表示できません。");
        }

        return this with { DoraRevealedCount = DoraRevealedCount + 1 };
    }

    /// <summary>
    /// n番目 (0オリジン) のドラ表示牌を返します。
    /// </summary>
    public Tile GetDoraIndicator(int n)
    {
        if (n < 0 || n >= DoraRevealedCount)
        {
            throw new ArgumentOutOfRangeException(nameof(n), $"表示中のドラ表示牌数は {DoraRevealedCount} です。");
        }

        return tiles_[5 + 2 * n];
    }

    /// <summary>
    /// n番目 (0オリジン) の裏ドラ表示牌を返します。
    /// </summary>
    public Tile GetUradoraIndicator(int n)
    {
        if (n < 0 || n >= DoraRevealedCount)
        {
            throw new ArgumentOutOfRangeException(nameof(n), $"参照可能な裏ドラ表示牌数は {DoraRevealedCount} です。");
        }

        return tiles_[4 + 2 * n];
    }

    public virtual bool Equals(Wall? other)
    {
        return other is Wall wall &&
            DrawnCount == wall.DrawnCount &&
            RinshanDrawnCount == wall.RinshanDrawnCount &&
            DoraRevealedCount == wall.DoraRevealedCount &&
            tiles_.SequenceEqual(wall.tiles_);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(DrawnCount);
        hash.Add(RinshanDrawnCount);
        hash.Add(DoraRevealedCount);
        foreach (var tile in tiles_)
        {
            hash.Add(tile);
        }
        return hash.ToHashCode();
    }

    public IEnumerator<Tile> GetEnumerator()
    {
        return ((IEnumerable<Tile>)tiles_).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)tiles_).GetEnumerator();
    }
}
