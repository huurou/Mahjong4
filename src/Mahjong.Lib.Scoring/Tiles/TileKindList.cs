using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mahjong.Lib.Scoring.Tiles;

/// <summary>
/// 牌種別の集合を表現するクラス
/// </summary>
[CollectionBuilder(typeof(TileKindListBuilder), "Create")]
public record TileKindList() : IEnumerable<TileKind>, IComparable<TileKindList>
{
    private readonly ImmutableList<TileKind> tileKinds_ = [];

    /// <summary>
    /// 全ての牌種別が萬子かどうか
    /// </summary>
    public bool IsAllMan => tileKinds_.All(x => x.IsMan);
    /// <summary>
    /// 全ての牌種別が筒子かどうか
    /// </summary>
    public bool IsAllPin => tileKinds_.All(x => x.IsPin);
    /// <summary>
    /// 全ての牌種別が索子かどうか
    /// </summary>
    public bool IsAllSou => tileKinds_.All(x => x.IsSou);
    /// <summary>
    /// 全ての牌種別が数牌でかつ同じ種類(萬子/筒子/索子)かどうか
    /// </summary>
    public bool IsAllSameSuit => IsAllMan || IsAllPin || IsAllSou;
    /// <summary>
    /// 全ての牌種別が数牌かどうか
    /// </summary>
    public bool IsAllNumber => tileKinds_.All(x => x.IsNumber);
    /// <summary>
    /// 全ての牌種別が字牌かどうか
    /// </summary>
    public bool IsAllHonor => tileKinds_.All(x => x.IsHonor);
    /// <summary>
    /// 全ての牌種別が風牌かどうか
    /// </summary>
    public bool IsAllWind => tileKinds_.All(x => x.IsWind);
    /// <summary>
    /// 全ての牌種別が三元牌かどうか
    /// </summary>
    public bool IsAllDragon => tileKinds_.All(x => x.IsDragon);

    /// <summary>
    /// 対子かどうか
    /// </summary>
    public bool IsToitsu => Count == 2 && this[0] == this[1];
    /// <summary>
    /// 順子かどうか
    /// </summary>
    public bool IsShuntsu
    {
        get
        {
            if (!IsAllSameSuit)
            {
                return false;
            }

            var numTiles = tileKinds_.Where(x => x.IsNumber).ToList();
            return numTiles.Count == 3 &&
                numTiles[0].Value + 1 == numTiles[1].Value &&
                numTiles[1].Value + 1 == numTiles[2].Value;
        }
    }
    /// <summary>
    /// 刻子かどうか
    /// </summary>
    public bool IsKoutsu => Count == 3 && this[0] == this[1] && this[1] == this[2];
    /// <summary>
    /// 槓子かどうか
    /// </summary>
    public bool IsKantsu => Count == 4 && this[0] == this[1] && this[1] == this[2] && this[2] == this[3];

    /// <summary>
    /// インデクサー 指定されたインデックスの牌種別を取得します
    /// </summary>
    /// <param name="index">取得する牌種別のインデックス</param>
    /// <returns>指定されたインデックスの牌種別</returns>
    public TileKind this[int index] => tileKinds_[index];

    /// <summary>
    /// 牌種別リスト内の牌種別の数を取得します
    /// </summary>
    public int Count => tileKinds_.Count;

    /// <summary>
    /// 指定の牌種別のコレクションから新しい牌種別リストを初期化します
    /// </summary>
    /// <param name="tileKinds">初期化に使用する牌種別のコレクション</param>
    public TileKindList(IEnumerable<TileKind> tileKinds) : this()
    {
        var builder = ImmutableList.CreateBuilder<TileKind>();
        builder.AddRange(tileKinds);
        builder.Sort();
        tileKinds_ = builder.ToImmutable();
    }

    private TileKindList(ImmutableList<TileKind> immutableList) : this()
    {
        tileKinds_ = immutableList.Sort();
    }

