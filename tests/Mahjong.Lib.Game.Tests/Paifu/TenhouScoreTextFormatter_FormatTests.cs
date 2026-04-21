using Mahjong.Lib.Game.Paifu;

namespace Mahjong.Lib.Game.Tests.Paifu;

public class TenhouScoreTextFormatter_FormatTests
{
    [Fact]
    public void 通常2翻30符_30符2飜2900点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 2, fu: 30, points: 2900, isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("30符2飜2900点", result);
    }

    [Fact]
    public void 満貫_5翻_満貫を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 5, fu: 30, points: 8000, isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("満貫8000点", result);
    }

    [Fact]
    public void 切り上げ満貫_4翻40符_満貫を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 4, fu: 40, points: 8000, isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("満貫8000点", result);
    }

    [Fact]
    public void 切り上げ満貫_3翻70符_満貫を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 3, fu: 70, points: 8000, isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("満貫8000点", result);
    }

    [Fact]
    public void 跳満_6翻_跳満を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 6, fu: 30, points: 12000, isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("跳満12000点", result);
    }

    [Fact]
    public void 倍満_8翻_倍満を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 8, fu: 30, points: 16000, isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("倍満16000点", result);
    }

    [Fact]
    public void 三倍満_11翻_三倍満を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 11, fu: 30, points: 24000, isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("三倍満24000点", result);
    }

    [Fact]
    public void 数え役満_13翻_数え役満を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 13, fu: 30, points: 32000, isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("数え役満32000点", result);
    }

    [Fact]
    public void 役満_単独_役満を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 13, fu: 0, points: 32000, isYakuman: true, yakumanMultiplier: 1);

        Assert.Equal("役満32000点", result);
    }

    [Fact]
    public void 二倍役満_二倍役満を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 26, fu: 0, points: 64000, isYakuman: true, yakumanMultiplier: 2);

        Assert.Equal("二倍役満64000点", result);
    }

    [Fact]
    public void 三倍役満_三倍役満を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 39, fu: 0, points: 96000, isYakuman: true, yakumanMultiplier: 3);

        Assert.Equal("三倍役満96000点", result);
    }
}
