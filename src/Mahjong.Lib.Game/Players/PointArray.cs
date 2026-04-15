using System.Collections;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Players;

/// <summary>
/// 各プレイヤーの持ち点の配列
/// </summary>
public record PointArray : IEnumerable<Point>
{
    private ImmutableArray<Point> points_;

    private PointArray()
    {
        points_ = [.. Enumerable.Repeat(new Point(0), 4)];
    }

    public PointArray(Point init)
    {
        points_ = [.. Enumerable.Repeat(init, 4)];
    }

    public Point this[PlayerIndex index] => points_[index.Value];

    /// <summary>
    /// 指定のプレイヤーインデックスの持ち点に値を加算した新しいPointArrayを返す
    /// </summary>
    /// <param name="index">対象プレイヤーインデックス</param>
    /// <param name="value">加算する値</param>
    /// <returns>持ち点を加算した新しいPointArray</returns>
    public PointArray AddPoint(PlayerIndex index, int value)
    {
        var builder = points_.ToBuilder();
        builder[index.Value] = new Point(builder[index.Value].Value + value);
        return new PointArray { points_ = builder.ToImmutable() };
    }

    /// <summary>
    /// 指定のプレイヤーインデックスの持ち点から値を減算した新しいPointArrayを返す
    /// </summary>
    /// <param name="index">対象プレイヤーインデックス</param>
    /// <param name="value">減算する値</param>
    /// <returns>持ち点を減算した新しいPointArray</returns>
    public PointArray SubtractPoint(PlayerIndex index, int value)
    {
        var builder = points_.ToBuilder();
        builder[index.Value] = new Point(builder[index.Value].Value - value);
        return new PointArray { points_ = builder.ToImmutable() };
    }

    public virtual bool Equals(PointArray? other)
    {
        return other is PointArray array && points_.SequenceEqual(array.points_);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var point in points_)
        {
            hash.Add(point);
        }
        return hash.ToHashCode();
    }

    public IEnumerator<Point> GetEnumerator()
    {
        return ((IEnumerable<Point>)points_).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)points_).GetEnumerator();
    }
}