    /// <summary>
    /// 文字列から牌種別リストを初期化します
    /// </summary>
    /// <param name="man">萬子の数字を並べた文字列</param>
    /// <param name="pin">筒子の数字を並べた文字列</param>
    /// <param name="sou">索子の数字を並べた文字列</param>
    /// <param name="honor">字牌を表す文字列 "tnsphrc" or "東南西北白發中"</param>
    public TileKindList(string man = "", string pin = "", string sou = "", string honor = "") : this()
    {
        var builder = ImmutableList.CreateBuilder<TileKind>();
        // 萬子の変換
        foreach (var c in man)
        {
            if (int.TryParse(c.ToString(), out var num))
            {
                if (num is >= 1 and <= 9)
                {
                    builder.Add(new TileKind(num - 1));
                }
                else { throw new ArgumentOutOfRangeException(nameof(man), num, "萬子の数字は1から9の範囲である必要があります。"); }
            }
            else { throw new ArgumentException($"入力された萬子の文字が正しくありません。 c:{c}", nameof(man)); }
        }
        // 筒子の変換
        foreach (var c in pin)
        {
            if (int.TryParse(c.ToString(), out var num))
            {
                if (num is >= 1 and <= 9)
                {
                    builder.Add(new TileKind(num + 8));
                }
                else { throw new ArgumentOutOfRangeException(nameof(pin), num, "筒子の数字は1から9の範囲である必要があります。"); }
            }
            else { throw new ArgumentException($"入力された筒子の文字が正しくありません。 c:{c}", nameof(pin)); }
        }
        // 索子の変換
        foreach (var c in sou)
        {
            if (int.TryParse(c.ToString(), out var num))
            {
                if (num is >= 1 and <= 9)
                {
                    builder.Add(new TileKind(num + 17));
                }
                else { throw new ArgumentOutOfRangeException(nameof(sou), num, "索子の数字は1から9の範囲である必要があります。"); }
            }
            else { throw new ArgumentException($"入力された索子の文字が正しくありません。 c:{c}", nameof(sou)); }
        }
        // 字牌の変換
        foreach (var c in honor)
        {
            var honorTileKind = c switch
            {
                't' or '東' => TileKind.Ton,
                'n' or '南' => TileKind.Nan,
                's' or '西' => TileKind.Sha,
                'p' or '北' => TileKind.Pei,
                'h' or '白' => TileKind.Haku,
                'r' or '發' => TileKind.Hatsu,
                'c' or '中' => TileKind.Chun,
                _ => throw new ArgumentException($"入力された字牌の文字が正しくありません。 c:{c}", nameof(honor))
            };
            builder.Add(honorTileKind);
        }
        builder.Sort((x, y) => x.Value.CompareTo(y.Value));
        tileKinds_ = builder.ToImmutable();
    }

    /// <summary>
    /// 指定された牌種別がこのリスト内に何個存在するかを数えます
    /// </summary>
    /// <param name="tileKind">数える対象の牌種別</param>
    /// <returns>指定された牌種別の数</returns>
    public int CountOf(TileKind tileKind)
    {
        return tileKinds_.Count(x => x == tileKind);
    }

    /// <summary>
    /// 指定された牌種別を牌種別リストに追加した新しい牌種別リストを返します
    /// </summary>
    /// <param name="tileKind">追加する牌種別</param>
    /// <returns>指定された牌種別が追加された新しい牌種別リスト</returns>
    public TileKindList Add(TileKind tileKind)
    {
        return [.. tileKinds_.Add(tileKind)];
    }

    /// <summary>
    /// 指定された牌種別のコレクションを牌種別リストに追加した新しい牌種別リストを返します
    /// </summary>
    /// <param name="tileKinds">追加する牌種別のコレクション</param>
    /// <returns>指定された牌種別が追加された新しい牌種別リスト</returns>
    public TileKindList AddRange(IEnumerable<TileKind> tileKinds)
    {
        return [.. tileKinds_.AddRange(tileKinds)];
    }

    /// <summary>
    /// 指定された牌種別をリストから指定された個数だけ削除した新しい牌種別リストを返します
    /// </summary>
    /// <param name="tileKind">削除する牌種別</param>
    /// <param name="count">削除する個数（デフォルトは1）</param>
    /// <returns>指定された牌種別を削除した新しい牌種別リスト</returns>
    /// <exception cref="ArgumentException">指定された牌種別が存在しないか、指定された個数だけ存在しない場合</exception>
    public TileKindList Remove(TileKind tileKind, int count = 1)
    {
        var builder = tileKinds_.ToBuilder();
        for (var i = 0; i < count; i++)
        {
            if (!builder.Remove(tileKind)) { throw new ArgumentException($"指定牌がありません。 tile:{tileKind} count:{count}", nameof(tileKind)); }
        }
#pragma warning disable IDE0028 // コレクションの初期化を簡略化します
        return new TileKindList(builder.ToImmutable());
#pragma warning restore IDE0028 // コレクションの初期化を簡略化します
    }

    /// <summary>
    /// 指定された牌種別のコレクションをリストから削除した新しい牌種別リストを返します
    /// </summary>
    /// <param name="tileKinds">削除する牌種別のコレクション</param>
    /// <returns>指定された牌種別を削除した新しい牌種別リスト</returns>
    /// <exception cref="ArgumentException">指定された牌種別が存在しない場合</exception>
    public TileKindList Remove(IEnumerable<TileKind> tileKinds)
    {
        var builder = tileKinds_.ToBuilder();
        foreach (var tile in tileKinds)
        {
            if (!builder.Remove(tile)) { throw new ArgumentException($"指定牌がありません。 tile:{tile}", nameof(tileKinds)); }
        }
        return [.. builder.ToImmutable()];
    }

