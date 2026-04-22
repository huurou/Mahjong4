using Mahjong.Lib.Game.Paifu;

namespace Mahjong.Lib.Game.Tests.Paifu;

public class TenhouScoreTextFormatter_FormatTests
{
    [Fact]
    public void 通常2翻30符_ロン_30符2飜2900点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 2, fu: 30, new RonPaymentShape(2900), isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("30符2飜2900点", result);
    }

    [Fact]
    public void 満貫_5翻_ロン_満貫8000点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 5, fu: 30, new RonPaymentShape(8000), isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("満貫8000点", result);
    }

    [Fact]
    public void 切り上げ満貫_4翻40符_ロン_満貫8000点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 4, fu: 40, new RonPaymentShape(8000), isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("満貫8000点", result);
    }

    [Fact]
    public void 切り上げ満貫_3翻70符_ロン_満貫8000点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 3, fu: 70, new RonPaymentShape(8000), isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("満貫8000点", result);
    }

    [Fact]
    public void 跳満_6翻_ロン_跳満12000点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 6, fu: 30, new RonPaymentShape(12000), isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("跳満12000点", result);
    }

    [Fact]
    public void 倍満_8翻_ロン_倍満16000点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 8, fu: 30, new RonPaymentShape(16000), isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("倍満16000点", result);
    }

    [Fact]
    public void 三倍満_11翻_ロン_三倍満24000点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 11, fu: 30, new RonPaymentShape(24000), isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("三倍満24000点", result);
    }

    [Fact]
    public void 数え役満_13翻_ロン_数え役満32000点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 13, fu: 30, new RonPaymentShape(32000), isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("数え役満32000点", result);
    }

    [Fact]
    public void 役満_単独_ロン_役満32000点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 13, fu: 0, new RonPaymentShape(32000), isYakuman: true, yakumanMultiplier: 1);

        Assert.Equal("役満32000点", result);
    }

    [Fact]
    public void 二倍役満_ロン_二倍役満64000点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 26, fu: 0, new RonPaymentShape(64000), isYakuman: true, yakumanMultiplier: 2);

        Assert.Equal("二倍役満64000点", result);
    }

    [Fact]
    public void 三倍役満_ロン_三倍役満96000点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 39, fu: 0, new RonPaymentShape(96000), isYakuman: true, yakumanMultiplier: 3);

        Assert.Equal("三倍役満96000点", result);
    }

    [Fact]
    public void 子ツモ_30符4翻_30符4飜2000_3900点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 4, fu: 30, new ChildTsumoPaymentShape(NonDealerPay: 2000, DealerPay: 3900), isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("30符4飜2000-3900点", result);
    }

    [Fact]
    public void 親ツモ_30符4翻_30符4飜3900点の全員支払形式を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 4, fu: 30, new DealerTsumoPaymentShape(EachPay: 3900), isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("30符4飜3900点∀", result);
    }

    [Fact]
    public void 子ツモ満貫_満貫2000_4000点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 5, fu: 30, new ChildTsumoPaymentShape(NonDealerPay: 2000, DealerPay: 4000), isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("満貫2000-4000点", result);
    }

    [Fact]
    public void 親ツモ満貫_満貫4000点の全員支払形式を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 5, fu: 30, new DealerTsumoPaymentShape(EachPay: 4000), isYakuman: false, yakumanMultiplier: 0);

        Assert.Equal("満貫4000点∀", result);
    }

    [Fact]
    public void 子ツモ役満_役満8000_16000点を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 13, fu: 0, new ChildTsumoPaymentShape(NonDealerPay: 8000, DealerPay: 16000), isYakuman: true, yakumanMultiplier: 1);

        Assert.Equal("役満8000-16000点", result);
    }

    [Fact]
    public void 親ツモ役満_役満16000点の全員支払形式を返す()
    {
        var result = TenhouScoreTextFormatter.Format(han: 13, fu: 0, new DealerTsumoPaymentShape(EachPay: 16000), isYakuman: true, yakumanMultiplier: 1);

        Assert.Equal("役満16000点∀", result);
    }
}
