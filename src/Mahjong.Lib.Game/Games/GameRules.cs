using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Games;

/// <summary>
/// 対局ルール 対局前に決まる要素
/// </summary>
public record GameRules
{
    /// <summary>
    /// 対局形式 (既定: 東南戦)
    /// </summary>
    public GameFormat Format { get; init; } = GameFormat.Tonnan;

    /// <summary>
    /// 赤ドラとして扱う牌の集合 (既定: 赤五萬1/赤五筒1/赤五索1)
    /// </summary>
    public ImmutableHashSet<Tile> RedDoraTiles { get; init; } = [new Tile(16), new Tile(52), new Tile(88)];

    /// <summary>
    /// 初期持ち点 (既定: 35000)
    /// </summary>
    public int InitialPoints { get; init; } = 35000;

    /// <summary>
    /// オーラス親あがり止め (1位単独確定のみ)
    /// </summary>
    public bool DealerWinStopAtAllLast { get; init; } = true;

    /// <summary>
    /// トビ終了点 この値未満で対局終了 (既定: 0)
    /// </summary>
    public int BankruptThreshold { get; init; }

    /// <summary>
    /// 食いタン有効
    /// </summary>
    public bool KuitanAllowed { get; init; } = true;

    /// <summary>
    /// 後付け有効
    /// </summary>
    public bool AtozukeAllowed { get; init; } = true;

    /// <summary>
    /// 連荘条件
    /// </summary>
    public RenchanCondition RenchanCondition { get; init; } = RenchanCondition.AgariOrTenpai;

    /// <summary>
    /// 暗槓に対する国士無双の槍槓 (暗槓チャンカン) を許可するかどうか (既定: true)
    /// </summary>
    public bool AllowAnkanChankanForKokushi { get; init; } = true;

    /// <summary>
    /// 指定された牌が赤ドラであるかを判定します
    /// </summary>
    public bool IsRedDora(Tile tile)
    {
        return RedDoraTiles.Contains(tile);
    }
}
