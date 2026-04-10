using Mahjong.Lib.Calls;
using Mahjong.Lib.Games;
using Mahjong.Lib.Tiles;
using Mahjong.Lib.Yakus;
using Mahjong.Lib.Yakus.Impl;

namespace Mahjong.Lib.Tests.Yakus;

public class Ippatsu_ValidTests
{

    [Fact]
    public void Valid_一発と立直があり面前の場合_成立する()
    {
        // Arrange
        var winSituation = new WinSituation
        {
            IsIppatsu = true,
            IsRiichi = true,
        };
        var callList = new CallList(); // 副露なし（面前）

        // Act
        var actual = Ippatsu.Valid(winSituation, callList);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Valid_一発が成立していない場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation { IsIppatsu = false };
        var callList = new CallList();

        // Act
        var actual = Ippatsu.Valid(winSituation, callList);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Valid_一発が成立するが副露がある場合_成立しない()
    {
        // Arrange
        var winSituation = new WinSituation { IsIppatsu = true };
        // 副露あり
        var callList = new CallList([
            Call.Pon(new TileKindList(man: "111"))
        ]);

        // Act
        var actual = Ippatsu.Valid(winSituation, callList);

        // Assert
        Assert.False(actual);
    }
}
