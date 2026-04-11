using Mahjong.Lib.Tiles;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Mahjong.Lib.Shantens;

/// <summary>
/// シャンテン数計算のためのコンテキスト
/// </summary>
/// <param name="TileKindList">計算対象の牌種別リスト</param>
internal record ShantenContext(TileKindList TileKindList)
{
    /// <summary>
    /// 現在処理中の牌種別
    /// </summary>
    public TileKind Current { get; init; } = TileKind.Man1;

    /// <summary>
    /// 面子の数
    /// </summary>
    public int MentsuCount { get; init; }

    /// <summary>
    /// 対子の数
    /// </summary>
    public int ToitsuCount { get; init; }

    /// <summary>
    /// 塔子の数
    /// </summary>
    public int TatsuCount { get; init; }

    /// <summary>
    /// 槓子の数
    /// </summary>
    public int KantsuCount { get; init; }

    /// <summary>
    /// 孤立している牌の集合
    /// </summary>
    public IsolationSet IsolationSet { get; init; } = [];

    /// <summary>
    /// 槓子になっている数牌のリスト
    /// </summary>
    public ImmutableList<TileKind> KantsuNumbers { get; init; } = [];

    /// <summary>
    /// 刻子を除去した新しいコンテキストを返す
    /// </summary>
    /// <returns>更新されたシャンテンコンテキスト</returns>
    private ShantenContext RemoveKoutsu()
    {
        return this with
        {
            TileKindList = TileKindList.Remove(Current, 3),
            MentsuCount = MentsuCount + 1,
        };
    }

    /// <summary>
    /// 数牌をスキャンしてシャンテン数を計算する
    /// </summary>
    /// <returns>シャンテン数</returns>
    public int ScanNumber()
    {
        var context = this;
        while (context.TileKindList.CountOf(context.Current) == 0)
        {
            // 今のTileの次の種類の牌を取得する（数牌スキャン中は字牌に到達しないため Value+1 で安全にアクセス可能）
            var next = TileKind.All[context.Current.Value + 1];
            if (next.IsNumber)
            {
                context = context with { Current = next };
            }
            else
            {
                return context.CalcShanten();
            }
        }

        return context.TileKindList.CountOf(context.Current) switch
        {
            1 => context.ScanNumber1(),
            2 => context.ScanNumber2(),
            3 => context.ScanNumber3(),
            4 => context.ScanNumber4(),
            _ => throw new InvalidOperationException(),
        };
    }

    /// <summary>
    /// 字牌をスキャンし、面子や対子などの情報を更新したコンテキストを返す
    /// </summary>
    /// <returns>更新されたシャンテンコンテキスト</returns>
    public ShantenContext ScanHonor()
    {
        var context = this;
        foreach (var honor in TileKind.Honors)
        {
            context = context.TileKindList.CountOf(honor) switch
            {
                1 => context with { IsolationSet = [.. context.IsolationSet, honor] },
                2 => context with { ToitsuCount = context.ToitsuCount + 1 },
                3 => context with { MentsuCount = context.MentsuCount + 1 },
                4 => context with
                {
                    MentsuCount = context.MentsuCount + 1,
                    KantsuCount = context.KantsuCount + 1,
                    IsolationSet = [.. context.IsolationSet, honor],
                },
                _ => context,
            };
        }
        if (context.KantsuCount != 0 && context.TileKindList.Count % 3 == 2)
        {
            context = context with { KantsuCount = context.KantsuCount - 1 };
        }

        return context;
    }

    /// <summary>
    /// 対象の牌が1枚の場合の処理を行い、シャンテン数を計算する
    /// </summary>
    /// <returns>シャンテン数</returns>
    private int ScanNumber1()
    {
        List<int> shantens = [];
        if (Current.TryGetAtDistance(1, out var tile1) && Current.TryGetAtDistance(2, out var tile2) && Current.TryGetAtDistance(3, out var tile3) &&
            TileKindList.CountOf(tile1) == 1 && TileKindList.CountOf(tile2) > 0 && TileKindList.CountOf(tile3) != 4)
        {
            shantens.Add(RemoveShuntsu(tile1, tile2).ScanNumber());
        }
        else
        {
            shantens.Add(RemoveIsolation().ScanNumber());
            if (Current.TryGetAtDistance(1, out tile1) && Current.TryGetAtDistance(2, out tile2) &&
                TileKindList.CountOf(tile2) > 0)
            {
                if (TileKindList.CountOf(tile1) > 0)
                {
                    shantens.Add(RemoveShuntsu(tile1, tile2).ScanNumber());
                }
                // 嵌張候補（Current と Current+2）
                shantens.Add(RemoveTatsu(tile2).ScanNumber());
            }
            if (Current.TryGetAtDistance(1, out tile1) && TileKindList.CountOf(tile1) > 0)
            {
                // 両面候補（Current と Current+1）
                shantens.Add(RemoveTatsu(tile1).ScanNumber());
            }
        }

        return shantens.Min();
    }

