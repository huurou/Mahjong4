namespace Mahjong.Lib.Scoring.Yakus.Impl;

/// <summary>
/// 裏ドラ
/// </summary>
public record Uradora : Yaku
{
    public override int Number => 53;
    public override string Name => "裏ドラ";
    public override int HanOpen => 0;
    public override int HanClosed => 1;
    public override bool IsYakuman => false;

    internal Uradora() { }
}
