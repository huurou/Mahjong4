using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Calls;

/// <summary>
/// 副露を表現するクラス
/// </summary>
public record Call
{
    /// <summary>
    /// 副露種類
    /// </summary>
    public CallType Type { get; init; }

    /// <summary>
    /// 副露に含まれる牌のリスト
    /// </summary>
    public ImmutableList<Tile> Tiles { get; init; }

    /// <summary>
    /// 副露元のプレイヤーインデックス (暗槓・加槓は自身)
    /// </summary>
    public PlayerIndex From { get; init; }

    /// <summary>
    /// 鳴かれた牌 (暗槓は鳴かれた牌が存在しないため null、加槓は元のポンで鳴かれた牌)
    /// </summary>
    public Tile? CalledTile { get; init; }

    public Call(CallType type, ImmutableList<Tile> tiles, PlayerIndex from, Tile? calledTile)
    {
        Validate(type, tiles, calledTile);

        Type = type;
        Tiles = tiles;
        From = from;
        CalledTile = calledTile;
    }

    /// <summary>
    /// 副露種別に応じた牌リストと鳴かれた牌の整合性を検証します。不正な場合は <see cref="ArgumentException"/> を投げます。
    /// </summary>
    public static void Validate(CallType type, ImmutableList<Tile> tiles, Tile? calledTile)
    {
        var expectedCount = type switch
        {
            CallType.Chi or CallType.Pon => 3,
            CallType.Ankan or CallType.Daiminkan or CallType.Kakan => 4,
            _ => throw new ArgumentException($"不明な副露種別:{type}", nameof(type)),
        };

        if (tiles.Count != expectedCount)
        {
            throw new ArgumentException(
                $"{type} では{expectedCount}枚の牌が必要です。実際:{tiles.Count}枚",
                nameof(tiles)
            );
        }

        if (type == CallType.Chi)
        {
            // 順子: 全て数牌、同スーツ、連続する3牌
            var kinds = tiles.Select(x => x.Kind).OrderBy(x => x).ToArray();
            if (!kinds[0].IsNumber)
            {
                throw new ArgumentException(
                    $"{type} では数牌 (萬子・筒子・索子) のみ指定可能です。",
                    nameof(tiles)
                );
            }
            if (!kinds[0].IsSameSuit(kinds[2]))
            {
                throw new ArgumentException(
                    $"{type} では同じスートの牌を指定する必要があります。",
                    nameof(tiles)
                );
            }
            if (!kinds[0].TryGetAtDistance(1, out var next1) || next1 != kinds[1] ||
                !kinds[1].TryGetAtDistance(1, out var next2) || next2 != kinds[2])
            {
                throw new ArgumentException(
                    $"{type} では連続する3牌を指定する必要があります。",
                    nameof(tiles)
                );
            }
        }
        else
        {
            // ポン・暗槓・大明槓・加槓: 全て同じ牌種
            var kind = tiles[0].Kind;
            if (tiles.Any(x => x.Kind != kind))
            {
                throw new ArgumentException(
                    $"{type} では全ての牌が同じ牌種である必要があります。",
                    nameof(tiles)
                );
            }
        }

        if (type == CallType.Ankan)
        {
            if (calledTile is not null)
            {
                throw new ArgumentException(
                    $"{type} では鳴かれた牌は存在しないため null を指定してください。calledTile:{calledTile}",
                    nameof(calledTile)
                );
            }
        }
        else
        {
            if (calledTile is null)
            {
                throw new ArgumentException(
                    $"{type} では鳴かれた牌を指定する必要があります。",
                    nameof(calledTile)
                );
            }
            if (!tiles.Contains(calledTile))
            {
                throw new ArgumentException(
                    $"{type} では鳴かれた牌が副露牌に含まれている必要があります。calledTile:{calledTile}",
                    nameof(calledTile)
                );
            }
        }
    }

    public virtual bool Equals(Call? other)
    {
        return other is not null
            && Type == other.Type
            && From == other.From
            && CalledTile == other.CalledTile
            && Tiles.SequenceEqual(other.Tiles);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Type);
        hash.Add(From);
        hash.Add(CalledTile);
        hash.Add(Tiles.Count);
        foreach (var tile in Tiles)
        {
            hash.Add(tile);
        }
        return hash.ToHashCode();
    }
}
