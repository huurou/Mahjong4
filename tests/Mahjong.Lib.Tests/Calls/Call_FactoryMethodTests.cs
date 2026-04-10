using Mahjong.Lib.Calls;
using Mahjong.Lib.Tiles;

namespace Mahjong.Lib.Tests.Calls;

public class Call_FactoryMethodTests
{
    [Fact]
    public void Chi_TileKindList版_順子で正常に作成される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123");

        // Act
        var call = Call.Chi(tileKindList);

        // Assert
        Assert.Equal(CallType.Chi, call.Type);
        Assert.Equal(tileKindList, call.TileKindList);
        Assert.True(call.IsChi);
    }

    [Fact]
    public void Chi_TileKindList版_刻子_ArgumentExceptionが発生する()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111");

        // Act
        var ex = Record.Exception(() => Call.Chi(tileKindList));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("チーの構成牌は順子でなければなりません", ex.Message);
    }

    [Fact]
    public void Chi_文字列パラメータ_萬子順子_正常に作成される()
    {
        // Act
        var call = Call.Chi(man: "123");

        // Assert
        Assert.Equal(CallType.Chi, call.Type);
        Assert.True(call.IsChi);
        Assert.Equal("一二三", call.TileKindList.ToString());
    }

    [Fact]
    public void Chi_文字列パラメータ_筒子順子_正常に作成される()
    {
        // Act
        var call = Call.Chi(pin: "456");

        // Assert
        Assert.Equal(CallType.Chi, call.Type);
        Assert.True(call.IsChi);
        Assert.Equal("(4)(5)(6)", call.TileKindList.ToString());
    }

    [Fact]
    public void Chi_文字列パラメータ_索子順子_正常に作成される()
    {
        // Act
        var call = Call.Chi(sou: "789");

        // Assert
        Assert.Equal(CallType.Chi, call.Type);
        Assert.True(call.IsChi);
        Assert.Equal("789", call.TileKindList.ToString());
    }

    [Fact]
    public void Chi_文字列パラメータ_無効な組み合わせ_ArgumentExceptionが発生する()
    {
        // Act
        var ex = Record.Exception(() => Call.Chi(man: "111"));

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void Pon_TileKindList版_刻子で正常に作成される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111");

        // Act
        var call = Call.Pon(tileKindList);

        // Assert
        Assert.Equal(CallType.Pon, call.Type);
        Assert.Equal(tileKindList, call.TileKindList);
        Assert.True(call.IsPon);
    }

    [Fact]
    public void Pon_TileKindList版_順子_ArgumentExceptionが発生する()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123");

        // Act
        var ex = Record.Exception(() => Call.Pon(tileKindList));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("ポンの構成牌は刻子でなければなりません", ex.Message);
    }

    [Fact]
    public void Pon_文字列パラメータ_萬子刻子_正常に作成される()
    {
        // Act
        var call = Call.Pon(man: "111");

        // Assert
        Assert.Equal(CallType.Pon, call.Type);
        Assert.True(call.IsPon);
        Assert.Equal("一一一", call.TileKindList.ToString());
    }

    [Fact]
    public void Pon_文字列パラメータ_筒子刻子_正常に作成される()
    {
        // Act
        var call = Call.Pon(pin: "555");

        // Assert
        Assert.Equal(CallType.Pon, call.Type);
        Assert.True(call.IsPon);
        Assert.Equal("(5)(5)(5)", call.TileKindList.ToString());
    }

    [Fact]
    public void Pon_文字列パラメータ_字牌刻子_正常に作成される()
    {
        // Act
        var call = Call.Pon(honor: "ttt");

        // Assert
        Assert.Equal(CallType.Pon, call.Type);
        Assert.True(call.IsPon);
        Assert.Equal("東東東", call.TileKindList.ToString());
    }

    [Fact]
    public void Ankan_TileKindList版_槓子で正常に作成される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "1111");

        // Act
        var call = Call.Ankan(tileKindList);

        // Assert
        Assert.Equal(CallType.Ankan, call.Type);
        Assert.Equal(tileKindList, call.TileKindList);
        Assert.True(call.IsAnkan);
        Assert.True(call.IsKan);
    }

    [Fact]
    public void Ankan_TileKindList版_刻子_ArgumentExceptionが発生する()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111");

        // Act
        var ex = Record.Exception(() => Call.Ankan(tileKindList));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("暗槓の構成牌は槓子でなければなりません", ex.Message);
    }

    [Fact]
    public void Ankan_文字列パラメータ_萬子槓子_正常に作成される()
    {
        // Act
        var call = Call.Ankan(man: "2222");

        // Assert
        Assert.Equal(CallType.Ankan, call.Type);
        Assert.True(call.IsAnkan);
        Assert.True(call.IsKan);
        Assert.Equal("二二二二", call.TileKindList.ToString());
    }

    [Fact]
    public void Ankan_文字列パラメータ_筒子槓子_正常に作成される()
    {
        // Act
        var call = Call.Ankan(pin: "7777");

        // Assert
        Assert.Equal(CallType.Ankan, call.Type);
        Assert.True(call.IsAnkan);
        Assert.Equal("(7)(7)(7)(7)", call.TileKindList.ToString());
    }

    [Fact]
    public void Minkan_TileKindList版_槓子で正常に作成される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "1111");

        // Act
        var call = Call.Minkan(tileKindList);

        // Assert
        Assert.Equal(CallType.Minkan, call.Type);
        Assert.Equal(tileKindList, call.TileKindList);
        Assert.True(call.IsMinkan);
        Assert.True(call.IsKan);
    }

    [Fact]
    public void Minkan_TileKindList版_刻子_ArgumentExceptionが発生する()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111");

        // Act
        var ex = Record.Exception(() => Call.Minkan(tileKindList));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("明槓の構成牌は槓子でなければなりません", ex.Message);
    }

    [Fact]
    public void Minkan_文字列パラメータ_索子槓子_正常に作成される()
    {
        // Act
        var call = Call.Minkan(sou: "3333");

        // Assert
        Assert.Equal(CallType.Minkan, call.Type);
        Assert.True(call.IsMinkan);
        Assert.True(call.IsKan);
        Assert.Equal("3333", call.TileKindList.ToString());
    }

    [Fact]
    public void Minkan_文字列パラメータ_字牌槓子_正常に作成される()
    {
        // Act
        var call = Call.Minkan(honor: "cccc");

        // Assert
        Assert.Equal(CallType.Minkan, call.Type);
        Assert.True(call.IsMinkan);
        Assert.Equal("中中中中", call.TileKindList.ToString());
    }

    [Fact]
    public void Nuki_TileKindList版_正常に作成される()
    {
        // Arrange
        var tileKindList = new TileKindList([TileKind.Pei]);

        // Act
        var call = Call.Nuki(tileKindList);

        // Assert
        Assert.Equal(CallType.Nuki, call.Type);
        Assert.Equal(tileKindList, call.TileKindList);
        Assert.True(call.IsNuki);
    }

    [Fact]
    public void Nuki_TileKindList版_空リスト_ArgumentExceptionが発生する()
    {
        // Arrange
        var tileKindList = new TileKindList();

        // Act
        var ex = Record.Exception(() => Call.Nuki(tileKindList));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("抜きの構成牌は1つ以上の牌を含む必要があります", ex.Message);
    }

    [Fact]
    public void Nuki_文字列パラメータ_萬子_正常に作成される()
    {
        // Act
        var call = Call.Nuki(man: "1");

        // Assert
        Assert.Equal(CallType.Nuki, call.Type);
        Assert.True(call.IsNuki);
        Assert.Equal("一", call.TileKindList.ToString());
    }

    [Fact]
    public void Nuki_文字列パラメータ_字牌_正常に作成される()
    {
        // Act
        var call = Call.Nuki(honor: "p");

        // Assert
        Assert.Equal(CallType.Nuki, call.Type);
        Assert.True(call.IsNuki);
        Assert.Equal("北", call.TileKindList.ToString());
    }

    [Fact]
    public void Nuki_文字列パラメータ_引数なし_ArgumentExceptionが発生する()
    {
        // Act
        var ex = Record.Exception(() => Call.Nuki());

        // Assert
        Assert.IsType<ArgumentException>(ex);
    }
}
