using System.Text;
using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.HandCalculating;
using Mahjong.Lib.Tiles;

Console.OutputEncoding = Encoding.UTF8;

// サンプル1: 役牌（白）
// 一一一 (2)(3)(4) 567 白白白中中 ロン中
PrintSample(
    "役牌（白）ロン",
    HandCalculator.Calc(
        new TileKindList(man: "111", pin: "234", sou: "567", honor: "hhhcc"),
        TileKind.Chun
    )
);

// サンプル2: 立直・平和・ツモ・一発（門前清自摸和・平和・立直・一発）
// 123m 234p 678p 34555s ツモ8p
PrintSample(
    "立直・一発・ツモ・平和",
    HandCalculator.Calc(
        new TileKindList(man: "123", pin: "234678", sou: "34555"),
        TileKind.Pin8,
        winSituation: new WinSituation
        {
            IsTsumo = true,
            IsRiichi = true,
            IsIppatsu = true,
        }
    )
);

// サンプル3: 断么九（鳴きあり）
// 23455m 234p 678p ポン444s ロン5m（カンチャン待ち）
PrintSample(
    "断么九（ポンあり）",
    HandCalculator.Calc(
        new TileKindList(man: "23455", pin: "234678"),
        TileKind.Man5,
        callList: [Call.Pon(sou: "444")]
    )
);

// サンプル4: 七対子・ドラ2
// 22m 55m 88m 33p 77p 44s 8s ツモ8s ドラ表示牌2p→3p → 33p がドラ2
PrintSample(
    "七対子・ドラ2",
    HandCalculator.Calc(
        new TileKindList(man: "225588", pin: "3377", sou: "4488"),
        TileKind.Sou8,
        doraIndicators: new TileKindList(pin: "2"),
        winSituation: new WinSituation { IsTsumo = true }
    )
);

// サンプル5: 国士無双十三面待ち
// 19m 19p 19s 東南西北白發中 + ロン中（13種すべて単騎）
PrintSample(
    "国士無双十三面待ち",
    HandCalculator.Calc(
        new TileKindList(man: "19", pin: "19", sou: "19", honor: "tnsphrcc"),
        TileKind.Chun
    )
);

// サンプル6: 清一色・対々和・三暗刻（親・ロン）
// 111m 333m 55m 777m 999m ロン9m 親
PrintSample(
    "清一色・対々和・三暗刻（親ロン）",
    HandCalculator.Calc(
        new TileKindList(man: "11133355777999"),
        TileKind.Man9,
        winSituation: new WinSituation { PlayerWind = Wind.East }
    )
);

static void PrintSample(string title, HandResult result)
{
    Console.WriteLine($"=== {title} ===");
    if (result.ErrorMessage is not null)
    {
        Console.WriteLine($"エラー: {result.ErrorMessage}");
    }
    else
    {
        Console.WriteLine($"役: {string.Join(", ", result.YakuList.Select(x => x.Name))}");
        Console.WriteLine($"符: {result.Fu}符");
        Console.WriteLine($"翻: {result.Han}翻");
        Console.WriteLine($"点数: {result.Score.Main}点" + (result.Score.Sub > 0 ? $"/{result.Score.Sub}点" : ""));
    }
    Console.WriteLine();
}
