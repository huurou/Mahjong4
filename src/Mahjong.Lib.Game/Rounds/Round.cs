using Mahjong.Lib.Game.Adoptions;
using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games;
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
    /// 包 (責任払い) の責任者配列 (index = 和了者, 値 = 責任者)
    /// 大三元 / 大四喜 / 四槓子 の確定トリガ副露が発生したタイミングで <see cref="PaoDetector"/> 経由で記録される
    /// </summary>
    public PlayerResponsibilityArray PaoResponsibleArray { get; init; } = new PlayerResponsibilityArray();

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
                if (PlayerRoundStatusArray[playerIndex].IsPendingRiichi)
                {
                    return playerIndex;
                }
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
    /// Turn プレイヤーの同巡フリテン (<see cref="PlayerRoundStatus.IsTemporaryFuriten"/>) は
    /// 自分の次ツモで解除する (天鳳準拠)。
    /// </summary>
    internal Round Tsumo()
    {
        var wall = Wall.Draw(out var tile);
        var handArray = HandArray.AddTile(Turn, tile);
        var currentStatus = PlayerRoundStatusArray[Turn];
        var statusArray = currentStatus.IsTemporaryFuriten
            ? PlayerRoundStatusArray.SetStatus(Turn, currentStatus with { IsTemporaryFuriten = false })
            : PlayerRoundStatusArray;
        return this with
        {
            Wall = wall,
            HandArray = handArray,
            PlayerRoundStatusArray = statusArray,
        };
    }

    /// <summary>
    /// 指定プレイヤーの同巡フリテン (<see cref="PlayerRoundStatus.IsTemporaryFuriten"/>) を設定します。
    /// ロン見逃し検出時に <see cref="States.RoundStates.RoundStateContext"/> から呼び出されます。
    /// 同値の場合は副作用なしで同一インスタンスを返します。
    /// </summary>
    internal Round SetTemporaryFuriten(PlayerIndex playerIndex, bool value)
    {
        var currentStatus = PlayerRoundStatusArray[playerIndex];
        if (currentStatus.IsTemporaryFuriten == value) { return this; }
        var status = currentStatus with { IsTemporaryFuriten = value };
        return this with { PlayerRoundStatusArray = PlayerRoundStatusArray.SetStatus(playerIndex, status) };
    }

    /// <summary>
    /// 指定されたプレイヤー群に同巡フリテン (<see cref="PlayerRoundStatus.IsTemporaryFuriten"/>=true) をまとめて適用します。
    /// 空の配列を渡した場合は副作用なしで同一インスタンスを返します。
    /// 打牌フェーズでロン見逃ししたプレイヤー (RoundStateContext が検出) を一括適用する用途
    /// </summary>
    internal Round ApplyTemporaryFuriten(ImmutableArray<PlayerIndex> playerIndices)
    {
        if (playerIndices.IsDefaultOrEmpty) { return this; }
        var round = this;
        foreach (var playerIndex in playerIndices)
        {
            round = round.SetTemporaryFuriten(playerIndex, true);
        }
        return round;
    }

    /// <summary>
    /// 現手番プレイヤーが打牌します。
    /// 第一打フラグ・一発フラグの解除、流し満貫条件 (幺九以外を捨てたら喪失)、嶺上中フラグの解除を行います。
    /// 一発フラグはツモ時点では維持し、打牌 (= ツモ和了しなかった) で消します。
    /// 打牌後に打牌者のフリテン状態を再評価します。
    /// </summary>
    internal Round Dahai(Tile tile)
    {
        var handArray = HandArray.RemoveTile(Turn, tile);
        var riverArray = RiverArray.AddTile(Turn, tile);
        var currentStatus = PlayerRoundStatusArray[Turn];
        var status = currentStatus with
        {
            IsFirstTurnBeforeDiscard = false,
            IsIppatsu = false,
            IsRinshan = false,
            IsNagashiMangan = currentStatus.IsNagashiMangan && tile.Kind.IsYaochu,
        };
        var statusArray = PlayerRoundStatusArray.SetStatus(Turn, status);
        var round = this with
        {
            HandArray = handArray,
            RiverArray = riverArray,
            PlayerRoundStatusArray = statusArray,
        };
        return round.EvaluateFuriten(Turn);
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
    /// 「自分の待ち牌が自分の河 または 自分から鳴かれた牌にある」場合に IsFuriten=true
    /// 打牌後に呼ぶことで、河が変わった打牌者のフリテンのみ更新します。
    /// (Phase 5 のロン見逃しによる同巡フリテンは <see cref="PlayerRoundStatus.IsTemporaryFuriten"/> で別途管理)
    /// </summary>
    internal Round EvaluateFuriten(PlayerIndex playerIndex)
    {
        var currentStatus = PlayerRoundStatusArray[playerIndex];
        var waitKinds = TenpaiHelper.EnumerateWaitTileKinds(HandArray[playerIndex]);
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
    /// 副露者は門前喪失、鳴かれた出元は流し満貫条件喪失 + 鳴かれた牌を記録、全員の第一打前/一発フラグを解除します。
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

        // 副露履歴更新と同じ with 式で PaoResponsibleArray を確定させる (atomicity 優先)
        var paoResponsibleArray = PaoDetector.Detect(callListArray[callerIndex], call).IsPao()
            ? PaoResponsibleArray.SetResponsible(callerIndex, fromIndex)
            : PaoResponsibleArray;

        return this with
        {
            RiverArray = riverArray,
            HandArray = handArray,
            CallListArray = callListArray,
            Turn = callerIndex,
            PendingDoraReveal = call.Type == CallType.Daiminkan,
            PlayerRoundStatusArray = statusArray,
            PaoResponsibleArray = paoResponsibleArray,
        };
    }

    /// <summary>
    /// 鳴き発生時に全プレイヤーの第一打前フラグ・一発フラグを解除します。
    /// 天鳳ルール: 暗槓・加槓も含め「鳴き」相当として一発/ダブリー/天和地和人和の権利を消失させる
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

        var tiles = ResolveAnkanTiles(tile);
        var handArray = HandArray;
        foreach (var t in tiles)
        {
            handArray = handArray.RemoveTile(Turn, t);
        }
        var call = new Call(CallType.Ankan, [.. tiles], Turn, null);
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
    /// 暗槓で手牌から引き抜かれる4枚を解決します (槓実行前の状態を参照)。
    /// CreateInquirySpec で国士無双暗槓チャンカン候補を判定する際など、実行前に槓子4枚を知る必要がある場面で使用します。
    /// </summary>
    /// <param name="tile">暗槓する牌種の牌 (同種4枚が手牌に揃っている必要があります)</param>
    internal ImmutableArray<Tile> ResolveAnkanTiles(Tile tile)
    {
        var kind = tile.Kind;
        var tiles = HandArray[Turn].Where(x => x.Kind == kind).Take(4).ToImmutableArray();
        if (tiles.Length != 4)
        {
            throw new InvalidOperationException($"指定牌種の4枚が手牌に揃っていません。kind:{kind} count:{tiles.Length}");
        }
        return tiles;
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

        var tiles = ResolveKakanTiles(addedTile);
        var existingPon = CallListArray[Turn].First(x => x.Type == CallType.Pon && x.Tiles[0].Kind == addedTile.Kind);

        var handArray = HandArray.RemoveTile(Turn, addedTile);
        var kakan = new Call(CallType.Kakan, [.. tiles], existingPon.From, existingPon.CalledTile);
        var callListArray = CallListArray.ReplaceCall(Turn, existingPon, kakan);
        var statusArray = ClearFirstTurnAndIppatsuForAll(PlayerRoundStatusArray);

        // 加槓で発火しうる包は四槓子のみ (大三元/大四喜は Pon/Daiminkan で既に確定済み)。
        // 四槓子 (Kakan 4 槓目) の責任者は元ポンの出し手とするのが天鳳準拠。
        var paoResponsibleArray = PaoDetector.Detect(callListArray[Turn], kakan).IsPao()
            ? PaoResponsibleArray.SetResponsible(Turn, existingPon.From)
            : PaoResponsibleArray;

        return this with
        {
            HandArray = handArray,
            CallListArray = callListArray,
            PendingDoraReveal = true,
            PlayerRoundStatusArray = statusArray,
            PaoResponsibleArray = paoResponsibleArray,
        };
    }

    /// <summary>
    /// 加槓後の槓子4枚 (元ポン 3枚 + 追加牌 1枚) を解決します (槓実行前の状態を参照)。
    /// CreateInquirySpec で槍槓候補を判定する際に使用します。
    /// </summary>
    /// <param name="addedTile">加槓で追加する手牌の牌</param>
    internal ImmutableArray<Tile> ResolveKakanTiles(Tile addedTile)
    {
        if (!HandArray[Turn].Contains(addedTile))
        {
            throw new InvalidOperationException($"指定牌が手牌にありません。tile:{addedTile}");
        }
        var kind = addedTile.Kind;
        var existingPon = CallListArray[Turn].FirstOrDefault(x => x.Type == CallType.Pon && x.Tiles[0].Kind == kind)
            ?? throw new InvalidOperationException($"加槓対象のポンがありません。kind:{kind}");
        return [.. existingPon.Tiles, addedTile];
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
    /// 包 (責任払い) 適用後の点数移動を再配分します。役満素点 (ScoreResult.PointDeltas) のみが対象で、本場・供託は含みません (天鳳準拠)。
    /// ツモ/嶺上ツモ: 役満素点の他家負担分をすべて責任者 1 人に集約
    /// ロン/槍槓: 役満素点を放銃者と責任者で折半 (100 点単位の端数は放銃者側に寄せる、天鳳準拠)
    /// </summary>
    private static PointArray AdjustPointDeltasForPao(
        PointArray originalDeltas,
        PlayerIndex winnerIndex,
        PlayerIndex responsibleIndex,
        PlayerIndex loserIndex,
        WinType winType
    )
    {
        var gain = originalDeltas[winnerIndex].Value;
        var adjusted = new PointArray(new Point(0)).AddPoint(winnerIndex, gain);

        if (winType is WinType.Tsumo or WinType.Rinshan)
        {
            adjusted = adjusted.SubtractPoint(responsibleIndex, gain);
        }
        else
        {
            var half = gain / 2;
            var responsiblePay = (half / 100) * 100;
            var loserPay = gain - responsiblePay;
            if (responsibleIndex == loserIndex)
            {
                adjusted = adjusted.SubtractPoint(loserIndex, gain);
            }
            else
            {
                adjusted = adjusted
                    .SubtractPoint(responsibleIndex, responsiblePay)
                    .SubtractPoint(loserIndex, loserPay);
            }
        }

        return adjusted;
    }

    /// <summary>
    /// 和了時の点数精算を行い、通知層へ渡す明細 (和了者毎のスコア / 和了牌 / 本場 / 供託受取) と精算後の Round を返します。
    /// 和了牌は呼び出し側で明示的に決定する (Ron=放銃者の河末尾 / Chankan=<see cref="Impl.RoundStateKan.KanTiles"/>.Last /
    /// Tsumo・Rinshan=和了者の手牌末尾)。
    /// </summary>
    /// <param name="winners">和了者 (ダブロンなら複数、上家取りのため放銃者から見た反時計回り順)</param>
    /// <param name="loserIndex">放銃者のインデックス ロン/槍槓では打牌者/加槓宣言者、ツモ/嶺上では和了者自身</param>
    /// <param name="winType">和了種別</param>
    /// <param name="winTile">和了牌 (Ron=放銃牌 / Chankan=加槓追加牌 / Tsumo・Rinshan=ツモ牌)</param>
    /// <param name="scoreResults">各和了者の点数計算結果 (<paramref name="winners"/> と同順)</param>
    internal (Round Settled, WinSettlementDetails Details) SettleWin(
        ImmutableArray<PlayerIndex> winners,
        PlayerIndex loserIndex,
        WinType winType,
        Tile winTile,
        ImmutableArray<ScoreResult> scoreResults
    )
    {
        if (winners.IsDefaultOrEmpty)
        {
            throw new InvalidOperationException("和了者が指定されていません。");
        }

        if (winners.Length != winners.Distinct().Count())
        {
            throw new InvalidOperationException("和了者に重複があります。");
        }

        if (scoreResults.IsDefaultOrEmpty || scoreResults.Length != winners.Length)
        {
            throw new InvalidOperationException("scoreResults は winners と同じ個数が必要です。");
        }

        var pointArray = PointArray;
        var winnersBuilder = ImmutableArray.CreateBuilder<AdoptedWinner>(winners.Length);

        for (var wi = 0; wi < winners.Length; wi++)
        {
            var winner = winners[wi];
            var rawResult = scoreResults[wi];
            var responsibleIndex = PaoResponsibleArray[winner];
            var isPaoApplicable = responsibleIndex is not null &&
                rawResult.YakuList.HasPaoEligibleYaku();
            var result = isPaoApplicable
                ? rawResult with { PointDeltas = AdjustPointDeltasForPao(rawResult.PointDeltas, winner, responsibleIndex!, loserIndex, winType) }
                : rawResult;
            for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
            {
                var playerIndex = new PlayerIndex(i);
                pointArray = pointArray.AddPoint(playerIndex, result.PointDeltas[playerIndex].Value);
            }
            var paoPlayerIndex = isPaoApplicable ? responsibleIndex : null;
            winnersBuilder.Add(new AdoptedWinner(winner, winTile, result, paoPlayerIndex));
        }

        var honbaValue = Honba.Value;
        if (honbaValue > 0)
        {
            var isRon = winType is WinType.Ron or WinType.Chankan;
            if (isRon)
            {
                // ダブロン/トリロン時は各和了者がそれぞれ本場ボーナスを放銃者から受け取る
                foreach (var winner in winners)
                {
                    var bonus = HONBA_BONUS_RON * honbaValue;
                    pointArray = pointArray.AddPoint(winner, bonus).SubtractPoint(loserIndex, bonus);
                }
            }
            else
            {
                var primaryWinner = winners[0];
                var each = HONBA_BONUS_TSUMO_EACH * honbaValue;
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
        var kyoutakuAward = new KyoutakuRiichiAward(winners[0], kyoutaku);

        // 和了者の誰かが立直成立しているなら裏ドラ表示牌を公開 (天鳳 JSON 牌譜の log[3] に入る)
        var anyRiichi = winners.Any(x =>
            PlayerRoundStatusArray[x].IsRiichi ||
            PlayerRoundStatusArray[x].IsDoubleRiichi);
        var uraDoraIndicators = anyRiichi
            ? CollectUraDoraIndicators()
            : [];

        var details = new WinSettlementDetails(winnersBuilder.ToImmutable(), Honba, kyoutakuAward, uraDoraIndicators);
        var settled = this with { PointArray = pointArray, KyoutakuRiichiCount = KyoutakuRiichiCount.Clear() };
        return (settled, details);
    }

    /// <summary>
    /// 現時点で表示されているドラと同じ枚数分の裏ドラ表示牌を収集する。
    /// 立直者が和了に含まれる場合のみ呼び出すこと (牌譜記録・点数計算で使用)
    /// </summary>
    private ImmutableArray<Tile> CollectUraDoraIndicators()
    {
        var builder = ImmutableArray.CreateBuilder<Tile>(Wall.DoraRevealedCount);
        for (var n = 0; n < Wall.DoraRevealedCount; n++)
        {
            builder.Add(Wall.GetUradoraIndicator(n));
        }
        return builder.ToImmutable();
    }

    /// <summary>
    /// 流局時の点数精算を行います。戻り値の <c>PointDeltas</c> は精算による各プレイヤーの点数差分
    /// (精算後 − 精算前) で、通知・牌譜記録で使用します。
    /// 荒牌平局: 流し満貫者がいれば満貫清算 (テンパイ料は代替)、いなければテンパイ料精算
    /// 途中流局: 点数移動なし (全要素 0 の PointDeltas を返す)
    /// </summary>
    internal (Round Settled, PointArray PointDeltas) SettleRyuukyoku(
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

        var zeroDeltas = new PointArray(new Point(0));
        if (type != RyuukyokuType.KouhaiHeikyoku)
        {
            return (this, zeroDeltas);
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

        var deltas = zeroDeltas;
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var playerIndex = new PlayerIndex(i);
            deltas = deltas.AddPoint(playerIndex, pointArray[playerIndex].Value - PointArray[playerIndex].Value);
        }
        return (this with { PointArray = pointArray }, deltas);
    }

    /// <summary>
    /// 四家立直: 全プレイヤーが立直確定状態にある場合に true。
    /// ConfirmRiichi 後に呼び出すこと (ロン応答経路では CancelRiichi が走るため本条件は立たない)。
    /// </summary>
    internal bool IsSuuchaRiichi()
    {
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            if (!PlayerRoundStatusArray[new PlayerIndex(i)].IsRiichi)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 四風連打: 第1巡 (全員の河が1枚のみ) かつ副露なし、全員が同一の風牌を捨てた場合に true。
    /// 4人目の打牌直後の ResponseOk で呼び出す想定。立直宣言 (ダブリー含む) があっても成立する (天鳳は四風連打優先)。
    /// </summary>
    internal bool IsSuufonrenda()
    {
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            if (CallListArray[new PlayerIndex(i)].Any())
            {
                return false;
            }
        }

        Scoring.Tiles.TileKind? firstKind = null;
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var river = RiverArray[new PlayerIndex(i)];
            if (river.Count() != 1)
            {
                return false;
            }

            var kind = river.First().Kind;
            if (!kind.IsWind)
            {
                return false;
            }

            // TileKind はシングルトンなので == で同値判定可
            if (firstKind is null)
            {
                firstKind = kind;
            }
            else if (firstKind != kind)
            {
                return false;
            }
        }
        return firstKind is not null;
    }

    /// <summary>
    /// 四槓流れ: 総槓数が4以上かつ槓宣言者が2人以上の場合に true。
    /// 同一プレイヤーによる4槓 (四槓子) は不成立 (和了待ち権利を保護)。
    /// 嶺上ツモ後 (槓子が副露に反映済) の ResponseOk 時点で呼び出す。
    /// </summary>
    internal bool IsSuukaikan()
    {
        var total = 0;
        var declarers = 0;
        for (var i = 0; i < PlayerIndex.PLAYER_COUNT; i++)
        {
            var kanCount = CallListArray[new PlayerIndex(i)]
                .Count(x => x.Type is CallType.Ankan or CallType.Daiminkan or CallType.Kakan);
            if (kanCount > 0) { declarers++; }
            total += kanCount;
        }
        return total >= 4 && declarers >= 2;
    }
}
