using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.States.GameStates.Impl;

/// <summary>
/// 和了による局終了イベント
/// </summary>
/// <param name="WinnerIndices">和了者 (複数はダブロン/トリプルロン)</param>
/// <param name="LoserIndex">放銃者 ロン/槍槓では打牌者/加槓宣言者、ツモ/嶺上では和了者自身 (= Winners[0])</param>
/// <param name="WinType">和了種別</param>
/// <param name="Winners">和了者毎の明細 (Index / 和了牌 / スコア計算結果)。未付与時は default</param>
/// <param name="Honba">精算前の本場 (null のときは新規 Honba(0) 扱い)</param>
/// <param name="KyoutakuRiichiAward">供託立直棒の受取情報 (供託がない場合は null)</param>
public record GameEventRoundEndedByWin(
    ImmutableArray<PlayerIndex> WinnerIndices,
    PlayerIndex LoserIndex,
    WinType WinType,
    ImmutableArray<AdoptedWinner> Winners = default,
    Honba? Honba = null,
    KyoutakuRiichiAward? KyoutakuRiichiAward = null
) : GameEvent
{
    public override string Name => "局終了(和了)";
}
