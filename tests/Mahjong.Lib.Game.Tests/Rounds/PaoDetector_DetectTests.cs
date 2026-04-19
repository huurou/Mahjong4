using Mahjong.Lib.Game.Calls;
using Mahjong.Lib.Game.Players;
using Mahjong.Lib.Game.Rounds;
using Mahjong.Lib.Game.Tiles;

namespace Mahjong.Lib.Game.Tests.Rounds;

public class PaoDetector_DetectTests
{
    // 牌種 (Kind) 早見 Tile.Id = Kind * 4 (同種1枚目)
    // 東=27, 南=28, 西=29, 北=30, 白=31, 發=32, 中=33
    private static Tile KindTile(int kind, int subIndex = 0)
    {
        return new(kind * 4 + subIndex);
    }

    private static Call PonOf(int kind, PlayerIndex from)
    {
        var called = KindTile(kind, 0);
        return new Call(CallType.Pon, [KindTile(kind, 1), KindTile(kind, 2), called], from, called);
    }

    private static Call DaiminkanOf(int kind, PlayerIndex from)
    {
        var called = KindTile(kind, 0);
        return new Call(
            CallType.Daiminkan,
            [KindTile(kind, 1), KindTile(kind, 2), KindTile(kind, 3), called],
            from,
            called
        );
    }

    private static Call KakanOf(int kind, PlayerIndex from)
    {
        var called = KindTile(kind, 0);
        return new Call(
            CallType.Kakan,
            [KindTile(kind, 1), KindTile(kind, 2), KindTile(kind, 3), called],
            from,
            called
        );
    }

    private static Call AnkanOf(int kind)
    {
        return new Call(
            CallType.Ankan,
            [KindTile(kind, 0), KindTile(kind, 1), KindTile(kind, 2), KindTile(kind, 3)],
            new PlayerIndex(0),
            null
        );
    }

    [Fact]
    public void 三元牌が1種しか揃っていない_Noneを返す()
    {
        // Arrange
        var from = new PlayerIndex(1);
        var pon = PonOf(31, from); // 白
        CallList callList = [pon];

        // Act
        var result = PaoDetector.Detect(callList, pon);

        // Assert
        Assert.Equal(PaoYakuman.None, result);
    }

    [Fact]
    public void 三元牌が2種揃っている_Noneを返す()
    {
        // Arrange
        var from = new PlayerIndex(1);
        var pon1 = PonOf(31, from); // 白
        var pon2 = PonOf(32, from); // 發
        CallList callList = [pon1, pon2];

        // Act
        var result = PaoDetector.Detect(callList, pon2);

        // Assert
        Assert.Equal(PaoYakuman.None, result);
    }

    [Fact]
    public void 三元牌3種目の刻子を鳴いた瞬間_Daisangenを返す()
    {
        // Arrange
        var from = new PlayerIndex(1);
        var pon1 = PonOf(31, from); // 白
        var pon2 = PonOf(32, from); // 發
        var pon3 = PonOf(33, from); // 中
        CallList callList = [pon1, pon2, pon3];

        // Act
        var result = PaoDetector.Detect(callList, pon3);

        // Assert
        Assert.Equal(PaoYakuman.Daisangen, result);
    }

    [Fact]
    public void 三元牌3種目が大明槓_Daisangenを返す()
    {
        // Arrange
        var from = new PlayerIndex(2);
        var pon1 = PonOf(31, from);
        var pon2 = PonOf(32, from);
        var kan3 = DaiminkanOf(33, from);
        CallList callList = [pon1, pon2, kan3];

        // Act
        var result = PaoDetector.Detect(callList, kan3);

        // Assert
        Assert.Equal(PaoYakuman.Daisangen, result);
    }

    [Fact]
    public void 風牌4種目の刻子を鳴いた瞬間_Daisuushiiを返す()
    {
        // Arrange
        var from = new PlayerIndex(3);
        var pon1 = PonOf(27, from); // 東
        var pon2 = PonOf(28, from); // 南
        var pon3 = PonOf(29, from); // 西
        var pon4 = PonOf(30, from); // 北
        CallList callList = [pon1, pon2, pon3, pon4];

        // Act
        var result = PaoDetector.Detect(callList, pon4);

        // Assert
        Assert.Equal(PaoYakuman.Daisuushii, result);
    }

    [Fact]
    public void 風牌が3種しか揃っていない_Noneを返す()
    {
        // Arrange
        var from = new PlayerIndex(1);
        var pon1 = PonOf(27, from);
        var pon2 = PonOf(28, from);
        var pon3 = PonOf(29, from);
        CallList callList = [pon1, pon2, pon3];

        // Act
        var result = PaoDetector.Detect(callList, pon3);

        // Assert
        Assert.Equal(PaoYakuman.None, result);
    }

