using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Adoptions;

/// <summary>
/// 和了採用結果 ダブロン/トリプルロン対応
/// </summary>
public record AdoptedWinAction : AdoptedRoundAction
{
    /// <summary>和了者情報 (複数可)</summary>
    public ImmutableList<AdoptedWinner> WinnerIndices { get; init; }

    /// <summary>放銃者 ロン/槍槓では打牌者/加槓宣言者、ツモ/嶺上では和了者自身。
    /// ツモ/嶺上かの判定は <see cref="WinType"/> で行うこと</summary>
    public PlayerIndex LoserIndex { get; init; }

    /// <summary>和了種別</summary>
    public WinType WinType { get; init; }

    /// <summary>供託立直棒の受取情報 (供託がない場合は <see cref="KyoutakuRiichiAward.Count"/> = 0)</summary>
    public KyoutakuRiichiAward KyoutakuRiichiAward { get; init; }

    /// <summary>本場数 (本場ボーナス計算に使用)</summary>
    public Honba Honba { get; init; }

    /// <summary>親続行 (連荘) か</summary>
    public bool DealerContinues { get; init; }

    public AdoptedWinAction(
        ImmutableList<AdoptedWinner> winnerIndices,
        PlayerIndex loserIndex,
        WinType winType,
        KyoutakuRiichiAward kyoutakuRiichiAward,
        Honba honba,
        bool dealerContinues
    )
    {
        if (winnerIndices.Count == 0)
        {
            throw new ArgumentException("和了者は1人以上必要です。", nameof(winnerIndices));
        }

        if (winType is WinType.Tsumo or WinType.Rinshan)
        {
            if (loserIndex != winnerIndices[0].PlayerIndex)
            {
                throw new ArgumentException(
                    $"ツモ/嶺上和了では放銃者 = 和了者である必要があります。winType:{winType} loserIndex:{loserIndex.Value} winners[0]:{winnerIndices[0].PlayerIndex.Value}",
                    nameof(loserIndex)
                );
            }
        }
        else if (winType is WinType.Ron or WinType.Chankan)
        {
            if (winnerIndices.Any(x => x.PlayerIndex == loserIndex))
            {
                throw new ArgumentException(
                    $"ロン/槍槓和了では和了者と放銃者は別プレイヤーである必要があります。winType:{winType} loserIndex:{loserIndex.Value}",
                    nameof(loserIndex)
                );
            }
        }

        WinnerIndices = winnerIndices;
        LoserIndex = loserIndex;
        WinType = winType;
        KyoutakuRiichiAward = kyoutakuRiichiAward;
        Honba = honba;
        DealerContinues = dealerContinues;
    }

    public virtual bool Equals(AdoptedWinAction? other)
    {
        return other is not null &&
            WinnerIndices.SequenceEqual(other.WinnerIndices) &&
            LoserIndex == other.LoserIndex &&
            WinType == other.WinType &&
            KyoutakuRiichiAward == other.KyoutakuRiichiAward &&
            Honba == other.Honba &&
            DealerContinues == other.DealerContinues;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var winner in WinnerIndices)
        {
            hash.Add(winner);
        }
        hash.Add(LoserIndex);
        hash.Add(WinType);
        hash.Add(KyoutakuRiichiAward);
        hash.Add(Honba);
        hash.Add(DealerContinues);
        return hash.ToHashCode();
    }
}
