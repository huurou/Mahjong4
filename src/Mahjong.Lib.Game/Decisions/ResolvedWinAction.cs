using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Decisions;

/// <summary>
/// 和了採用結果 ダブロン/トリプルロン対応
/// </summary>
public record ResolvedWinAction : ResolvedRoundAction
{
    /// <summary>和了者情報 (複数可)</summary>
    public ImmutableList<ResolvedWinner> WinnerIndices { get; init; }

    /// <summary>放銃者 ロン/槍槓では打牌者/加槓宣言者、ツモ/嶺上では null</summary>
    public PlayerIndex? LoserIndex { get; init; }

    /// <summary>和了種別</summary>
    public WinType WinType { get; init; }

    /// <summary>供託立直棒の受取情報 (供託がない場合は null)</summary>
    public KyoutakuRiichiAward? KyoutakuRiichiAward { get; init; }

    /// <summary>本場数 (本場ボーナス計算に使用)</summary>
    public Honba Honba { get; init; }

    /// <summary>親続行 (連荘) か</summary>
    public bool DealerContinues { get; init; }

    public ResolvedWinAction(
        ImmutableList<ResolvedWinner> winnerIndices,
        PlayerIndex? loserIndex,
        WinType winType,
        KyoutakuRiichiAward? kyoutakuRiichiAward,
        Honba honba,
        bool dealerContinues
    )
    {
        if (winnerIndices.Count == 0)
        {
            throw new ArgumentException("和了者は1人以上必要です。", nameof(winnerIndices));
        }

        if (winType == WinType.Ron && loserIndex is null)
        {
            throw new ArgumentException("ロン和了では放銃者の指定が必要です。", nameof(loserIndex));
        }

        WinnerIndices = winnerIndices;
        LoserIndex = loserIndex;
        WinType = winType;
        KyoutakuRiichiAward = kyoutakuRiichiAward;
        Honba = honba;
        DealerContinues = dealerContinues;
    }

    public virtual bool Equals(ResolvedWinAction? other)
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
