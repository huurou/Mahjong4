namespace Mahjong.Lib.Game.States.RoundStates.Impl;

/// <summary>
/// 副露
/// </summary>
public record RoundStateCall : RoundState
{
    public override string Name => "副露";

    public override void Entry(RoundStateContext context)
    {
        base.Entry(context);

        // TODO: 副露処理（河から打牌を除去、副露を生成）

        if (IsDaiminkan())
        {
            Transit(context, new RoundStateKanTsumo());
        }
        else
        {
            Transit(context, new RoundStateDahai());
        }
    }

    private bool IsDaiminkan()
    {
        // TODO: 直前の副露が大明槓であるか判定
        return false;
    }
}