    /// <summary>
    /// 対象の牌が2枚の場合の処理を行い、シャンテン数を計算する
    /// </summary>
    /// <returns>シャンテン数</returns>
    private int ScanNumber2()
    {
        List<int> shantens = [];
        shantens.Add(RemoveToitsu().ScanNumber());
        if (Current.TryGetAtDistance(1, out var tile1) && Current.TryGetAtDistance(2, out var tile2) &&
            TileKindList.CountOf(tile1) > 0 && TileKindList.CountOf(tile2) > 0)
        {
            shantens.Add(RemoveShuntsu(tile1, tile2).ScanNumber());
        }

        return shantens.Min();
    }

    /// <summary>
    /// 対象の牌が3枚の場合の処理を行い、シャンテン数を計算する
    /// </summary>
    /// <returns>シャンテン数</returns>
    private int ScanNumber3()
    {
        List<int> shantens = [];
        shantens.Add(RemoveKoutsu().ScanNumber());
        var toitsuRemoved = RemoveToitsu();
        if (Current.TryGetAtDistance(1, out var tile1) && Current.TryGetAtDistance(2, out var tile2) &&
            TileKindList.CountOf(tile1) > 0 && TileKindList.CountOf(tile2) > 0)
        {
            shantens.Add(toitsuRemoved.RemoveShuntsu(tile1, tile2).ScanNumber());
        }
        else
        {
            if (Current.TryGetAtDistance(2, out tile2) && toitsuRemoved.TileKindList.CountOf(tile2) > 0)
            {
                // 嵌張候補（Current と Current+2）
                shantens.Add(toitsuRemoved.RemoveTatsu(tile2).ScanNumber());
            }
            if (Current.TryGetAtDistance(1, out tile1) && toitsuRemoved.TileKindList.CountOf(tile1) > 0)
            {
                // 両面候補（Current と Current+1）
                shantens.Add(toitsuRemoved.RemoveTatsu(tile1).ScanNumber());
            }
        }
        if (Current.TryGetAtDistance(1, out tile1) && Current.TryGetAtDistance(2, out tile2) &&
            TileKindList.CountOf(tile1) >= 2 && TileKindList.CountOf(tile2) >= 2)
        {
            shantens.Add(RemoveShuntsu(tile1, tile2).RemoveShuntsu(tile1, tile2).ScanNumber());
        }

        return shantens.Min();
    }

