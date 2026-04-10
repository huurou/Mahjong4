using Mahjong.Lib.Fus;

namespace Mahjong.Lib.Tests.Fus;

public class Fu_StaticPropertyTests
{
    [Fact]
    public void FuTei_副底を返す()
    {
        // Arrange & Act
        var fu = Fu.Futei;

        // Assert
        Assert.Equal(FuType.Futei, fu.Type);
        Assert.Equal(20, fu.Value);
        Assert.Equal(0, fu.Number);
    }

    [Fact]
    public void Menzen_面前加符を返す()
    {
        // Arrange & Act
        var fu = Fu.Menzen;

        // Assert
        Assert.Equal(FuType.Menzen, fu.Type);
        Assert.Equal(10, fu.Value);
        Assert.Equal(1, fu.Number);
    }

    [Fact]
    public void Chiitoitsu_七対子符を返す()
    {
        // Arrange & Act
        var fu = Fu.Chiitoitsu;

        // Assert
        Assert.Equal(FuType.Chiitoitsu, fu.Type);
        Assert.Equal(25, fu.Value);
        Assert.Equal(2, fu.Number);
    }

    [Fact]
    public void FuteiOpenPinfu_副底食い平和を返す()
    {
        // Arrange & Act
        var fu = Fu.FuteiOpenPinfu;

        // Assert
        Assert.Equal(FuType.FuteiOpenPinfu, fu.Type);
        Assert.Equal(30, fu.Value);
        Assert.Equal(3, fu.Number);
    }

    [Fact]
    public void Tsumo_ツモ符を返す()
    {
        // Arrange & Act
        var fu = Fu.Tsumo;

        // Assert
        Assert.Equal(FuType.Tsumo, fu.Type);
        Assert.Equal(2, fu.Value);
        Assert.Equal(4, fu.Number);
    }

    [Fact]
    public void Kanchan_カンチャン待ちを返す()
    {
        // Arrange & Act
        var fu = Fu.Kanchan;

        // Assert
        Assert.Equal(FuType.Kanchan, fu.Type);
        Assert.Equal(2, fu.Value);
        Assert.Equal(5, fu.Number);
    }

    [Fact]
    public void Penchan_ペンチャン待ちを返す()
    {
        // Arrange & Act
        var fu = Fu.Penchan;

        // Assert
        Assert.Equal(FuType.Penchan, fu.Type);
        Assert.Equal(2, fu.Value);
        Assert.Equal(6, fu.Number);
    }

    [Fact]
    public void Tanki_単騎待ちを返す()
    {
        // Arrange & Act
        var fu = Fu.Tanki;

        // Assert
        Assert.Equal(FuType.Tanki, fu.Type);
        Assert.Equal(2, fu.Value);
        Assert.Equal(7, fu.Number);
    }

    [Fact]
    public void JantouPlayerWind_自風の雀頭を返す()
    {
        // Arrange & Act
        var fu = Fu.JantouPlayerWind;

        // Assert
        Assert.Equal(FuType.JantouPlayerWind, fu.Type);
        Assert.Equal(2, fu.Value);
        Assert.Equal(8, fu.Number);
    }

    [Fact]
    public void JantouRoundWind_場風の雀頭を返す()
    {
        // Arrange & Act
        var fu = Fu.JantouRoundWind;

        // Assert
        Assert.Equal(FuType.JantouRoundWind, fu.Type);
        Assert.Equal(2, fu.Value);
        Assert.Equal(9, fu.Number);
    }

    [Fact]
    public void JantouDragon_三元牌の雀頭を返す()
    {
        // Arrange & Act
        var fu = Fu.JantouDragon;

        // Assert
        Assert.Equal(FuType.JantouDragon, fu.Type);
        Assert.Equal(2, fu.Value);
        Assert.Equal(10, fu.Number);
    }

    [Fact]
    public void MinkoChunchan_中張牌の明刻を返す()
    {
        // Arrange & Act
        var fu = Fu.MinkoChunchan;

        // Assert
        Assert.Equal(FuType.MinkoChunchan, fu.Type);
        Assert.Equal(2, fu.Value);
        Assert.Equal(11, fu.Number);
    }

    [Fact]
    public void MinkoYaochu_么九牌の明刻を返す()
    {
        // Arrange & Act
        var fu = Fu.MinkoYaochu;

        // Assert
        Assert.Equal(FuType.MinkoYaochu, fu.Type);
        Assert.Equal(4, fu.Value);
        Assert.Equal(12, fu.Number);
    }

    [Fact]
    public void AnkoChunchan_中張牌の暗刻を返す()
    {
        // Arrange & Act
        var fu = Fu.AnkoChunchan;

        // Assert
        Assert.Equal(FuType.AnkoChunchan, fu.Type);
        Assert.Equal(4, fu.Value);
        Assert.Equal(13, fu.Number);
    }

    [Fact]
    public void AnkoYaochu_么九牌の暗刻を返す()
    {
        // Arrange & Act
        var fu = Fu.AnkoYaochu;

        // Assert
        Assert.Equal(FuType.AnkoYaochu, fu.Type);
        Assert.Equal(8, fu.Value);
        Assert.Equal(14, fu.Number);
    }

    [Fact]
    public void MinkanChunchan_中張牌の明槓を返す()
    {
        // Arrange & Act
        var fu = Fu.MinkanChunchan;

        // Assert
        Assert.Equal(FuType.MinkanChunchan, fu.Type);
        Assert.Equal(8, fu.Value);
        Assert.Equal(15, fu.Number);
    }

    [Fact]
    public void MinkanYaochu_么九牌の明槓を返す()
    {
        // Arrange & Act
        var fu = Fu.MinkanYaochu;

        // Assert
        Assert.Equal(FuType.MinkanYaochu, fu.Type);
        Assert.Equal(16, fu.Value);
        Assert.Equal(16, fu.Number);
    }

    [Fact]
    public void AnkanChunchan_中張牌の暗槓を返す()
    {
        // Arrange & Act
        var fu = Fu.AnkanChunchan;

        // Assert
        Assert.Equal(FuType.AnkanChunchan, fu.Type);
        Assert.Equal(16, fu.Value);
        Assert.Equal(17, fu.Number);
    }

    [Fact]
    public void AnkanYaochu_么九牌の暗槓を返す()
    {
        // Arrange & Act
        var fu = Fu.AnkanYaochu;

        // Assert
        Assert.Equal(FuType.AnkanYaochu, fu.Type);
        Assert.Equal(32, fu.Value);
        Assert.Equal(18, fu.Number);
    }
}
