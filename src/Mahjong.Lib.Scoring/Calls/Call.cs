using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Calls;

/// <summary>
/// 副露を表現するクラス
/// </summary>
public record Call : IComparable<Call>
{
    /// <summary>
    /// 副露の種類
    /// </summary>
    public CallType Type { get; }
    /// <summary>
    /// 副露を構成する牌種別のリスト
    /// </summary>
    public TileKindList TileKindList { get; }

    /// <summary>
    /// チーかどうか
    /// </summary>
    public bool IsChi => Type == CallType.Chi;
    /// <summary>
    /// ポンかどうか
    /// </summary>
    public bool IsPon => Type == CallType.Pon;
    /// <summary>
    /// 槓かどうか
    /// </summary>
    public bool IsKan => Type is CallType.Ankan or CallType.Minkan;
    /// <summary>
    /// 暗槓かどうか
    /// </summary>
    public bool IsAnkan => Type == CallType.Ankan;
    /// <summary>
    /// 明槓かどうか
    /// </summary>
    public bool IsMinkan => Type == CallType.Minkan;
    /// <summary>
    /// 抜きかどうか
    /// </summary>
    public bool IsNuki => Type == CallType.Nuki;
    /// <summary>
    /// 門前でないかどうか
    /// </summary>
    public bool IsOpen => Type is CallType.Chi or CallType.Pon or CallType.Minkan or CallType.Nuki;