    /// <summary>
    /// 指定された牌種別が牌種別リスト内で最初に見つかった位置のインデックスを返します
    /// </summary>
    /// <param name="tileKind">検索する牌種別</param>
    /// <returns>指定された牌種別が見つかった場合は0から始まるインデックス、見つからない場合は-1</returns>
    public int IndexOf(TileKind tileKind)
    {
        return tileKinds_.IndexOf(tileKind);
    }

    /// <summary>
    /// 牌種別リストを反復処理する列挙子を返します
    /// </summary>
    /// <returns>牌種別リストの各要素を反復処理するために使用する列挙子</returns>
    public IEnumerator<TileKind> GetEnumerator()
    {
        return tileKinds_.GetEnumerator();
    }

    /// <summary>
    /// 牌種別リストを反復処理する非ジェネリック列挙子を返します
    /// </summary>
    /// <returns>牌種別リストの各要素を反復処理するために使用する非ジェネリック列挙子</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 現在の牌種別リストと指定された牌種別リストを比較し、並び順での位置を示す値を返します
    /// </summary>
    /// <param name="other">比較する牌種別リスト</param>
    /// <returns>現在のインスタンスが引数より前にある場合は負の値、同じ位置の場合は0、後にある場合は正の値</returns>
    public int CompareTo(TileKindList? other)
    {
        if (other is null)
        {
            return 1;
        }

        var minCount = Math.Min(Count, other.Count);
        for (var i = 0; i < minCount; i++)
        {
            var comparison = this[i].CompareTo(other[i]);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        return Count.CompareTo(other.Count);
    }

    /// <summary>
    /// 指定された牌種別リストと現在の牌種別リストが等しいかどうかを判断します
    /// </summary>
    /// <param name="other">このオブジェクトと比較する牌種別リスト</param>
    /// <returns>指定された牌種別リストと現在の牌種別リストが等しい場合はtrue、それ以外の場合はfalse</returns>
    public virtual bool Equals(TileKindList? other)
    {
        return other is not null && (ReferenceEquals(this, other) || tileKinds_.SequenceEqual(other.tileKinds_));
    }

    /// <summary>
    /// オブジェクトのハッシュコードを計算します
    /// </summary>
    /// <returns>このオブジェクトのハッシュコード</returns>
    public override int GetHashCode()
    {
        return tileKinds_.Aggregate(0, (hash, tile) => hash * 31 + tile.GetHashCode());
    }

    /// <summary>
    /// 牌種別リストの文字列表現を取得します
    /// </summary>
    /// <returns>牌種別リストに含まれる全ての牌種別を連結した文字列</returns>
    public sealed override string ToString()
    {
        return string.Join("", this);
    }

    /// <summary>
    /// 最初の TileKindList が二番目の TileKindList より小さいかどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の TileKindList</param>
    /// <param name="right">比較する二番目の TileKindList</param>
    /// <returns>最初のインスタンスが二番目のインスタンスより小さい場合は true、それ以外の場合は false</returns>
    public static bool operator <(TileKindList? left, TileKindList? right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    /// <summary>
    /// 最初の TileKindList が二番目の TileKindList より大きいかどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の TileKindList</param>
    /// <param name="right">比較する二番目の TileKindList</param>
    /// <returns>最初のインスタンスが二番目のインスタンスより大きい場合は true、それ以外の場合は false</returns>
    public static bool operator >(TileKindList? left, TileKindList? right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    /// <summary>
    /// 最初の TileKindList が二番目の TileKindList 以下かどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の TileKindList</param>
    /// <param name="right">比較する二番目の TileKindList</param>
    /// <returns>最初のインスタンスが二番目のインスタンス以下の場合は true、それ以外の場合は false</returns>
    public static bool operator <=(TileKindList? left, TileKindList? right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// 最初の TileKindList が二番目の TileKindList 以上かどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の TileKindList</param>
    /// <param name="right">比較する二番目の TileKindList</param>
    /// <returns>最初のインスタンスが二番目のインスタンス以上の場合は true、それ以外の場合は false</returns>
    public static bool operator >=(TileKindList? left, TileKindList? right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// TileKindListのコレクションビルダーを提供するクラスです
    /// </summary>
    public static class TileKindListBuilder
    {
        /// <summary>
        /// 指定された牌の配列から新しい牌種別リストを作成します
        /// </summary>
        /// <param name="values">牌種別リストに含める牌種別の配列</param>
        /// <returns>新しい牌種別リスト</returns>
        public static TileKindList Create(ReadOnlySpan<TileKind> values)
        {
            var builder = ImmutableList.CreateBuilder<TileKind>();
            foreach (var value in values) { builder.Add(value); }
            return new TileKindList(builder.ToImmutable());
        }
    }
}
