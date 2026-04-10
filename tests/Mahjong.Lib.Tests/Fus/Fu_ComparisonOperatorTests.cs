using Mahjong.Lib.Fus;

namespace Mahjong.Lib.Tests.Fus;

public class Fu_ComparisonOperatorTests
{
    [Fact]
    public void 小なり演算子_Typeが小さい場合_trueを返す()
    {
        // Arrange & Act & Assert
        Assert.True(Fu.Futei < Fu.Menzen);
        Assert.False(Fu.Menzen < Fu.Futei);
        Assert.False(Fu.Tsumo < Fu.Tsumo);
    }

    [Fact]
    public void 大なり演算子_Typeが大きい場合_trueを返す()
    {
        // Arrange & Act & Assert
        Assert.True(Fu.Menzen > Fu.Futei);
        Assert.False(Fu.Futei > Fu.Menzen);
        Assert.False(Fu.Tsumo > Fu.Tsumo);
    }

    [Fact]
    public void 以下演算子_Type以下の場合_trueを返す()
    {
        // Arrange & Act & Assert
        Assert.True(Fu.Futei <= Fu.Menzen);
        Assert.True(Fu.Tsumo <= Fu.Tsumo);
        Assert.False(Fu.Menzen <= Fu.Futei);
    }

    [Fact]
    public void 以上演算子_Type以上の場合_trueを返す()
    {
        // Arrange & Act & Assert
        Assert.True(Fu.Menzen >= Fu.Futei);
        Assert.True(Fu.Tsumo >= Fu.Tsumo);
        Assert.False(Fu.Futei >= Fu.Menzen);
    }

    [Fact]
    public void Null比較_正しく処理される()
    {
        // Arrange
        Fu? nullFu = null;

        // Act & Assert
        Assert.True(nullFu < Fu.Tsumo);
        Assert.False(nullFu > Fu.Tsumo);
        Assert.True(nullFu <= Fu.Tsumo);
        Assert.False(nullFu >= Fu.Tsumo);
    }
}
