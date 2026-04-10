using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 一気通貫
/// </summary>
public record Ittsuu : Yaku
{
    public override int Number => 24;
    public override string Name => "一気通貫";
    public override int HanOpen => 1;
    public override int HanClosed => 2;
    public override bool IsYakuman => false;

    internal Ittsuu() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        var shuntsus = hand.CombineFuuro(callList).Where(x => x.IsShuntsu);
        if (shuntsus.Count() < 3) { return false; }
        var suits = new[]
        {
            shuntsus.Where(x=>x.IsAllMan),
            shuntsus.Where(x=>x.IsAllPin),
            shuntsus.Where(x=>x.IsAllSou),
        };
        foreach (var suit in suits)
        {
            if (suit.Count() < 3) { continue; }
            var values = suit.Select(x => x.Where(x => x.IsNumber).Select(y => y.Number));
            return values.Any(x => x.SequenceEqual([1, 2, 3])) &&
                values.Any(x => x.SequenceEqual([4, 5, 6])) &&
                values.Any(x => x.SequenceEqual([7, 8, 9]));
        }
        return false;
    }
}
