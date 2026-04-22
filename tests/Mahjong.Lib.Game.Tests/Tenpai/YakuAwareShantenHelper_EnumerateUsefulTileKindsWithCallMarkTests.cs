using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Games;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Tenpai;
using Mahjong.Lib.Game.Tiles;
using Mahjong.Lib.Scoring.Tiles;
using System.Collections.Immutable;
using Hand = Mahjong.Lib.Game.Hands.Hand;

namespace Mahjong.Lib.Game.Tests.Tenpai;

public class YakuAwareShantenHelper_EnumerateUsefulTileKindsWithCallMarkTests
{
    [Fact]
    public void 役ありシャンテン不能_空配列を返す()
    {
        // Arrange: 7 経路すべて INFINITE になる手牌を設計:
        //   門前: Chi 副露あり → INFINITE
        //   翻牌: 手牌に役牌の刻子も対子も Call もなし → INFINITE
        //   断么九: Chi に m1 (幺九) 含む → INFINITE
        //   対々和: Chi 副露あり → INFINITE
        //   一色手(m/p/s): 副露に複数スート含む → すべて INFINITE
        var hand = BuildHand(TileKind.Sha, TileKind.Pei, TileKind.Hatsu, TileKind.Chun);
        var chi1 = MakeChiCall(TileKind.Man1, TileKind.Man2, TileKind.Man3, calledIndex: 0);
        var chi2 = MakeChiCall(TileKind.Pin4, TileKind.Pin5, TileKind.Pin6, calledIndex: 0);
        var chi3 = MakeChiCall(TileKind.Sou6, TileKind.Sou7, TileKind.Sou8, calledIndex: 0);
        CallList calls = [chi1, chi2, chi3];

        // Act
        var actual = YakuAwareShantenHelper.EnumerateUsefulTileKindsWithCallMark(
            hand, calls, new GameRules(), roundWindIndex: 0, seatWindIndex: 0);

        // Assert: 全経路 INFINITE なので有効牌なし
        Assert.Empty(actual);
    }

    [Fact]
    public void ポン可能な役牌対子_Markがポンになる()
    {
        // Arrange: 白対子 + 他の中張形、白を引ければ翻牌シャンテンが進む
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou1, TileKind.Sou2, TileKind.Sou3,
            TileKind.Sou5,
            TileKind.Haku, TileKind.Haku
        );

        // Act
        var actual = YakuAwareShantenHelper.EnumerateUsefulTileKindsWithCallMark(
            hand, [], new GameRules(), roundWindIndex: 0, seatWindIndex: 0);

