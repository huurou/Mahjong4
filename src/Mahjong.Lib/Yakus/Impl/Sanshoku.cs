using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 三色同順
/// </summary>
public record Sanshoku : Yaku
{
    public override int Number => 25;
    public override string Name => "三色同順";
    public override int HanOpen => 1;
    public override int HanClosed => 2;
    public override bool IsYakuman => false;

    internal Sanshoku() { }

    public static bool Valid(Hand hand, CallList callList)
    {
        var shuntsus = hand.CombineFuuro(callList).Where(x => x.IsShuntsu);
        if (shuntsus.Count() < 3) { return false; }
        var mans = shuntsus.Where(x => x[0].IsMan);
        var pins = shuntsus.Where(x => x[0].IsPin);
        var sous = shuntsus.Where(x => x[0].IsSou);
        foreach (var man in mans)
        {
            foreach (var pin in pins)
            {
                foreach (var sou in sous)
                {
                    var manValues = man.Where(x => x.IsNumber).Select(x => x.Number);
                    var pinValues = pin.Where(x => x.IsNumber).Select(x => x.Number);
                    var souValues = sou.Where(x => x.IsNumber).Select(x => x.Number);
                    if (manValues.SequenceEqual(pinValues) && pinValues.SequenceEqual(souValues)) { return true; }
                }
            }
        }
        return false;
    }
}
