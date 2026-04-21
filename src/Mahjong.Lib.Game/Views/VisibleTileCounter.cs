using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Game.Views;

/// <summary>
/// PlayerRoundView から 指定牌種の見えていない枚数 (4 − 見えている枚数) を算出する静的ユーティリティ
/// 見えている枚数 = 自分の手牌 + 全プレイヤーの河 + 全プレイヤーの副露 (暗槓含む) + ドラ表示牌 に含まれる該当牌種の個数
/// </summary>
public static class VisibleTileCounter
{
    /// <summary>
    /// 指定の牌種が見えていない残り枚数を返します。下限は 0
    /// </summary>
    public static int CountUnseen(PlayerRoundView view, TileKind tileKind)
    {
        var visible = 0;
        visible += view.OwnHand.Count(x => x.Kind == tileKind);
        foreach (var river in view.RiverArray)
        {
            visible += river.Count(x => x.Kind == tileKind);
        }
        foreach (var callList in view.CallListArray)
        {
            foreach (var call in callList)
            {
                visible += call.Tiles.Count(x => x.Kind == tileKind);
            }
        }
        visible += view.DoraIndicators.Count(x => x.Kind == tileKind);
        return Math.Max(0, 4 - visible);
    }
}
