using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.States.RoundStates;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// <see cref="RoundEndedEventArgs"/> から通知・トレース層に流す <see cref="AdoptedRoundAction"/> を組み立てるヘルパ。
/// 通知 (<see cref="Notifications.WinNotification"/> / <see cref="Notifications.RyuukyokuNotification"/>) と
/// トレース (<see cref="IGameTracer.OnRoundEnded"/>) で同じ内容を共有するため同アセンブリ内で internal static として提供する
/// </summary>
internal static class AdoptedRoundActionBuilder
{
    public static AdoptedRoundAction Build(RoundEndedEventArgs args)
    {
        return args switch
        {
            RoundEndedByWinEventArgs win => new AdoptedWinAction(
                winnerIndices: [.. win.Winners],
                loserIndex: win.LoserIndex,
                winType: win.WinType,
                kyoutakuRiichiAward: win.KyoutakuRiichiAward,
                honba: win.Honba,
                uraDoraIndicators: win.UraDoraIndicators,
                dealerContinues: false
            ),
            RoundEndedByRyuukyokuEventArgs ryu => new AdoptedRyuukyokuAction(
                Type: ryu.Type,
                TenpaiPlayerIndices: [.. ryu.TenpaiPlayerIndices],
                NagashiManganPlayerIndices: [.. ryu.NagashiManganPlayerIndices],
                PointDeltas: ryu.PointDeltas,
                DealerContinues: false
            ),
            _ => throw new NotSupportedException($"未対応の局終了引数: {args?.GetType().Name}"),
        };
    }
}
