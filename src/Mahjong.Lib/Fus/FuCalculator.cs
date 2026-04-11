using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Fus;

public static class FuCalculator
{
    public static FuList Calc(
        Hand hand,
        TileKind winTile,
        TileKindList winGroup,
        CallList? callList = null,
        WinSituation? winSituation = null,
        GameRules? gameRules = null
    )
    {
        callList ??= [];
        winSituation ??= new();
        gameRules ??= new();

        if (hand.Count == 7) { return [Fu.Chiitoitsu]; }
        FuList fuList = [
            .. CalcJantou(hand, winSituation),
            .. CalcMentsu(hand, winGroup, callList, winSituation),
            .. CalcWait(winTile, winGroup),
        ];
        fuList = CalcBase(fuList, hand, callList, winSituation, gameRules);
        return [.. fuList.OrderBy(x => x.Type)];
    }

    private static FuList CalcJantou(Hand hand, WinSituation winSituation)
    {
        var toitsuTile = hand.First(x => x.IsToitsu)[0];
        if (!toitsuTile.IsHonor) { return []; }

        FuList fuList = [];
        if (toitsuTile.IsDragon)
        {
            fuList = fuList.Add(Fu.JantouDragon);
        }
        else
        {
            if (toitsuTile == winSituation.PlayerWind.ToTileKind())
            {
                fuList = fuList.Add(Fu.JantouPlayerWind);
            }
            if (toitsuTile == winSituation.RoundWind.ToTileKind())
            {
                fuList = fuList.Add(Fu.JantouRoundWind);
            }
        }
        return fuList;
    }

    private static FuList CalcMentsu(Hand hand, TileKindList winGroup, CallList callList, WinSituation winSituation)
    {
        FuList fuList = [];
        // 手牌の明刻
        foreach (var minko in callList.Where(x => x.IsPon).Select(x => x.TileKindList))
        {
            fuList = fuList.Add(minko[0].IsChunchan ? Fu.MinkoChunchan : Fu.MinkoYaochu);
        }
        // シャンポン待ちロン和了のとき明刻扱いになる
        if (!winSituation.IsTsumo && winGroup.IsKoutsu)
        {
            fuList = fuList.Add(winGroup[0].IsChunchan ? Fu.MinkoChunchan : Fu.MinkoYaochu);
        }
        // 手牌の暗刻
        foreach (var anko in hand.Where(x => x.IsKoutsu && x != winGroup))
        {
            fuList = fuList.Add(anko[0].IsChunchan ? Fu.AnkoChunchan : Fu.AnkoYaochu);
        }
        // シャンポン待ちツモ和了のとき暗刻扱いになる
        if (winSituation.IsTsumo && winGroup.IsKoutsu)
        {
            fuList = fuList.Add(winGroup[0].IsChunchan ? Fu.AnkoChunchan : Fu.AnkoYaochu);
        }
        // 明槓
        foreach (var minkan in callList.Where(x => x.IsMinkan).Select(x => x.TileKindList))
        {
            fuList = fuList.Add(minkan[0].IsChunchan ? Fu.MinkanChunchan : Fu.MinkanYaochu);
        }
        // 暗槓
        foreach (var ankan in callList.Where(x => x.IsAnkan).Select(x => x.TileKindList))
        {
            fuList = fuList.Add(ankan[0].IsChunchan ? Fu.AnkanChunchan : Fu.AnkanYaochu);
        }
        return fuList;
    }

    private static FuList CalcWait(TileKind winTile, TileKindList winGroup)
    {
        FuList fuList = [];
        if (winTile.IsNumber && winGroup.IsShuntsu)
        {
            // ペンチャン待ち
            if (winTile.Number == 3 && winGroup.IndexOf(winTile) == 2 ||
                winTile.Number == 7 && winGroup.IndexOf(winTile) == 0)
            {
                fuList = fuList.Add(Fu.Penchan);
            }
            // カンチャン待ち
            if (winGroup.IndexOf(winTile) == 1)
            {
                fuList = fuList.Add(Fu.Kanchan);
            }
        }
        // 単騎待ち
        if (winGroup.IsToitsu)
        {
            fuList = fuList.Add(Fu.Tanki);
        }
        return fuList;
    }

    private static FuList CalcBase(FuList fuList, Hand hand, CallList callList, WinSituation winSituation, GameRules gameRules)
    {
        if (winSituation.IsTsumo)
        {
            // ピンヅモありの場合、ツモでもツモ符を加えない
            if (gameRules.PinzumoEnabled && fuList.Count == 0 && !callList.HasOpen) { }
            else
            {
                fuList = fuList.Add(Fu.Tsumo);
            }
        }
        // 食い平和のロンアガリは副底を30符にする
        if (!winSituation.IsTsumo && callList.HasOpen && fuList.Total == 0)
        {
            fuList = fuList.Add(Fu.FuteiOpenPinfu);
        }
        else
        {
            fuList = fuList.Add(Fu.Futei);
            // 門前ロン
            if (!winSituation.IsTsumo && !callList.HasOpen)
            {
                fuList = fuList.Add(Fu.Menzen);
            }
        }
        return fuList;
    }
}
