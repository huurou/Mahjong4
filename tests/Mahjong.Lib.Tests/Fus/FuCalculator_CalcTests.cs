using Mahjong.Lib.Calls;
using Mahjong.Lib.Fus;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Fus;

public class FuCalculator_CalcTests
{
    [Fact]
    public void 省略可能な引数を省略_既定値で計算される()
    {
        // Arrange
        var hand = new Hand([new(man: "234"), new(man: "567"), new(pin: "234"), new(sou: "234"), new(sou: "55")]);
        var winTile = TileKind.Sou4;
        var winGroup = new TileKindList(sou: "234");

        // Act
        var actual = FuCalculator.Calc(hand, winTile, winGroup);

        // Assert: 副底20 + 面前ロン10 = 30符
        Assert.Equal(30, actual.Total);
    }

    [Fact]
    public void カンチャン待ち_カンチャン符が付与される()
    {
        // Arrange: 3-5待ちのカンチャンに4でアガリ
        var hand = new Hand([new(man: "234"), new(man: "567"), new(pin: "345"), new(sou: "234"), new(sou: "55")]);
        var winTile = TileKind.Pin4;
        var winGroup = new TileKindList(pin: "345");

        // Act
        var actual = FuCalculator.Calc(hand, winTile, winGroup);

        // Assert
        Assert.Contains(Fu.Kanchan, actual);
    }

    [Fact]
    public void ペンチャン待ち_123_ペンチャン符が付与される()
    {
        // Arrange: 1-2待ちのペンチャンに3でアガリ
        var hand = new Hand([new(man: "123"), new(man: "567"), new(pin: "234"), new(sou: "234"), new(sou: "55")]);
        var winTile = TileKind.Man3;
        var winGroup = new TileKindList(man: "123");

        // Act
        var actual = FuCalculator.Calc(hand, winTile, winGroup);

        // Assert
        Assert.Contains(Fu.Penchan, actual);
    }

    [Fact]
    public void ペンチャン待ち_789_ペンチャン符が付与される()
    {
        // Arrange: 8-9待ちのペンチャンに7でアガリ
        var hand = new Hand([new(man: "789"), new(man: "234"), new(pin: "234"), new(sou: "234"), new(sou: "55")]);
        var winTile = TileKind.Man7;
        var winGroup = new TileKindList(man: "789");

        // Act
        var actual = FuCalculator.Calc(hand, winTile, winGroup);

        // Assert
        Assert.Contains(Fu.Penchan, actual);
    }

    [Fact]
    public void 食い平和_ロンアガリ_副底が30符になる()
    {
        // Arrange: 副露1つ・4順子1雀頭・両面待ち・非役牌雀頭 → 食い平和
        var hand = new Hand([new(man: "234"), new(pin: "234"), new(pin: "567"), new(sou: "88")]);
        var winTile = TileKind.Man4;
        var winGroup = new TileKindList(man: "234");
        var callList = new CallList([Call.Chi(sou: "234")]);
        var winSituation = new WinSituation { IsTsumo = false };

        // Act
        var actual = FuCalculator.Calc(hand, winTile, winGroup, callList, winSituation);

        // Assert: 30符（食い平和の副底）
        Assert.Contains(Fu.FuteiOpenPinfu, actual);
        Assert.Equal(30, actual.Total);
    }

    [Fact]
    public void ピンヅモあり_面前ツモ平和形_ツモ符が付かない()
    {
        // Arrange: 4順子1雀頭・両面待ち・ツモ
        var hand = new Hand([new(man: "234"), new(man: "567"), new(pin: "234"), new(sou: "234"), new(sou: "55")]);
        var winTile = TileKind.Sou4;
        var winGroup = new TileKindList(sou: "234");
        var winSituation = new WinSituation { IsTsumo = true };
        var gameRules = new GameRules { PinzumoEnabled = true };

        // Act
        var actual = FuCalculator.Calc(hand, winTile, winGroup, winSituation: winSituation, gameRules: gameRules);

        // Assert: 副底20符のみ
        Assert.DoesNotContain(Fu.Tsumo, actual);
        Assert.Equal(20, actual.Total);
    }

    [Fact]
    public void ピンヅモなし_面前ツモ平和形_ツモ符が付く()
    {
        // Arrange
        var hand = new Hand([new(man: "234"), new(man: "567"), new(pin: "234"), new(sou: "234"), new(sou: "55")]);
        var winTile = TileKind.Sou4;
        var winGroup = new TileKindList(sou: "234");
        var winSituation = new WinSituation { IsTsumo = true };
        var gameRules = new GameRules { PinzumoEnabled = false };

        // Act
        var actual = FuCalculator.Calc(hand, winTile, winGroup, winSituation: winSituation, gameRules: gameRules);

        // Assert
        Assert.Contains(Fu.Tsumo, actual);
    }
}
