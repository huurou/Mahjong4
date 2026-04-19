using Mahjong.Lib.Game.Inquiries;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Responses;
using Mahjong.Lib.Game.States.RoundStates;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// 採用済み応答を <see cref="RoundStateContext"/> の対応するイベント発火に変換してディスパッチする抽象。
/// KanTsumo フェーズでアクション応答 (Dahai/Ankan/Kakan) が採用された場合、後続の AfterKanTsumo で消費する応答を戻り値で返す
/// </summary>
public interface IResponseDispatcher
{
    /// <summary>
    /// 通常フェーズの応答をディスパッチする
    /// </summary>
    /// <returns>KanTsumo でアクション応答が採用された場合、後続の AfterKanTsumo で消費すべき <see cref="PlayerResponse"/>。
    /// 通常フェーズ (およびフェーズ完了型) は null</returns>
    Task<PlayerResponse?> DispatchAsync(
        RoundStateContext context,
        RoundInquirySpec spec,
        ImmutableArray<AdoptedPlayerResponse> adopted
    );

    /// <summary>
    /// 直前の KanTsumo でペンディングしたアクション応答を AfterKanTsumo フェーズに改めてディスパッチする
    /// </summary>
    Task DispatchAfterKanTsumoAsync(RoundStateContext context, PlayerResponse pendingResponse);
}
