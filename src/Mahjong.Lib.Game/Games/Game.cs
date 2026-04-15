using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Walls;

namespace Mahjong.Lib.Game.Games;

/// <summary>
/// 対局
/// PlayerList の index 0 が起家となる仕様。起家決定・並び替えは呼び出し側の責務
/// </summary>
/// <param name="PlayerList">プレイヤー席のリスト (index 0 が起家)</param>
/// <param name="Rules">対局ルール</param>
/// <param name="RoundWind">場風</param>
/// <param name="RoundNumber">局数</param>
/// <param name="Honba">本場</param>
/// <param name="KyoutakuRiichiCount">供託リーチ棒の本数</param>
/// <param name="PointArray">各プレイヤーの持ち点</param>
public record Game(
    PlayerList PlayerList,
    GameRules Rules,
    RoundWind RoundWind,
    RoundNumber RoundNumber,
    Honba Honba,
    KyoutakuRiichiCount KyoutakuRiichiCount,
    PointArray PointArray
)
{
    /// <summary>
    /// 指定されたプレイヤーとルールから初期状態の対局を生成します
    /// 東一局 0本場、初期持ち点は Rules.InitialPoints
    /// </summary>
    public static Game Create(PlayerList players, GameRules rules)
    {
        return new Game(
            players,
            rules,
            RoundWind.East,
            new RoundNumber(0),
            new Honba(0),
            new KyoutakuRiichiCount(0),
            new PointArray(new Point(rules.InitialPoints))
        );
    }

    /// <summary>
    /// 現在の対局状態から次局用の Round を生成します
    /// </summary>
    internal Round CreateRound(IWallGenerator wallGenerator)
    {
        return new Round(
            RoundWind,
            RoundNumber,
            Honba,
            KyoutakuRiichiCount,
            RoundNumber.ToDealer(),
            PointArray,
            wallGenerator
        );
    }

    /// <summary>
    /// 局終了時の Round から持ち点・供託を取り込みます
    /// </summary>
    internal Game ApplyRoundResult(Round endedRound)
    {
        return this with
        {
            PointArray = endedRound.PointArray,
            KyoutakuRiichiCount = endedRound.KyoutakuRiichiCount,
        };
    }

    /// <summary>
    /// 次局へ進めます
    /// RoundWind は東→南→西→北→東 と循環する仕様
    /// </summary>
    /// <param name="mode">次局への進め方</param>
    internal Game AdvanceToNextRound(RoundAdvanceMode mode)
    {
        if (mode == RoundAdvanceMode.Renchan)
        {
            return this with
            {
                Honba = new Honba(Honba.Value + 1),
            };
        }

        var nextHonba = mode == RoundAdvanceMode.DealerChangeWithHonba
            ? new Honba(Honba.Value + 1)
            : new Honba(0);
        var nextNumber = RoundNumber.Value + 1;
        return nextNumber <= 3
            ? this with
            {
                RoundNumber = new RoundNumber(nextNumber),
                Honba = nextHonba,
            }
            : this with
            {
                RoundWind = RoundWind.FromValue((RoundWind.Value + 1) % 4),
                RoundNumber = new RoundNumber(0),
                Honba = nextHonba,
            };
    }
}
