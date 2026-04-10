using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Yakus.Impl;

/// <summary>
/// 断么九
/// </summary>
public record Tanyao : Yaku
{
    public override int Number => 8;
    public override string Name => "断么九";
    public override int HanOpen => 1;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Tanyao() { }

    public static bool Valid(Hand hand, CallList callList, GameRules gameRules)
    {
        return hand.CombineFuuro(callList).SelectMany(x => x).All(x => x.IsChunchan) &&
            (!callList.HasOpen || gameRules.KuitanEnabled);
    }
}
