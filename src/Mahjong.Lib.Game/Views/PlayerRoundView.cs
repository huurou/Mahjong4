using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Views;

/// <summary>
/// プレイヤー視点でフィルタ済みの卓情報
/// 自分の手牌は見えるが他家の手牌は見えない 山の内部も見えない
/// </summary>
/// <param name="ViewerIndex">対象プレイヤーのインデックス</param>
/// <param name="RoundWind">場風</param>
/// <param name="RoundNumber">局数</param>
/// <param name="Honba">本場</param>
/// <param name="KyoutakuRiichiCount">供託リーチ棒の本数</param>
/// <param name="TurnIndex">現在の手番</param>
/// <param name="PointArray">各プレイヤーの持ち点</param>
/// <param name="OwnHand">自分の手牌</param>
/// <param name="CallListArray">全プレイヤーの副露 (公開情報)</param>
/// <param name="RiverArray">全プレイヤーの河 (公開情報)</param>
/// <param name="DoraIndicators">ドラ表示牌 (公開済みのもの)</param>
/// <param name="OwnStatus">自分の局内状態 (フリテン等の非公開情報を含む)</param>
/// <param name="OtherPlayerStatuses">他家の公開局内状態</param>
/// <param name="WallLiveRemaining">ツモ山の残り牌数</param>
public record PlayerRoundView(
    PlayerIndex ViewerIndex,
    RoundWind RoundWind,
    RoundNumber RoundNumber,
    Honba Honba,
    KyoutakuRiichiCount KyoutakuRiichiCount,
    PlayerIndex TurnIndex,
    PointArray PointArray,
    Hand OwnHand,
    CallListArray CallListArray,
    RiverArray RiverArray,
    ImmutableList<Tile> DoraIndicators,
    OwnRoundStatus OwnStatus,
    ImmutableArray<VisiblePlayerRoundStatus> OtherPlayerStatuses,
    int WallLiveRemaining
);
