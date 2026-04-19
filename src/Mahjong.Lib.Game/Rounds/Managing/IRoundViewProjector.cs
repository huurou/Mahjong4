using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Views;

namespace Mahjong.Lib.Game.Rounds.Managing;

/// <summary>
/// Round から指定プレイヤー視点の PlayerRoundView を生成する
/// </summary>
public interface IRoundViewProjector
{
    /// <summary>
    /// 指定プレイヤー視点で情報フィルタ済みの卓情報を生成する
    /// </summary>
    PlayerRoundView Project(Round round, PlayerIndex viewerIndex);
}