    /// <summary>
    /// 対象の牌が4枚の場合の処理を行い、シャンテン数を計算する
    /// </summary>
    /// <returns>シャンテン数</returns>
    private int ScanNumber4()
    {
        List<int> shantens = [];
        var koutsuRemoved = RemoveKoutsu();
        TileKind? tile1;
        if (Current.TryGetAtDistance(2, out var tile2) && koutsuRemoved.TileKindList.CountOf(tile2) > 0)
        {
            // +2が同スートなら+1も必ず同スート（不変条件）
            var hasTile1 = Current.TryGetAtDistance(1, out tile1);
            Debug.Assert(hasTile1 && tile1 is not null);
            if (koutsuRemoved.TileKindList.CountOf(tile1) > 0)
            {
                shantens.Add(koutsuRemoved.RemoveShuntsu(tile1, tile2).ScanNumber());
            }
            // 嵌張候補（Current と Current+2）
            shantens.Add(koutsuRemoved.RemoveTatsu(tile2).ScanNumber());
        }
        if (Current.TryGetAtDistance(1, out tile1) && koutsuRemoved.TileKindList.CountOf(tile1) > 0)
        {
            // 両面候補（Current と Current+1）
            shantens.Add(koutsuRemoved.RemoveTatsu(tile1).ScanNumber());
        }
        shantens.Add(koutsuRemoved.RemoveIsolation().ScanNumber());

        var toitsuRemoved = RemoveToitsu();
        if (Current.TryGetAtDistance(2, out tile2) && toitsuRemoved.TileKindList.CountOf(tile2) > 0)
        {
            // +2が同スートなら+1も必ず同スート（不変条件）
            var hasTile1 = Current.TryGetAtDistance(1, out tile1);
            Debug.Assert(hasTile1 && tile1 is not null);
            if (toitsuRemoved.TileKindList.CountOf(tile1) > 0)
            {
                shantens.Add(toitsuRemoved.RemoveShuntsu(tile1, tile2).ScanNumber());
            }
            // 嵌張候補（Current と Current+2）
            shantens.Add(toitsuRemoved.RemoveTatsu(tile2).ScanNumber());
        }
        if (Current.TryGetAtDistance(1, out tile1) && toitsuRemoved.TileKindList.CountOf(tile1) > 0)
        {
            // 両面候補（Current と Current+1）
            shantens.Add(toitsuRemoved.RemoveTatsu(tile1).ScanNumber());
        }

        return shantens.Min();
    }

    /// <summary>
    /// 面子や対子などの情報からシャンテン数を計算する
    /// </summary>
    /// <returns>シャンテン数</returns>
    private int CalcShanten()
    {
        var shanten = 8 - MentsuCount * 2 - ToitsuCount - TatsuCount;
        var mentsuKouho = MentsuCount + TatsuCount;
        if (ToitsuCount != 0)
        {
            mentsuKouho += ToitsuCount - 1;
        }
        // 同種の数牌を4枚持っているときに刻子&単騎待ちとみなされないようにする
        else if (IsolationSet.Count > 0 && KantsuNumbers.Count > 0 && IsolationSet.All(KantsuNumbers.Contains))
        {
            shanten++;
        }
        if (mentsuKouho > 4)
        {
            shanten += mentsuKouho - 4;
        }
        if (shanten != ShantenConstants.SHANTEN_AGARI && shanten < KantsuCount)
        {
            shanten = KantsuCount;
        }

        return shanten;
    }

    /// <summary>
    /// 順子を除去した新しいコンテキストを返す
    /// </summary>
    /// <param name="tileKind1">順子の2つ目の牌種別</param>
    /// <param name="tileKind2">順子の3つ目の牌種別</param>
    /// <returns>更新されたシャンテンコンテキスト</returns>
    private ShantenContext RemoveShuntsu(TileKind tileKind1, TileKind tileKind2)
    {
        return this with
        {
            TileKindList = TileKindList.Remove([Current, tileKind1, tileKind2]),
            MentsuCount = MentsuCount + 1,
        };
    }

    /// <summary>
    /// 対子を除去した新しいコンテキストを返す
    /// </summary>
    /// <returns>更新されたシャンテンコンテキスト</returns>
    private ShantenContext RemoveToitsu()
    {
        return this with
        {
            TileKindList = TileKindList.Remove(Current, 2),
            ToitsuCount = ToitsuCount + 1,
        };
    }

    /// <summary>
    /// 塔子（両面・嵌張）を除去した新しいコンテキストを返す
    /// </summary>
    /// <param name="tileKind">除去する相手の牌種別（Current+1 なら両面、Current+2 なら嵌張）</param>
    /// <returns>更新されたシャンテンコンテキスト</returns>
    private ShantenContext RemoveTatsu(TileKind tileKind)
    {
        return this with
        {
            TileKindList = TileKindList.Remove([Current, tileKind]),
            TatsuCount = TatsuCount + 1,
        };
    }

    /// <summary>
    /// 孤立牌を除去した新しいコンテキストを返す 孤立牌のリストを更新する
    /// </summary>
    /// <returns>更新されたシャンテンコンテキスト</returns>
    private ShantenContext RemoveIsolation()
    {
        return this with
        {
            TileKindList = TileKindList.Remove(Current),
            IsolationSet = [.. IsolationSet, Current],
        };
    }
}
