using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.GameStates;

namespace Mahjong.Lib.Game.Games;

/// <summary>
/// 対局終了条件の判定
/// </summary>
internal static class GameEndPolicy
{
    /// <summary>
    /// 現在の対局状態と局終了イベントから、対局を終了すべきかを判定します
    /// 呼び出し順: ApplyRoundResult → ShouldEndAfterRound → (false なら AdvanceToNextRound)
    /// </summary>
    /// <param name="game">持ち点・供託反映済みの Game</param>
    /// <param name="roundEndEvent">局終了イベント</param>
    /// <param name="dealerContinues">親続行するか (呼び出し側が Round / evt から算出)</param>
    public static bool ShouldEndAfterRound(Game game, GameEvent roundEndEvent, bool dealerContinues)
    {
        // トビ
        if (game.PointArray.Any(x => x.Value < game.Rules.BankruptThreshold))
        {
            return true;
        }

        // 規定局数消化
        if (IsLastRound(game.Rules.Format, game.RoundWind, game.RoundNumber) && !dealerContinues)
        {
            return true;
        }

        // オーラス親あがり止め (Phase 1 はフックのみ Phase 2 で実装)
        if (game.Rules.DealerWinStopAtOorasu && IsOorasuDealerWinStop(game, roundEndEvent))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// オーラス親あがり止めの判定 (Phase 1 は未実装のため常に false)
    /// </summary>
    private static bool IsOorasuDealerWinStop(Game game, GameEvent roundEndEvent)
    {
        return false;
    }

    /// <summary>
    /// 指定の局が対局形式上の最終局かを判定します
    /// </summary>
    private static bool IsLastRound(GameFormat format, RoundWind wind, RoundNumber number)
    {
        return format switch
        {
            GameFormat.SingleRound => true,
            GameFormat.Tonpuu => wind == RoundWind.East && number.Value == 3,
            GameFormat.Tonnan => wind == RoundWind.South && number.Value == 3,
            _ => throw new InvalidOperationException($"未対応の対局形式: {format}"),
        };
    }
}