    [Fact]
    public void 四槓子_4つ目が大明槓で確定_Suukantsuを返す()
    {
        // Arrange
        var from = new PlayerIndex(2);
        var kan1 = DaiminkanOf(0, from); // 一萬
        var kan2 = DaiminkanOf(9, from); // 一筒
        var kan3 = DaiminkanOf(18, from); // 一索
        var kan4 = DaiminkanOf(27, from); // 東
        CallList callList = [kan1, kan2, kan3, kan4];

        // Act
        var result = PaoDetector.Detect(callList, kan4);

        // Assert
        Assert.Equal(PaoYakuman.Suukantsu, result);
    }

    [Fact]
    public void 四槓子_4つ目が加槓で確定_Suukantsuを返す()
    {
        // Arrange
        var from = new PlayerIndex(2);
        var kan1 = DaiminkanOf(0, from);
        var kan2 = DaiminkanOf(9, from);
        var kan3 = DaiminkanOf(18, from);
        var kan4 = KakanOf(27, from);
        CallList callList = [kan1, kan2, kan3, kan4];

        // Act
        var result = PaoDetector.Detect(callList, kan4);

        // Assert
        Assert.Equal(PaoYakuman.Suukantsu, result);
    }

    [Fact]
    public void 四槓子_最後が暗槓_Noneを返す()
    {
        // Arrange
        var from = new PlayerIndex(2);
        var kan1 = DaiminkanOf(0, from);
        var kan2 = DaiminkanOf(9, from);
        var kan3 = DaiminkanOf(18, from);
        var ankan4 = AnkanOf(27);
        CallList callList = [kan1, kan2, kan3, ankan4];

        // Act
        var result = PaoDetector.Detect(callList, ankan4);

        // Assert
        Assert.Equal(PaoYakuman.None, result);
    }

    [Fact]
    public void 大三元_トリガ副露が暗槓の3種目_Noneを返す()
    {
        // Arrange
        // ポン2種 + 暗槓1種 の場合、暗槓は責任者なしのためNone
        var from = new PlayerIndex(1);
        var pon1 = PonOf(31, from);
        var pon2 = PonOf(32, from);
        var ankan3 = AnkanOf(33);
        CallList callList = [pon1, pon2, ankan3];

        // Act
        var result = PaoDetector.Detect(callList, ankan3);

        // Assert
        Assert.Equal(PaoYakuman.None, result);
    }

    [Fact]
    public void 三元牌3種ポン済みで加槓_Noneを返す()
    {
        // Arrange
        // 3 種ポン済みの状態で既存ポン (白) を加槓。大三元はポン時点で既に確定しており、
        // 加槓は新規牌種を増やさないため Daisangen の再トリガにはならない (責任者の誤上書き防止)
        var from = new PlayerIndex(1);
        var pon2 = PonOf(32, from); // 發
        var pon3 = PonOf(33, from); // 中
        var kakan1 = KakanOf(31, from); // 白 加槓 (既存ポンの差し替え後)
        CallList callList = [pon2, pon3, kakan1];

        // Act
        var result = PaoDetector.Detect(callList, kakan1);

        // Assert
        Assert.Equal(PaoYakuman.None, result);
    }

    [Fact]
    public void 風牌4種ポン済みで加槓_Noneを返す()
    {
        // Arrange
        // 大四喜でも同様。4 種ポン済みの状態で加槓しても再トリガしない
        var from = new PlayerIndex(2);
        var pon2 = PonOf(28, from); // 南
        var pon3 = PonOf(29, from); // 西
        var pon4 = PonOf(30, from); // 北
        var kakan1 = KakanOf(27, from); // 東 加槓
        CallList callList = [pon2, pon3, pon4, kakan1];

        // Act
        var result = PaoDetector.Detect(callList, kakan1);

        // Assert
        Assert.Equal(PaoYakuman.None, result);
    }

    [Fact]
    public void 加槓4槓目かつ風牌4種既に鳴き済み_Suukantsuを返す()
    {
        // Arrange
        // 風牌 4 種鳴き済み + 追加の大明槓 2 つで計 3 槓の状態。
        // ここで既存ポン (東) を加槓して 4 槓目とする。
        // Kakan は Daisuushii の再トリガ対象外、Suukantsu のみ返す (旧実装では誤って Daisuushii を返していた)
        var from = new PlayerIndex(3);
        var pon2 = PonOf(28, from); // 南
        var pon3 = PonOf(29, from); // 西
        var daiminkan4 = DaiminkanOf(30, from); // 北 (1 槓目、原典の Daisuushii 確定契機)
        var daiminkan5 = DaiminkanOf(0, from); // 一萬 (2 槓目)
        var daiminkan6 = DaiminkanOf(9, from); // 一筒 (3 槓目)
        var kakan1 = KakanOf(27, from); // 東 加槓 (4 槓目、元ポン東を差し替え)
        CallList callList = [pon2, pon3, daiminkan4, daiminkan5, daiminkan6, kakan1];

        // Act
        var result = PaoDetector.Detect(callList, kakan1);

        // Assert
        Assert.Equal(PaoYakuman.Suukantsu, result);
    }
}
