using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Adoptions;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds;

/// <summary>
/// 和了精算 (<see cref="Round.SettleWin"/>) の副産物。通知層に役情報・点数情報・本場・供託を渡すために保持する
/// </summary>
/// <param name="Winners">和了者毎の詳細 (Index / 和了牌 / 役情報を含む ScoreResult)</param>
/// <param name="Honba">精算前の本場 (本場加算対象の集計表示に使用)</param>
/// <param name="KyoutakuRiichiAward">供託立直棒の受取情報 (供託がない場合は <see cref="KyoutakuRiichiAward.Count"/> = 0)</param>
public record WinSettlementDetails(
    ImmutableArray<AdoptedWinner> Winners,
    Honba Honba,
    KyoutakuRiichiAward KyoutakuRiichiAward
);
