using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games.Scoring;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rivers;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Game.Walls;
using System.Collections.Immutable;

namespace Mahjong.Lib.Game.Rounds;

/// <summary>
/// 局
/// 対局開始時にプレイヤーに0-3のPlayerIndexをランダムに振り分ける
/// PlayerIndex0が起家
/// 局ではプレイヤーではなくそのPlayerIndexでどのプレイヤーの行動かを管理する
/// </summary>
/// <param name="RoundWind">場風</param>
/// <param name="RoundNumber">局数</param>
/// <param name="Honba">本場</param>
/// <param name="KyoutakuRiichiCount">供託リーチ棒の本数</param>
/// <param name="Turn">手番</param>
/// <param name="PointArray">各プレイヤーの持ち点の配列</param>
/// <param name="Wall">山牌</param>
public record Round(
    RoundWind RoundWind,
    RoundNumber RoundNumber,
    Honba Honba,
    KyoutakuRiichiCount KyoutakuRiichiCount,
    PlayerIndex Turn,
    PointArray PointArray,
    Wall Wall
)
{
    /// <summary>
    /// 各プレイヤーの手牌の配列
    /// </summary>
    public HandArray HandArray { get; init; } = new HandArray();

    /// <summary>
    /// 各プレイヤーの副露リストの配列
    /// </summary>
    public CallListArray CallListArray { get; init; } = new CallListArray();

    /// <summary>
    /// 各プレイヤーの河の配列
    /// </summary>
    public RiverArray RiverArray { get; init; } = new RiverArray();

    /// <summary>
    /// 加槓・大明槓による保留中のカンドラめくりが存在するか
    /// (嶺上ツモ直前に消化される)
    /// </summary>
    public bool PendingDoraReveal { get; init; }

    /// <summary>
    /// 各プレイヤーの局内状態 (門前・立直・一発・フリテン等)
    /// </summary>
    public PlayerRoundStatusArray PlayerRoundStatusArray { get; init; } = new PlayerRoundStatusArray();

    /// <summary>
    /// 立直宣言を保留中のプレイヤー (高々1人)。PlayerRoundStatus.IsPendingRiichi から導出
    /// </summary>
    public PlayerIndex? PendingRiichiPlayerIndex
    {
        get
        {
            for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
            {
                var playerIndex = new PlayerIndex(i);
                if (PlayerRoundStatusArray[playerIndex].IsPendingRiichi) { return playerIndex; }
            }
            return null;
        }
    }

    /// <summary>
    /// 配牌を行います。親から反時計回りに4枚ずつ3周+1枚ずつ1周で各プレイヤー13枚。
    /// </summary>
    internal Round Haipai()
    {
        var wall = Wall;
        var handArray = HandArray;
        var dealerIndex = RoundNumber.ToDealer();

        var playerIndex = dealerIndex;
        // ラウンド1〜3: 各プレイヤーに4枚ずつ
        for (var i = 0; i < 3; i++)
        {
            for (var _ = 0; _ < 4; _++)
            {
                wall = wall.Draw(4, out var tiles);
                handArray = handArray.AddTiles(playerIndex, tiles);
                playerIndex = playerIndex.Next();
            }
        }
        // ラウンド4: 各プレイヤーに1枚ずつ
        for (var _ = 0; _ < 4; _++)
        {
            wall = wall.Draw(out var tile);
            handArray = handArray.AddTile(playerIndex, tile);
            playerIndex = playerIndex.Next();
        }

        return this with
        {
            Wall = wall,
            HandArray = handArray,
            Turn = dealerIndex,
        };
    }

    /// <summary>
    /// 現手番プレイヤーが山から1枚ツモります。
    /// 一発フラグはツモ時点では維持し (一発ツモ和了を可能にする)、打牌時 (Dahai) に消します。
    /// </summary>
    internal Round Tsumo()
    {
        var wall = Wall.Draw(out var tile);
        var handArray = HandArray.AddTile(Turn, tile);
        return this with
        {
            Wall = wall,
            HandArray = handArray,
        };
    }

    /// <summary>
    /// 現手番プレイヤーが打牌します。
    /// 第一打フラグ・一発フラグの解除、流し満貫条件 (幺九以外を捨てたら喪失)、嶺上中フラグの解除を行います。
    /// 一発フラグはツモ時点では維持し、打牌 (= ツモ和了しなかった) で消します。
    /// 打牌後に打牌者のフリテン状態を再評価します。
    /// </summary>
    internal Round Dahai(Tile tile, ITenpaiChecker tenpaiChecker)
    {
        var handArray = HandArray.RemoveTile(Turn, tile);
        var riverArray = RiverArray.AddTile(Turn, tile);
        var currentStatus = PlayerRoundStatusArray[Turn];
        var status = currentStatus with
        {
            IsFirstTurnBeforeDiscard = false,
            IsIppatsu = false,
            IsRinshan = false,
            IsNagashiMangan = currentStatus.IsNagashiMangan && tile.IsYaochuu,
        };
        var statusArray = PlayerRoundStatusArray.SetStatus(Turn, status);
        var round = this with
        {
            HandArray = handArray,
            RiverArray = riverArray,
            PlayerRoundStatusArray = statusArray,
        };
        return round.EvaluateFuriten(Turn, tenpaiChecker);
    }

    /// <summary>
    /// 立直宣言を保留します (打牌時に呼ぶ)。持ち点・供託・立直フラグはまだ変更しません。
    /// 持ち点が1000未満、または既に立直済みの場合は例外を投げます。
    /// 全員が和了応答でなかった場合 (= ロン応答が無かった) に <see cref="ConfirmRiichi"/> で確定、
    /// 和了応答 (ロン) が来た場合は <see cref="CancelRiichi"/> で破棄します。
    /// 鳴き応答が来た場合は鳴かれても立直成立なので <see cref="ConfirmRiichi"/> を呼びます。
    /// </summary>
    internal Round PendRiichi(PlayerIndex playerIndex)
    {
        if (PendingRiichiPlayerIndex is { } pending)
        {
            throw new InvalidOperationException($"既に立直保留中です。pending:{pending.Value}");
        }

        var currentStatus = PlayerRoundStatusArray[playerIndex];
        if (currentStatus.IsRiichi)
        {
            throw new InvalidOperationException($"既に立直宣言済みです。player:{playerIndex.Value}");
        }

        if (PointArray[playerIndex].Value < 1000)
        {
            throw new InvalidOperationException($"立直に必要な持ち点が足りません。player:{playerIndex.Value} point:{PointArray[playerIndex].Value}");
        }

        // ダブリー判定は打牌前のスナップショットで保存 (打牌後は IsFirstTurnBeforeDiscard が落ちる)
        var status = currentStatus with
        {
            IsPendingRiichi = true,
            IsPendingRiichiDouble = currentStatus.IsFirstTurnBeforeDiscard,
        };
        var statusArray = PlayerRoundStatusArray.SetStatus(playerIndex, status);
        return this with { PlayerRoundStatusArray = statusArray };
    }

    /// <summary>
    /// 保留中の立直宣言を確定します。当該プレイヤーの IsRiichi/IsIppatsu を立て、
    /// 第一打前なら IsDoubleRiichi も立て、持ち点 -1000・供託リーチ棒 +1 を行います。
    /// 保留中の立直が無い場合は何もせずそのまま返します。
    /// </summary>
    internal Round ConfirmRiichi()
    {
        if (PendingRiichiPlayerIndex is not { } player)
        {
            return this;
        }

        var currentStatus = PlayerRoundStatusArray[player];
        var status = currentStatus with
        {
            IsRiichi = true,
            IsDoubleRiichi = currentStatus.IsPendingRiichiDouble,
            IsIppatsu = true,
            IsPendingRiichi = false,
            IsPendingRiichiDouble = false,
        };
        var statusArray = PlayerRoundStatusArray.SetStatus(player, status);
        return this with
        {
            PlayerRoundStatusArray = statusArray,
            PointArray = PointArray.SubtractPoint(player, 1000),
            KyoutakuRiichiCount = KyoutakuRiichiCount.Add(1),
        };
    }

    /// <summary>
    /// 保留中の立直宣言を破棄します (直後の打牌でロンされた場合に呼ぶ)。
    /// 立直保留をクリアします。
    /// </summary>
    internal Round CancelRiichi()
    {
        if (PendingRiichiPlayerIndex is not { } player)
        {
            return this;
        }

        var currentStatus = PlayerRoundStatusArray[player];
        var status = currentStatus with
        {
            IsPendingRiichi = false,
            IsPendingRiichiDouble = false,
        };
        var statusArray = PlayerRoundStatusArray.SetStatus(player, status);
        return this with { PlayerRoundStatusArray = statusArray };
    }

    /// <summary>
    /// 指定プレイヤーのフリテン状態を再評価します。
    /// 「自分の待ち牌が自分の河 または 自分から鳴かれた牌にある」場合に IsFuriten=true。
    /// 打牌後に呼ぶことで、河が変わった打牌者のフリテンのみ更新します。
    /// (Phase 5 のロン見逃しによる同巡フリテンは <see cref="PlayerRoundStatus.IsTemporaryFuriten"/> で別途管理)
    /// </summary>
    internal Round EvaluateFuriten(PlayerIndex playerIndex, ITenpaiChecker tenpaiChecker)
    {
        var currentStatus = PlayerRoundStatusArray[playerIndex];
        var waitKinds = tenpaiChecker.EnumerateWaitTileKinds(HandArray[playerIndex], CallListArray[playerIndex]);
        var isFuriten = waitKinds.Count > 0 &&
            (RiverArray[playerIndex].Any(x => waitKinds.Contains(x.Kind)) ||
                currentStatus.TilesCalledFromRiver.Any(x => waitKinds.Contains(x.Kind)));
        if (currentStatus.IsFuriten == isFuriten)
        {
            return this;
        }
        var status = currentStatus with { IsFuriten = isFuriten };
        var statusArray = PlayerRoundStatusArray.SetStatus(playerIndex, status);
        return this with { PlayerRoundStatusArray = statusArray };
    }

    /// <summary>
    /// 次のプレイヤーに手番を進めます。
    /// </summary>
    internal Round NextTurn()
    {
        return this with { Turn = Turn.Next() };
    }

    /// <summary>
    /// チーを実行します。直前の打牌 + callerの手牌2枚で順子を作ります。
    /// </summary>
    internal Round Chi(PlayerIndex callerIndex, ImmutableList<Tile> handTiles)
    {
        return ExecuteOpenCall(callerIndex, CallType.Chi, handTiles);
    }

    /// <summary>
    /// ポンを実行します。直前の打牌 + callerの手牌2枚で刻子を作ります。
    /// </summary>
    internal Round Pon(PlayerIndex callerIndex, ImmutableList<Tile> handTiles)
    {
        return ExecuteOpenCall(callerIndex, CallType.Pon, handTiles);
    }

    /// <summary>
    /// 大明槓を実行します。直前の打牌 + callerの手牌3枚で槓子を作ります。
    /// </summary>
    internal Round Daiminkan(PlayerIndex callerIndex, ImmutableList<Tile> handTiles)
    {
        if (!Wall.CanKan)
        {
            throw new InvalidOperationException("槓できません。嶺上牌の残数もしくはツモ山の残数がありません。");
        }
        return ExecuteOpenCall(callerIndex, CallType.Daiminkan, handTiles);
    }

    /// <summary>
    /// 他家からの副露(チー・ポン・大明槓)を処理します。
    /// 副露者は門前喪失、鳴かれた出元は流し満貫条件喪失 + 鳴かれた牌を記録、全員の第一打前/一発フラグを解除します
    /// </summary>
    private Round ExecuteOpenCall(PlayerIndex callerIndex, CallType type, ImmutableList<Tile> handTiles)
    {
        var fromIndex = Turn;
        var riverArray = RiverArray.RemoveLastTile(fromIndex, out var calledTile);
        if (calledTile is null)
        {
            throw new InvalidOperationException("副露対象の打牌がありません。河が空です。");
        }

        var handCounts = HandArray[callerIndex].GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
        foreach (var group in handTiles.GroupBy(x => x))
        {
            if (!handCounts.TryGetValue(group.Key, out var available) || available < group.Count())
            {
                throw new ArgumentException(
                    $"手牌に該当牌が不足しています。tile:{group.Key} 要求:{group.Count()}枚 保有:{available}枚",
                    nameof(handTiles)
                );
            }
        }

        var handArray = HandArray;
        foreach (var tile in handTiles)
        {
            handArray = handArray.RemoveTile(callerIndex, tile);
        }

        var call = new Call(type, handTiles.Add(calledTile), fromIndex, calledTile);
        var callListArray = CallListArray.AddCall(callerIndex, call);

        var statusArray = ClearFirstTurnAndIppatsuForAll(PlayerRoundStatusArray);
        // 副露を行ったプレイヤーは以後門前ではなくなる
        var status = statusArray[callerIndex] with { IsMenzen = false };
        statusArray = statusArray.SetStatus(callerIndex, status);
        // 鳴かれたプレイヤーは流し満貫条件喪失 + 鳴かれた牌を記録 (フリテン判定に使用)
        var fromStatus = statusArray[fromIndex];
        fromStatus = fromStatus with
        {
            IsNagashiMangan = false,
            TilesCalledFromRiver = fromStatus.TilesCalledFromRiver.Add(calledTile),
        };
        statusArray = statusArray.SetStatus(fromIndex, fromStatus);

        return this with
        {
            RiverArray = riverArray,
            HandArray = handArray,
            CallListArray = callListArray,
            Turn = callerIndex,
            PendingDoraReveal = call.Type == CallType.Daiminkan,
            PlayerRoundStatusArray = statusArray,
        };
    }

    /// <summary>
    /// 鳴き発生時に全プレイヤーの第一打前フラグ・一発フラグを解除します。
    /// 天鳳ルール: 暗槓・加槓も含め「鳴き」相当として一発/ダブリー/天和地和人和の権利を消失させる。
    /// </summary>
    private static PlayerRoundStatusArray ClearFirstTurnAndIppatsuForAll(PlayerRoundStatusArray array)
    {
        var statusArray = array;
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            var currentStatus = statusArray[playerIndex];
            if (currentStatus.IsFirstTurnBeforeDiscard || currentStatus.IsIppatsu)
            {
                var status = currentStatus with
                {
                    IsFirstTurnBeforeDiscard = false,
                    IsIppatsu = false,
                };
                statusArray = statusArray.SetStatus(playerIndex, status);
            }
        }
        return statusArray;
    }

    /// <summary>
    /// 暗槓を実行します。指定の牌と同種4枚を現手番プレイヤーの手牌から除去し副露に追加します。
    /// 天鳳ルール: 暗槓も一発消滅/ダブリー無効の「鳴き相当」として扱い、門前は維持します。
    /// </summary>
    /// <param name="tile">暗槓する牌種の牌 (同種4枚が手牌に揃っている必要があります)</param>
    internal Round Ankan(Tile tile)
    {
        if (!Wall.CanKan)
        {
            throw new InvalidOperationException("槓できません。嶺上牌の残数もしくはツモ山の残数がありません。");
        }

        var kind = tile.Kind;
        var tiles = HandArray[Turn].Where(x => x.Kind == kind).Take(4).ToImmutableList();
        if (tiles.Count != 4)
        {
            throw new InvalidOperationException($"指定牌種の4枚が手牌に揃っていません。kind:{kind} count:{tiles.Count}");
        }

        var handArray = HandArray;
        foreach (var t in tiles)
        {
            handArray = handArray.RemoveTile(Turn, t);
        }
        var call = new Call(CallType.Ankan, tiles, Turn, null);
        var callListArray = CallListArray.AddCall(Turn, call);
        var statusArray = ClearFirstTurnAndIppatsuForAll(PlayerRoundStatusArray);
        return this with
        {
            Wall = Wall.RevealDora(), // 暗槓はカンドラ即乗り
            HandArray = handArray,
            CallListArray = callListArray,
            PlayerRoundStatusArray = statusArray,
        };
    }

    /// <summary>
    /// 加槓を実行します。addedTileを現手番プレイヤーの手牌から除き、既存のポンを加槓に差し替えます。
    /// 天鳳ルール: 加槓も一発消滅/ダブリー無効の「鳴き相当」として扱います。
    /// </summary>
    /// <param name="addedTile">加槓で追加する手牌の牌</param>
    internal Round Kakan(Tile addedTile)
    {
        if (!Wall.CanKan)
        {
            throw new InvalidOperationException("槓できません。嶺上牌の残数もしくはツモ山の残数がありません。");
        }

        if (!HandArray[Turn].Contains(addedTile))
        {
            throw new InvalidOperationException($"指定牌が手牌にありません。tile:{addedTile}");
        }

        var kind = addedTile.Kind;
        var existingPon = CallListArray[Turn].FirstOrDefault(x => x.Type == CallType.Pon && x.Tiles[0].Kind == kind)
            ?? throw new InvalidOperationException($"加槓対象のポンがありません。kind:{kind}");

        var handArray = HandArray.RemoveTile(Turn, addedTile);
        var kakan = new Call(CallType.Kakan, existingPon.Tiles.Add(addedTile), existingPon.From, existingPon.CalledTile);
        var callListArray = CallListArray.ReplaceCall(Turn, existingPon, kakan);
        var statusArray = ClearFirstTurnAndIppatsuForAll(PlayerRoundStatusArray);
        return this with
        {
            HandArray = handArray,
            CallListArray = callListArray,
            PendingDoraReveal = true,
            PlayerRoundStatusArray = statusArray,
        };
    }

    /// <summary>
    /// 嶺上牌からツモします。保留中のカンドラがある場合は先にめくります。
    /// 当該プレイヤーの IsRinshan を true にします (次の打牌時に false に戻ります)
    /// </summary>
    internal Round RinshanTsumo()
    {
        var round = PendingDoraReveal
            ? this with { Wall = Wall.RevealDora(), PendingDoraReveal = false }
            : this;
        var wall = round.Wall.DrawRinshan(out var tile);
        var handArray = round.HandArray.AddTile(round.Turn, tile);
        var status = round.PlayerRoundStatusArray[round.Turn] with { IsRinshan = true };
        var statusArray = round.PlayerRoundStatusArray.SetStatus(round.Turn, status);
        return round with
        {
            Wall = wall,
            HandArray = handArray,
            PlayerRoundStatusArray = statusArray,
        };
    }

    private const int HONBA_BONUS_TSUMO_EACH = 100;
    private const int HONBA_BONUS_RON = 300;

    /// <summary>
    /// 和了時の点数精算 (スコア計算 + 本場 + 供託) を行います。
    /// </summary>
    /// <param name="winners">和了者 (ダブロンなら複数、上家取りのため放銃者から見た反時計回り順)</param>
    /// <param name="loserIndex">放銃者のインデックス ロン/槍槓では打牌者/加槓宣言者、ツモ/嶺上では和了者自身</param>
    /// <param name="winType">和了種別</param>
    /// <param name="scoreCalculator">点数計算機</param>
    internal Round SettleWin(ImmutableArray<PlayerIndex> winners, PlayerIndex loserIndex, WinType winType, IScoreCalculator scoreCalculator)
    {
        if (winners.IsDefaultOrEmpty)
        {
            throw new InvalidOperationException("和了者が指定されていません。");
        }
        if (winners.Length != winners.Distinct().Count())
        {
            throw new InvalidOperationException("和了者に重複があります。");
        }

        var pointArray = PointArray;

        foreach (var winner in winners)
        {
            var request = new ScoreRequest(this, winner, loserIndex, winType);
            var result = scoreCalculator.Calculate(request);
            for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
            {
                var playerIndex = new PlayerIndex(i);
                pointArray = pointArray.AddPoint(playerIndex, result.PointDeltas[playerIndex].Value);
            }
        }

        var honba = Honba.Value;
        if (honba > 0)
        {
            var isRon = winType is WinType.Ron or WinType.Chankan;
            if (isRon)
            {
                // ダブロン/トリロン時は各和了者がそれぞれ本場ボーナスを放銃者から受け取る
                foreach (var winner in winners)
                {
                    var bonus = HONBA_BONUS_RON * honba;
                    pointArray = pointArray.AddPoint(winner, bonus).SubtractPoint(loserIndex, bonus);
                }
            }
            else
            {
                var primaryWinner = winners[0];
                var each = HONBA_BONUS_TSUMO_EACH * honba;
                for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
                {
                    var playerIndex = new PlayerIndex(i);
                    if (playerIndex != primaryWinner)
                    {
                        pointArray = pointArray.AddPoint(primaryWinner, each).SubtractPoint(playerIndex, each);
                    }
                }
            }
        }

        var kyoutaku = KyoutakuRiichiCount.Value;
        if (kyoutaku > 0)
        {
            pointArray = pointArray.AddPoint(winners[0], kyoutaku * 1000);
        }

        return this with { PointArray = pointArray, KyoutakuRiichiCount = KyoutakuRiichiCount.Clear() };
    }

    /// <summary>
    /// 流局時の点数精算を行います。
    /// 荒牌平局: 流し満貫者がいれば満貫清算 (テンパイ料は代替)、いなければテンパイ料精算。
    /// 途中流局: 点数移動なし。
    /// </summary>
    internal Round SettleRyuukyoku(
        RyuukyokuType type,
        ImmutableArray<PlayerIndex> tenpaiPlayers,
        ImmutableArray<PlayerIndex> nagashiManganPlayers
    )
    {
        if (!tenpaiPlayers.IsDefaultOrEmpty && tenpaiPlayers.Length != tenpaiPlayers.Distinct().Count())
        {
            throw new InvalidOperationException("テンパイ者に重複があります。");
        }
        if (!nagashiManganPlayers.IsDefaultOrEmpty && nagashiManganPlayers.Length != nagashiManganPlayers.Distinct().Count())
        {
            throw new InvalidOperationException("流し満貫者に重複があります。");
        }

        if (type != RyuukyokuType.KouhaiHeikyoku)
        {
            return this;
        }

        var pointArray = PointArray;
        var dealerIndex = RoundNumber.ToDealer();

        if (nagashiManganPlayers.IsDefaultOrEmpty)
        {
            var tenpaiCount = tenpaiPlayers.IsDefaultOrEmpty ? 0 : tenpaiPlayers.Length;
            if (tenpaiCount is not 0 and not PlayerIndex.PLAYER_COUNT)
            {
                var (gain, pay) = tenpaiCount switch
                {
                    1 => (3000, 1000),
                    2 => (1500, 1500),
                    3 => (1000, 3000),
                    _ => throw new InvalidOperationException($"不正なテンパイ人数: {tenpaiCount}"),
                };
                for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
                {
                    var playerIndex = new PlayerIndex(i);
                    if (tenpaiPlayers.Contains(playerIndex))
                    {
                        pointArray = pointArray.AddPoint(playerIndex, gain);
                    }
                    else
                    {
                        pointArray = pointArray.SubtractPoint(playerIndex, pay);
                    }
                }
            }
        }
        else
        {
            foreach (var winner in nagashiManganPlayers)
            {
                pointArray = pointArray.ApplyNagashiMangan(winner, dealerIndex);
            }
        }
        return this with { PointArray = pointArray };
    }
}
