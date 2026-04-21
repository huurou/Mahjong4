using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.GameStates.Impl;

/// <summary>
/// 和了による局終了イベント
/// </summary>
/// <param name="WinnerIndices">和了者 (複数はダブロン/トリプルロン)</param>
/// <param name="LoserIndex">放銃者 ロン/槍槓では打牌者/加槓宣言者、ツモ/嶺上では和了者自身 (= Winners[0])</param>
/// <param name="WinType">和了種別</param>
/// <param name="Winners">和了者毎の明細 (Index / 和了牌 / スコア計算結果)</param>
/// <param name="Honba">精算前の本場</param>
/// <param name="KyoutakuRiichiAward">供託立直棒の受取情報 (供託がない場合は <see cref="KyoutakuRiichiAward.Count"/> = 0)</param>
/// <param name="UraDoraIndicators">立直者が和了に含まれる場合の裏ドラ表示牌 (精算時点で枚数確定するため固定長)。
/// 立直者が一人もいない和了では空配列</param>
public record GameEventRoundEndedByWin(
    ImmutableArray<PlayerIndex> WinnerIndices,
    PlayerIndex LoserIndex,
    WinType WinType,
    ImmutableArray<AdoptedWinner> Winners,
    Honba Honba,
    KyoutakuRiichiAward KyoutakuRiichiAward,
    ImmutableArray<Tile> UraDoraIndicators
) : GameEvent
{
    public override string Name => "局終了(和了)";
}
