using Mahjong.Lib.Scoring.Calls;
using Mahjong.Lib.Scoring.Tiles;

namespace Mahjong.Lib.Scoring.Tests.Calls;

public class Call_ConstructorTests
{
    [Fact]
    public void チーの場合_順子で正常に作成される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123");

        // Act
        var call = new Call(CallType.Chi, tileKindList);

        // Assert
        Assert.Equal(CallType.Chi, call.Type);
        Assert.Equal(tileKindList, call.TileKindList);
        Assert.True(call.IsChi);
    }

    [Fact]
    public void ポンの場合_刻子で正常に作成される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "111");

        // Act
        var call = new Call(CallType.Pon, tileKindList);

        // Assert
        Assert.Equal(CallType.Pon, call.Type);
        Assert.Equal(tileKindList, call.TileKindList);
        Assert.True(call.IsPon);
    }

    [Fact]
    public void 暗槓の場合_槓子で正常に作成される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "1111");

        // Act
        var call = new Call(CallType.Ankan, tileKindList);

        // Assert
        Assert.Equal(CallType.Ankan, call.Type);
        Assert.Equal(tileKindList, call.TileKindList);
        Assert.True(call.IsAnkan);
        Assert.True(call.IsKan);
    }

    [Fact]
    public void 明槓の場合_槓子で正常に作成される()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "1111");

        // Act
        var call = new Call(CallType.Minkan, tileKindList);

        // Assert
        Assert.Equal(CallType.Minkan, call.Type);
        Assert.Equal(tileKindList, call.TileKindList);
        Assert.True(call.IsMinkan);
        Assert.True(call.IsKan);
    }

    [Fact]
    public void 抜きの場合_正常に作成される()
    {
        // Arrange
        var tileKindList = new TileKindList([TileKind.Pei]);

        // Act
        var call = new Call(CallType.Nuki, tileKindList);

        // Assert
        Assert.Equal(CallType.Nuki, call.Type);
        Assert.Equal(tileKindList, call.TileKindList);
        Assert.True(call.IsNuki);
    }

    [Fact]
    public void チーに順子以外_ArgumentExceptionが発生する()
    {
        // Arrange
        var koutsuList = new TileKindList(man: "111");

        // Act
        var ex = Record.Exception(() => new Call(CallType.Chi, koutsuList));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("チーの構成牌は順子でなければなりません", ex.Message);
        Assert.Equal("tileKindList", ((ArgumentException)ex).ParamName);
    }

    [Fact]
    public void チーに空の牌種別リスト_ArgumentExceptionが発生する()
    {
        // Arrange
        var emptyList = new TileKindList();

        // Act
        var ex = Record.Exception(() => new Call(CallType.Chi, emptyList));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("チーの構成牌は順子でなければなりません", ex.Message);
        Assert.Equal("tileKindList", ((ArgumentException)ex).ParamName);
    }

    [Fact]
    public void ポンに刻子以外_ArgumentExceptionが発生する()
    {
        // Arrange
        var shuntsuList = new TileKindList(man: "123");

        // Act
        var ex = Record.Exception(() => new Call(CallType.Pon, shuntsuList));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("ポンの構成牌は刻子でなければなりません", ex.Message);
        Assert.Equal("tileKindList", ((ArgumentException)ex).ParamName);
    }

    [Fact]
    public void 暗槓に槓子以外_ArgumentExceptionが発生する()
    {
        // Arrange
        var shuntsuList = new TileKindList(man: "123");

        // Act
        var ex = Record.Exception(() => new Call(CallType.Ankan, shuntsuList));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("暗槓の構成牌は槓子でなければなりません", ex.Message);
        Assert.Equal("tileKindList", ((ArgumentException)ex).ParamName);
    }

    [Fact]
    public void 明槓に槓子以外_ArgumentExceptionが発生する()
    {
        // Arrange
        var shuntsuList = new TileKindList(man: "123");

        // Act
        var ex = Record.Exception(() => new Call(CallType.Minkan, shuntsuList));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("明槓の構成牌は槓子でなければなりません", ex.Message);
        Assert.Equal("tileKindList", ((ArgumentException)ex).ParamName);
    }

    [Fact]
    public void 抜きに空の牌種別リスト_ArgumentExceptionが発生する()
    {
        // Arrange
        var emptyList = new TileKindList();

        // Act
        var ex = Record.Exception(() => new Call(CallType.Nuki, emptyList));

        // Assert
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("抜きの構成牌は1つ以上の牌を含む必要があります", ex.Message);
        Assert.Equal("tileKindList", ((ArgumentException)ex).ParamName);
    }

    [Fact]
    public void 未定義のCallType_ArgumentOutOfRangeExceptionが発生する()
    {
        // Arrange
        var tileKindList = new TileKindList(man: "123");
        var undefinedCallType = (CallType)999;

        // Act
        var ex = Record.Exception(() => new Call(undefinedCallType, tileKindList));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
        Assert.Equal("type", ((ArgumentOutOfRangeException)ex).ParamName);
    }
}
