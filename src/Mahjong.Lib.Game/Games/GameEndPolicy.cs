using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.States.GameStates;
using Mahjong.Lib.Game.States.GameStates.Impl;

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
        if (game.Rules.DealerWinStopAtAllLast && IsAllLastDealerWinStop(game, roundEndEvent))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// オーラス親あがり止めの判定
    /// (a) 対局形式の最終局 (b) 和了終了 (c) 親が和了者に含まれる (d) 親が1位単独確定
    /// を全て満たす場合 true を返す
    /// </summary>
    private static bool IsAllLastDealerWinStop(Game game, GameEvent roundEndEvent)
    {
        if (!IsLastRound(game.Rules.Format, game.RoundWind, game.RoundNumber))
        {
            return false;
        }
        if (roundEndEvent is not GameEventRoundEndedByWin win)
        {
            return false;
        }

        var dealerIndex = game.RoundNumber.ToDealer();
        if (!win.WinnerIndices.Contains(dealerIndex))
        {
            return false;
        }

        var dealerPoint = game.PointArray[dealerIndex].Value;
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            if (i == dealerIndex.Value) { continue; }
            if (game.PointArray[new PlayerIndex(i)].Value >= dealerPoint)
            {
                return false;
            }
        }
        return true;
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
