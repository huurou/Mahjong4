using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.HandCalculating;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.HandCalculating;

public partial class HandCalculator_CalcTests
{
    [Fact]
    public void アガリ形ではない手牌_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123459");
        var winTile = TileKind.Sou9;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Equal("手牌がアガリ形ではありません。", actual.ErrorMessage);
    }

    [Fact]
    public void 手牌にアガリ牌がない_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou5;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Equal("手牌にアガリ牌がありません。", actual.ErrorMessage);
    }

    [Fact]
    public void 立直かつ非面前_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var callList = new CallList([Call.Chi(new(man: "123"))]);
        var winSituation = new WinSituation { IsRiichi = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, callList, winSituation: winSituation);

        // Assert
        Assert.Equal("立直と非面前は両立できません。", actual.ErrorMessage);
    }

    [Fact]
    public void ダブル立直かつ非面前_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var callList = new CallList([Call.Chi(new(man: "123"))]);
        var winSituation = new WinSituation { IsDoubleRiichi = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, callList, winSituation: winSituation);

        // Assert
        Assert.Equal("ダブル立直と非面前は両立できません。", actual.ErrorMessage);
    }

    [Fact]
    public void 一発かつ非面前_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var callList = new CallList([Call.Chi(new(man: "123"))]);
        var winSituation = new WinSituation { IsIppatsu = true, IsRiichi = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, callList, winSituation: winSituation);

        // Assert
        Assert.Equal("一発と非面前は両立できません。", actual.ErrorMessage);
    }

    [Fact]
    public void 一発かつ立直なし_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsIppatsu = true, IsRiichi = false };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("一発は立直orダブル立直時にしか成立しません。", actual.ErrorMessage);
    }

    [Fact]
    public void 槍槓かつツモアガリ_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsChankan = true, IsTsumo = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("槍槓とツモアガリは両立できません。", actual.ErrorMessage);
    }

    [Fact]
    public void 嶺上開花かつロンアガリ_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsRinshan = true, IsTsumo = false };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("嶺上開花とロンアガリは両立できません。", actual.ErrorMessage);
    }

    [Fact]
    public void 海底撈月かつロンアガリ_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsHaitei = true, IsTsumo = false };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("海底撈月とロンアガリは両立できません。", actual.ErrorMessage);
    }

    [Fact]
    public void 河底撈魚かつツモアガリ_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsHoutei = true, IsTsumo = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("河底撈魚とツモアガリは両立できません。", actual.ErrorMessage);
    }

    [Fact]
    public void 海底撈月かつ嶺上開花_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsHaitei = true, IsRinshan = true, IsTsumo = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("海底撈月と嶺上開花は両立できません。", actual.ErrorMessage);
    }

    [Fact]
    public void 河底撈魚かつ槍槓_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsHoutei = true, IsChankan = true, IsTsumo = false };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("河底撈魚と槍槓は両立できません。", actual.ErrorMessage);
    }

    [Fact]
    public void 天和かつ子_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winSituation = new WinSituation { IsTenhou = true, PlayerWind = Wind.South, IsTsumo = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, null, winSituation: winSituation);

        // Assert
        Assert.Equal("天和はプレイヤーが親の時のみ有効です。", actual.ErrorMessage);
    }

    [Fact]
    public void 天和かつロン和了_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = null as TileKind;
        var winSituation = new WinSituation { IsTenhou = true, IsTsumo = false, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("天和とロンアガリは両立できません。", actual.ErrorMessage);
    }

    [Fact]
    public void 副露を伴う天和_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "66", sou: "123444");
        var winTile = null as TileKind;
        var callList = new CallList([Call.Chi(new(man: "789"))]);
        var winSituation = new WinSituation { IsTenhou = true, IsTsumo = true, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, callList, winSituation: winSituation);

        // Assert
        Assert.Equal("副露を伴う天和は無効です。", actual.ErrorMessage);
    }

    [Fact]
    public void 天和時に和了牌が指定されている_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsTenhou = true, IsTsumo = true, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("天和の時は和了牌にnullを指定してください。", actual.ErrorMessage);
    }

    [Fact]
    public void 流し満貫時に和了牌が指定されている_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Man1;
        var winSituation = new WinSituation { IsNagashimangan = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("流し満貫の時は和了牌にnullを指定してください。", actual.ErrorMessage);
    }

    [Fact]
    public void 地和かつ親_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsChiihou = true, PlayerWind = Wind.East, IsTsumo = true };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("地和はプレイヤーが子の時のみ有効です。", actual.ErrorMessage);
    }

    [Fact]
    public void 地和かつロン和了_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsChiihou = true, IsTsumo = false, PlayerWind = Wind.South };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("地和とロンアガリは両立できません。", actual.ErrorMessage);
    }

    [Fact]
    public void 地和かつ副露_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var callList = new CallList([Call.Chi(new(man: "789"))]);
        var winSituation = new WinSituation { IsChiihou = true, IsTsumo = true, PlayerWind = Wind.South };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, callList, winSituation: winSituation);

        // Assert
        Assert.Equal("副露を伴う地和は無効です。", actual.ErrorMessage);
    }

    [Fact]
    public void 人和かつ親_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsRenhou = true, PlayerWind = Wind.East };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("人和はプレイヤーが子の時のみ有効です。", actual.ErrorMessage);
    }

    [Fact]
    public void 人和かつツモ和了_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var winSituation = new WinSituation { IsRenhou = true, IsTsumo = true, PlayerWind = Wind.South };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, winSituation: winSituation);

        // Assert
        Assert.Equal("人和とツモアガリは両立できません。", actual.ErrorMessage);
    }

    [Fact]
    public void 人和かつ副露_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var callList = new CallList([Call.Chi(new(man: "789"))]);
        var winSituation = new WinSituation { IsRenhou = true, IsTsumo = false, PlayerWind = Wind.South };

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, callList, winSituation: winSituation);

        // Assert
        Assert.Equal("副露を伴う人和は無効です。", actual.ErrorMessage);
    }

    [Fact]
    public void 通常和了で和了牌がnull_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        TileKind? winTile = null;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Equal("和了牌が指定されていません。", actual.ErrorMessage);
    }

    [Fact]
    public void 手牌枚数が14枚未満で副露なし_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "12345");
        var winTile = TileKind.Sou5;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Equal("手牌の枚数が不正です。期待値: 14, 実際: 13", actual.ErrorMessage);
    }

    [Fact]
    public void 副露数に対して手牌枚数が不整合_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123456", pin: "66", sou: "123444");
        var winTile = TileKind.Sou4;
        var callList = new CallList([Call.Chi(new(man: "789"))]);

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, callList);

        // Assert
        Assert.Equal("手牌の枚数が不正です。期待値: 11, 実際: 14", actual.ErrorMessage);
    }

    [Fact]
    public void 副露が5つ以上_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "11");
        var winTile = TileKind.Man1;
        var callList = new CallList([
            Call.Pon(new(pin: "222")),
            Call.Pon(new(pin: "333")),
            Call.Pon(new(pin: "444")),
            Call.Pon(new(pin: "555")),
            Call.Pon(new(pin: "666")),
        ]);

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile, callList);

        // Assert
        Assert.Equal("副露は4つまでしか指定できません。", actual.ErrorMessage);
    }

    [Fact]
    public void 同種の牌が5枚以上_エラーメッセージが返される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "11111234", pin: "66", sou: "2345");
        var winTile = TileKind.Man4;

        // Act
        var actual = HandCalculator.Calc(tileKindList, winTile);

        // Assert
        Assert.Equal("同種の牌が4枚を超えています。", actual.ErrorMessage);
    }

}
