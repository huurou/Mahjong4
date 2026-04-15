using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Hands;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rivers;
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
/// <param name="WallGenerator">山牌生成機</param>
public record Round(
    RoundWind RoundWind,
    RoundNumber RoundNumber,
    Honba Honba,
    KyoutakuRiichiCount KyoutakuRiichiCount,
    PlayerIndex Turn,
    PointArray PointArray,
    IWallGenerator WallGenerator
)
{
    /// <summary>
    /// 山牌
    /// </summary>
    public Wall Wall { get; init; } = WallGenerator.Generate();

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
    /// 配牌を行います。親から反時計回りに4枚ずつ3周+1枚ずつ1周で各プレイヤー13枚。
    /// </summary>
    public Round Haipai()
    {
        var wall = Wall;
        var handArray = HandArray;
        var dealer = RoundNumber.ToDealer();

        var player = dealer;
        // ラウンド1〜3: 各プレイヤーに4枚ずつ
        for (var i = 0; i < 3; i++)
        {
            for (var p = 0; p < 4; p++)
            {
                wall = wall.Draw(4, out var tiles);
                handArray = handArray.AddTiles(player, tiles);
                player = player.Next();
            }
        }
        // ラウンド4: 各プレイヤーに1枚ずつ
        for (var p = 0; p < 4; p++)
        {
            wall = wall.Draw(out var tile);
            handArray = handArray.AddTile(player, tile);
            player = player.Next();
        }

        return this with
        {
            Wall = wall,
            HandArray = handArray,
            Turn = dealer,
        };
    }

    /// <summary>
    /// 現手番プレイヤーが山から1枚ツモります。
    /// </summary>
    public Round Tsumo()
    {
        var wall = Wall.Draw(out var tile);
        var handArray = HandArray.AddTile(Turn, tile);
        return this with { Wall = wall, HandArray = handArray };
    }

    /// <summary>
    /// 現手番プレイヤーが打牌します。
    /// </summary>
    public Round Dahai(Tile tile)
    {
        var handArray = HandArray.RemoveTile(Turn, tile);
        var riverArray = RiverArray.AddTile(Turn, tile);
        return this with { HandArray = handArray, RiverArray = riverArray };
    }

    /// <summary>
    /// 次のプレイヤーに手番を進めます。
    /// </summary>
    public Round NextTurn()
    {
        return this with { Turn = Turn.Next() };
    }

    /// <summary>
    /// チーを実行します。直前の打牌 + callerの手牌2枚で順子を作ります。
    /// </summary>
    public Round Chi(PlayerIndex callerIndex, ImmutableList<Tile> handTiles)
    {
        return ExecuteOpenCall(callerIndex, CallType.Chi, handTiles);
    }

    /// <summary>
    /// ポンを実行します。直前の打牌 + callerの手牌2枚で刻子を作ります。
    /// </summary>
    public Round Pon(PlayerIndex callerIndex, ImmutableList<Tile> handTiles)
    {
        return ExecuteOpenCall(callerIndex, CallType.Pon, handTiles);
    }

    /// <summary>
    /// 大明槓を実行します。直前の打牌 + callerの手牌3枚で槓子を作ります。
    /// </summary>
    public Round Daiminkan(PlayerIndex callerIndex, ImmutableList<Tile> handTiles)
    {
        if (!Wall.CanKan)
        {
            throw new InvalidOperationException("槓できません。嶺上牌の残数もしくはツモ山の残数がありません。");
        }
        return ExecuteOpenCall(callerIndex, CallType.Daiminkan, handTiles);
    }

    private Round ExecuteOpenCall(PlayerIndex callerIndex, CallType type, ImmutableList<Tile> handTiles)
    {
        var from = Turn;
        var river = RiverArray[from];
        if (!river.Any())
        {
            throw new InvalidOperationException("副露対象の打牌がありません。河が空です。");
        }
        var calledTile = river.Last();
        var callerHand = HandArray[callerIndex];
        foreach (var tile in handTiles)
        {
            if (!callerHand.Contains(tile))
            {
                throw new ArgumentException($"指定牌が手牌にありません。tile:{tile}", nameof(handTiles));
            }
        }
        var tiles = handTiles.Add(calledTile);
        var call = new Call(type, tiles, from, calledTile);
        return ExecuteOpenCall(callerIndex, call);
    }

    /// <summary>
    /// 他家からの副露(チー・ポン・大明槓)を処理します。
    /// </summary>
    private Round ExecuteOpenCall(PlayerIndex callerIndex, Call call)
    {
        var riverArray = RiverArray.RemoveLastTile(call.From, out _);
        var handArray = HandArray;
        foreach (var tile in call.Tiles)
        {
            if (tile != call.CalledTile)
            {
                handArray = handArray.RemoveTile(callerIndex, tile);
            }
        }
        var callListArray = CallListArray.AddCall(callerIndex, call);
        return this with
        {
            RiverArray = riverArray,
            HandArray = handArray,
            CallListArray = callListArray,
            Turn = callerIndex,
            PendingDoraReveal = call.Type == CallType.Daiminkan,
        };
    }

    /// <summary>
    /// 暗槓を実行します。指定の牌と同種4枚を現手番プレイヤーの手牌から除去し副露に追加します。
    /// </summary>
    /// <param name="tile">暗槓する牌種の牌 (同種4枚が手牌に揃っている必要があります)</param>
    public Round Ankan(Tile tile)
    {
        if (!Wall.CanKan)
        {
            throw new InvalidOperationException("槓できません。嶺上牌の残数もしくはツモ山の残数がありません。");
        }
        var kind = tile.Id / 4;
        var tiles = HandArray[Turn].Where(x => x.Id / 4 == kind).Take(4).ToImmutableList();
        if (tiles.Count != 4)
        {
            throw new InvalidOperationException($"指定牌種の4枚が手牌に揃っていません。kind:{kind} count:{tiles.Count}");
        }
        var handArray = HandArray;
        foreach (var t in tiles)
        {
            handArray = handArray.RemoveTile(Turn, t);
        }
        var call = new Call(CallType.Ankan, tiles, Turn, tile);
        var callListArray = CallListArray.AddCall(Turn, call);
        return (this with { HandArray = handArray, CallListArray = callListArray }).RevealDora();
    }

    /// <summary>
    /// 加槓を実行します。addedTileを現手番プレイヤーの手牌から除き、既存のポンを加槓に差し替えます。
    /// </summary>
    /// <param name="addedTile">加槓で追加する手牌の牌</param>
    public Round Kakan(Tile addedTile)
    {
        if (!Wall.CanKan)
        {
            throw new InvalidOperationException("槓できません。嶺上牌の残数もしくはツモ山の残数がありません。");
        }
        if (!HandArray[Turn].Contains(addedTile))
        {
            throw new InvalidOperationException($"指定牌が手牌にありません。tile:{addedTile}");
        }
        var kind = addedTile.Id / 4;
        var existingPon = CallListArray[Turn].FirstOrDefault(x =>
            x.Type == CallType.Pon && x.Tiles.Any(y => y.Id / 4 == kind)
        )
            ?? throw new InvalidOperationException($"加槓対象のポンがありません。kind:{kind}");
        var handArray = HandArray.RemoveTile(Turn, addedTile);
        var kakan = new Call(
            CallType.Kakan,
            existingPon.Tiles.Add(addedTile),
            existingPon.From,
            existingPon.CalledTile
        );
        var callListArray = CallListArray.ReplaceCall(Turn, existingPon, kakan);
        return this with { HandArray = handArray, CallListArray = callListArray, PendingDoraReveal = true };
    }

    /// <summary>
    /// 嶺上牌からツモします。保留中のカンドラがある場合は先にめくります。
    /// </summary>
    public Round RinshanTsumo()
    {
        var round = PendingDoraReveal
            ? RevealDora() with { PendingDoraReveal = false }
            : this;
        var wall = round.Wall.DrawRinshan(out var tile);
        var handArray = round.HandArray.AddTile(round.Turn, tile);
        return round with { Wall = wall, HandArray = handArray };
    }

    /// <summary>
    /// 新ドラを1枚表示します。
    /// </summary>
    public Round RevealDora()
    {
        return this with { Wall = Wall.RevealDora() };
    }

    /// <summary>
    /// 持ち点を更新します。
    /// </summary>
    public Round SetPoints(PointArray pointArray)
    {
        return this with { PointArray = pointArray };
    }

    /// <summary>
    /// 供託リーチ棒を指定数だけ加算します。
    /// </summary>
    public Round AddKyoutakuRiichi(int count)
    {
        return this with { KyoutakuRiichiCount = new KyoutakuRiichiCount(KyoutakuRiichiCount.Value + count) };
    }

    /// <summary>
    /// 供託を0にします。
    /// </summary>
    public Round ClearKyoutaku()
    {
        return this with { KyoutakuRiichiCount = new KyoutakuRiichiCount(0) };
    }
}