    /// <summary>
    /// 副露のインスタンスを初期化します
    /// </summary>
    /// <param name="type">副露の種類</param>
    /// <param name="tileKindList">副露を構成する牌種別のリスト</param>
    /// <exception cref="ArgumentException">副露の種類と牌の構成が一致しない場合</exception>
    public Call(CallType type, TileKindList tileKindList)
    {
        switch (type)
        {
            case CallType.Chi:
                if (!tileKindList.IsShuntsu)
                {
                    throw new ArgumentException($"チーの構成牌は順子でなければなりません。tileKindList:{tileKindList}", nameof(tileKindList));
                }
                break;

            case CallType.Pon:
                if (!tileKindList.IsKoutsu)
                {
                    throw new ArgumentException($"ポンの構成牌は刻子でなければなりません。tileKindList:{tileKindList}", nameof(tileKindList));
                }
                break;

            case CallType.Ankan:
                if (!tileKindList.IsKantsu)
                {
                    throw new ArgumentException($"暗槓の構成牌は槓子でなければなりません。tileKindList:{tileKindList}", nameof(tileKindList));
                }
                break;

            case CallType.Minkan:
                if (!tileKindList.IsKantsu)
                {
                    throw new ArgumentException($"明槓の構成牌は槓子でなければなりません。tileKindList:{tileKindList}", nameof(tileKindList));
                }
                break;

            case CallType.Nuki:
                if (tileKindList.Count == 0)
                {
                    throw new ArgumentException($"抜きの構成牌は1つ以上の牌を含む必要があります。tileKindList:{tileKindList}", nameof(tileKindList));
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        Type = type;
        TileKindList = tileKindList;
    }

    /// <summary>
    /// チーの副露を作成します
    /// </summary>
    /// <param name="tileKindList">チーを構成する牌種別のリスト（順子である必要があります）</param>
    /// <returns>チーの副露</returns>
    /// <exception cref="ArgumentException">牌の構成が順子でない場合</exception>
    public static Call Chi(TileKindList tileKindList)
    {
        return new Call(CallType.Chi, tileKindList);
    }
    /// <summary>
    /// 牌の文字列表現からチーの副露を作成します
    /// </summary>
    /// <param name="man">萬子の文字列表現</param>
    /// <param name="pin">筒子の文字列表現</param>
    /// <param name="sou">索子の文字列表現</param>
    /// <param name="honor">字牌の文字列表現</param>
    /// <returns>チーの副露</returns>
    /// <exception cref="ArgumentException">牌の構成が順子でない場合</exception>
    public static Call Chi(string man = "", string pin = "", string sou = "", string honor = "")
    {
        return new Call(CallType.Chi, new TileKindList(man, pin, sou, honor));
    }

    /// <summary>
    /// ポンの副露を作成します
    /// </summary>
    /// <param name="tileKindList">ポンを構成する牌種別のリスト（刻子である必要があります）</param>
    /// <returns>ポンの副露</returns>
    /// <exception cref="ArgumentException">牌の構成が刻子でない場合</exception>
    public static Call Pon(TileKindList tileKindList)
    {
        return new Call(CallType.Pon, tileKindList);
    }
    /// <summary>
    /// 牌の文字列表現からポンの副露を作成します
    /// </summary>
    /// <param name="man">萬子の文字列表現</param>
    /// <param name="pin">筒子の文字列表現</param>
    /// <param name="sou">索子の文字列表現</param>
    /// <param name="honor">字牌の文字列表現</param>
    /// <returns>ポンの副露</returns>
    /// <exception cref="ArgumentException">牌の構成が刻子でない場合</exception>
    public static Call Pon(string man = "", string pin = "", string sou = "", string honor = "")
    {
        return new Call(CallType.Pon, new TileKindList(man, pin, sou, honor));
    }

    /// <summary>
    /// 暗槓の副露を作成します
    /// </summary>
    /// <param name="tileKindList">暗槓を構成する牌種別のリスト（槓子である必要があります）</param>
    /// <returns>暗槓の副露</returns>
    /// <exception cref="ArgumentException">牌の構成が槓子でない場合</exception>
    public static Call Ankan(TileKindList tileKindList)
    {
        return new Call(CallType.Ankan, tileKindList);
    }
    /// <summary>
    /// 牌の文字列表現から暗槓の副露を作成します
    /// </summary>
    /// <param name="man">萬子の文字列表現</param>
    /// <param name="pin">筒子の文字列表現</param>
    /// <param name="sou">索子の文字列表現</param>
    /// <param name="honor">字牌の文字列表現</param>
    /// <returns>暗槓の副露</returns>
    /// <exception cref="ArgumentException">牌の構成が槓子でない場合</exception>
    public static Call Ankan(string man = "", string pin = "", string sou = "", string honor = "")
    {
        return new Call(CallType.Ankan, new TileKindList(man, pin, sou, honor));
    }

    /// <summary>
    /// 明槓の副露を作成します
    /// </summary>
    /// <param name="tileKindList">明槓を構成する牌種別のリスト（槓子である必要があります）</param>
    /// <returns>明槓の副露</returns>
    /// <exception cref="ArgumentException">牌の構成が槓子でない場合</exception>
    public static Call Minkan(TileKindList tileKindList)
    {
        return new Call(CallType.Minkan, tileKindList);
    }
    /// <summary>
    /// 牌の文字列表現から明槓の副露を作成します
    /// </summary>
    /// <param name="man">萬子の文字列表現</param>
    /// <param name="pin">筒子の文字列表現</param>
    /// <param name="sou">索子の文字列表現</param>
    /// <param name="honor">字牌の文字列表現</param>
    /// <returns>明槓の副露</returns>
    /// <exception cref="ArgumentException">牌の構成が槓子でない場合</exception>
    public static Call Minkan(string man = "", string pin = "", string sou = "", string honor = "")
    {
        return new Call(CallType.Minkan, new TileKindList(man, pin, sou, honor));
    }

    /// <summary>
    /// 抜きの副露を作成します
    /// </summary>
    /// <param name="tileKindList">抜きを構成する牌種別のリスト（1つ以上の牌を含む必要があります）</param>
    /// <returns>抜きの副露</returns>
    /// <exception cref="ArgumentException">牌の構成が空の場合</exception>
    public static Call Nuki(TileKindList tileKindList)
    {
        return new Call(CallType.Nuki, tileKindList);
    }
    /// <summary>
    /// 牌の文字列表現から抜きの副露を作成します
    /// </summary>
    /// <param name="man">萬子の文字列表現</param>
    /// <param name="pin">筒子の文字列表現</param>
    /// <param name="sou">索子の文字列表現</param>
    /// <param name="honor">字牌の文字列表現</param>
    /// <returns>抜きの副露</returns>
    /// <exception cref="ArgumentException">牌の構成が空の場合</exception>
    public static Call Nuki(string man = "", string pin = "", string sou = "", string honor = "")
    {
        return new Call(CallType.Nuki, new TileKindList(man, pin, sou, honor));
    }

    /// <summary>
    /// 現在のインスタンスを指定したオブジェクトと比較し、並び順での相対位置を示す整数を返します。
    /// CallTypeを第一ソートキー、TileKindListを第二ソートキーとして比較します。
    /// </summary>
    /// <param name="other">比較するオブジェクト</param>
    /// <returns>
    /// 現在のインスタンスが比較対象オブジェクトより前にある場合は負の値、
    /// 同じ位置にある場合は 0、
    /// 後にある場合は正の値
    /// </returns>
    public int CompareTo(Call? other)
    {
        if (other is null)
        {
            return 1;
        }

        var typeComparison = Type.CompareTo(other.Type);
        if (typeComparison != 0)
        {
            return typeComparison;
        }

        return TileKindList.CompareTo(other.TileKindList);
    }

    /// <summary>
    /// 最初の Call が二番目の Call より小さいかどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の Call</param>
    /// <param name="right">比較する二番目の Call</param>
    /// <returns>最初のインスタンスが二番目のインスタンスより小さい場合は true、それ以外の場合は false</returns>
    public static bool operator <(Call? left, Call? right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    /// <summary>
    /// 最初の Call が二番目の Call より大きいかどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の Call</param>
    /// <param name="right">比較する二番目の Call</param>
    /// <returns>最初のインスタンスが二番目のインスタンスより大きい場合は true、それ以外の場合は false</returns>
    public static bool operator >(Call? left, Call? right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    /// <summary>
    /// 最初の Call が二番目の Call 以下かどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の Call</param>
    /// <param name="right">比較する二番目の Call</param>
    /// <returns>最初のインスタンスが二番目のインスタンス以下の場合は true、それ以外の場合は false</returns>
    public static bool operator <=(Call? left, Call? right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// 最初の Call が二番目の Call 以上かどうかを判定します
    /// </summary>
    /// <param name="left">比較する最初の Call</param>
    /// <param name="right">比較する二番目の Call</param>
    /// <returns>最初のインスタンスが二番目のインスタンス以上の場合は true、それ以外の場合は false</returns>
    public static bool operator >=(Call? left, Call? right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// 副露の文字列表現を返します
    /// </summary>
    /// <returns>副露の種類と構成牌を含む文字列</returns>
    public override string ToString()
    {
        return $"{Type.ToStr()}-{TileKindList}";
    }
}