        // Assert: 白が有効牌に含まれ、マークは Pon
        var hakuEntry = actual.FirstOrDefault(x => x.Kind == TileKind.Haku);
        Assert.NotEqual(default, hakuEntry);
        Assert.Equal(CallMark.Pon, hakuEntry.Mark);
    }

    [Fact]
    public void チー可能な数牌両面_Markがチーになる()
    {
        // Arrange: 3 面子 (Pin/Sou/Haku刻) + Man5-Man6 搭子 + 孤立 Ton/Nan = 2 シャンテン手。
        // Man4 ツモで Man4-5-6 順子完成 → 1 シャンテンに進む。Man4 は手牌に 0 枚でポン不可、
        // Man5+Man6 を使った Chi 仮想で役ありシャンテンが更に進むため Chi マークがつく。
        var hand = BuildHand(
            TileKind.Pin2, TileKind.Pin3, TileKind.Pin4,
            TileKind.Sou3, TileKind.Sou4, TileKind.Sou5,
            TileKind.Haku, TileKind.Haku, TileKind.Haku,
            TileKind.Man5, TileKind.Man6,
            TileKind.Ton, TileKind.Nan
        );

        // Act
        var actual = YakuAwareShantenHelper.EnumerateUsefulTileKindsWithCallMark(
            hand, [], new GameRules(), roundWindIndex: 0, seatWindIndex: 0);

        // Assert: Man4 は有効牌かつ Chi マーク
        var man4 = actual.FirstOrDefault(x => x.Kind == TileKind.Man4);
        Assert.NotEqual(default, man4);
        Assert.Equal(CallMark.Chi, man4.Mark);
    }

    [Fact]
    public void ポン可能な対子で有効牌_Markがポンになる()
    {
        // Arrange: Haku 対子 + Pin234/Sou345 面子 + Man/Ton/Nan 孤立で約 2 シャンテン。
        // Haku をツモると Haku 刻子でシャンテンが進む。ポン仮想 (Haku 対子 2 枚除去) でも進むため Pon マーク。
        var hand = BuildHand(
            TileKind.Pin2, TileKind.Pin3, TileKind.Pin4,
            TileKind.Sou3, TileKind.Sou4, TileKind.Sou5,
            TileKind.Haku, TileKind.Haku,
            TileKind.Man5, TileKind.Man6,
            TileKind.Ton, TileKind.Nan, TileKind.Pei
        );

        // Act
        var actual = YakuAwareShantenHelper.EnumerateUsefulTileKindsWithCallMark(
            hand, [], new GameRules(), roundWindIndex: 0, seatWindIndex: 0);

        // Assert: Haku は有効牌で、Pon マーク (ポン可能な役牌対子)
        var haku = actual.FirstOrDefault(x => x.Kind == TileKind.Haku);
        Assert.NotEqual(default, haku);
        Assert.Equal(CallMark.Pon, haku.Mark);
    }

    [Fact]
    public void 既存暗槓ありで仮想Call衝突想定_例外を投げない()
    {
        // Arrange: 手牌 10 枚 + 暗槓 1 組で既存副露と Tile.Id が衝突しうる仮想 Call 生成を誘発。
        // 実装の BuildVirtualKoutsuCall / BuildVirtualShuntsuCall は Tile.Id が他と重複しても問題なく動作する
        // (ShantenCalculator は Kind のみ参照) ことを例外非発生で確認する。
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou7, TileKind.Sou8,
            TileKind.Haku, TileKind.Ton
        );
        var ankan = MakeKoutsuCall(CallType.Ankan, TileKind.Hatsu);
        CallList calls = [ankan];

        // Act
        var exception = Record.Exception(() =>
            YakuAwareShantenHelper.EnumerateUsefulTileKindsWithCallMark(
                hand, calls, new GameRules(), roundWindIndex: 0, seatWindIndex: 0));

        // Assert: 仮想 Call は Call.Validate を通る最小構成なので例外なし
        Assert.Null(exception);
    }

    [Fact]
    public void テンパイ状態_副露マークは付かない()
    {
        // Arrange: 既にテンパイ (m1m2m3 p4p5p6 s1s2s3 Ha Ha 東東 → 東 or 白待ち)
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou1, TileKind.Sou2, TileKind.Sou3,
            TileKind.Haku, TileKind.Haku,
            TileKind.Ton, TileKind.Ton
        );

        // Act
        var actual = YakuAwareShantenHelper.EnumerateUsefulTileKindsWithCallMark(
            hand, [], new GameRules(), roundWindIndex: 0, seatWindIndex: 0);

        // Assert: テンパイ (シャンテン 0) では副露マークは一切付かない (書籍: テンパイなら鳴かない)
        Assert.All(actual, entry => Assert.Equal(CallMark.None, entry.Mark));
    }

    [Fact]
    public void 有効牌なし_空配列()
    {
        // Arrange: 既に和了形 14 枚
        var hand = BuildHand(
            TileKind.Man1, TileKind.Man2, TileKind.Man3,
            TileKind.Pin4, TileKind.Pin5, TileKind.Pin6,
            TileKind.Sou7, TileKind.Sou8, TileKind.Sou9,
            TileKind.Ton, TileKind.Ton,
            TileKind.Haku, TileKind.Haku, TileKind.Haku
        );

        // Act
        var actual = YakuAwareShantenHelper.EnumerateUsefulTileKindsWithCallMark(
            hand, [], new GameRules(), roundWindIndex: 0, seatWindIndex: 0);

        // Assert: 和了 (-1 シャンテン) なので有効牌なし
        Assert.Empty(actual);
    }

    // ================= ヘルパー =================

    private static Hand BuildHand(params TileKind[] kinds)
    {
        var countsByKind = new Dictionary<TileKind, int>();
        var tiles = new List<Tile>();
        foreach (var kind in kinds)
        {
            if (!countsByKind.TryGetValue(kind, out var copy)) { copy = 0; }
            countsByKind[kind] = copy + 1;
            tiles.Add(new Tile(kind.Value * 4 + copy));
        }
        return new Hand(tiles);
    }

    private static Call MakeKoutsuCall(CallType type, TileKind kind)
    {
        var baseId = kind.Value * 4;
        var count = type is CallType.Ankan or CallType.Daiminkan or CallType.Kakan ? 4 : 3;
        var tiles = Enumerable.Range(0, count).Select(x => new Tile(baseId + x)).ToImmutableList();
        var calledTile = type == CallType.Ankan ? null : tiles[0];
        return new Call(type, tiles, new PlayerIndex(3), calledTile);
    }

    /// <summary>
    /// 連続する 3 牌種のチー副露を生成。<paramref name="calledIndex"/> は鳴かれた牌の位置 (0/1/2)
    /// </summary>
    private static Call MakeChiCall(TileKind k1, TileKind k2, TileKind k3, int calledIndex)
    {
        var tiles = new[] { new Tile(k1.Value * 4), new Tile(k2.Value * 4), new Tile(k3.Value * 4) }.ToImmutableList();
        return new Call(CallType.Chi, tiles, new PlayerIndex(3), tiles[calledIndex]);
    }
}
