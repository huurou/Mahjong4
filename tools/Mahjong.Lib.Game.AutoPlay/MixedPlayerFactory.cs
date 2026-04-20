using Mahjong.Lib.Game.Players;

namespace Mahjong.Lib.Game.AutoPlay;

/// <summary>
/// プレイヤーインデックスごとに異なる <see cref="IPlayerFactory"/> を委譲する複合 Factory。
/// 対局 (= 4 回の <see cref="Create"/> 呼び出し) ごとに内部で席配置をランダムシャッフルし、
/// 起家バイアスを均して AI 比較の公平性を確保する。
/// </summary>
/// <remarks>
/// <see cref="Create"/> は 1 対局につき 4 回連続で呼ばれる前提。callIndex_ は 4 回周期で 0 に戻り、
/// 0 を検知するたびに席配置 currentAssignment_ をシャッフルする。
/// このため 1 対局で 4 回未満しか呼ばれない/5 回以上呼ばれる/対局途中に差し込まれる呼び出しが混ざると
/// 席配置が崩れて公平性が破れる。呼び出し側はこの契約 (1 対局 = 4 回の Create 連続呼び出し) を守ること
/// </remarks>
public sealed class MixedPlayerFactory : IPlayerFactory
{
    private readonly IPlayerFactory[] factories_;
    private readonly Random shuffleRng_;
    private readonly int[] currentAssignment_ = new int[PlayerIndex.PLAYER_COUNT];
    private int callIndex_;

    public MixedPlayerFactory(IPlayerFactory[] factories, Random shuffleRng)
    {
        if (factories.Length != PlayerIndex.PLAYER_COUNT)
        {
            throw new ArgumentException(
                $"factories は {PlayerIndex.PLAYER_COUNT} 席分必要です。実際:{factories.Length}席",
                nameof(factories)
            );
        }

        factories_ = factories;
        shuffleRng_ = shuffleRng;
        for (var i = 0; i < currentAssignment_.Length; i++)
        {
            currentAssignment_[i] = i;
        }
    }

    public string DisplayName => string.Join(" / ", factories_.Select(x => x.DisplayName).Distinct());

    public Player Create(PlayerIndex index, PlayerId id)
    {
        if (callIndex_ == 0)
        {
            Shuffle(currentAssignment_);
        }
        var factory = factories_[currentAssignment_[index.Value]];
        callIndex_ = (callIndex_ + 1) % PlayerIndex.PLAYER_COUNT;
        return factory.Create(index, id);
    }

    private void Shuffle(int[] arr)
    {
        for (var i = arr.Length - 1; i > 0; i--)
        {
            var j = shuffleRng_.Next(i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
}
