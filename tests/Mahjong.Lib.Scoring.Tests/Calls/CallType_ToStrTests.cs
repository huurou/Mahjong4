using Mahjong.Lib.Scoring.Calls;

namespace Mahjong.Lib.Scoring.Tests.Calls;

public class CallType_ToStrTests
{
    [Fact]
    public void Chi_チーを返す()
    {
        // Arrange & Act
        var result = CallType.Chi.ToStr();

        // Assert
        Assert.Equal("チー", result);
    }

    [Fact]
    public void Pon_ポンを返す()
    {
        // Arrange & Act
        var result = CallType.Pon.ToStr();

        // Assert
        Assert.Equal("ポン", result);
    }

    [Fact]
    public void Ankan_暗槓を返す()
    {
        // Arrange & Act
        var result = CallType.Ankan.ToStr();

        // Assert
        Assert.Equal("暗槓", result);
    }

    [Fact]
    public void Minkan_明槓を返す()
    {
        // Arrange & Act
        var result = CallType.Minkan.ToStr();

        // Assert
        Assert.Equal("明槓", result);
    }

    [Fact]
    public void Nuki_抜きを返す()
    {
        // Arrange & Act
        var result = CallType.Nuki.ToStr();

        // Assert
        Assert.Equal("抜き", result);
    }

    [Fact]
    public void 未定義の値_ArgumentOutOfRangeExceptionが発生する()
    {
        // Arrange
        var undefinedCallType = (CallType)999;

        // Act
        var ex = Record.Exception(() => undefinedCallType.ToStr());

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(ex);
        Assert.Equal("callType", ((ArgumentOutOfRangeException)ex).ParamName);
    }
}
