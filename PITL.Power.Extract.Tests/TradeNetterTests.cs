using Services;

namespace PITL.Power.Extract.Tests;
public class TradeNetterTests
{
    private readonly DateTime _defaultDate = new DateTime(2026, 1, 12);

    private readonly TradeNetter _sut = new();

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(22)]
    [InlineData(26)]
    [InlineData(27)]
    public void PeriodCount_OutOfRange(int badPeriodCount)
    {
        var trade1 = PowerTrade.Create(_defaultDate, badPeriodCount);
        var trade2 = PowerTrade.Create(_defaultDate, badPeriodCount);
        var ex = Assert.Throws<ArgumentException>(() => _sut.Net([trade1, trade2], _defaultDate));
    }

    [Fact]
    public void Period_Inconsistent()
    {
        var trade1 = PowerTrade.Create(_defaultDate, 24);
        var trade2 = PowerTrade.Create(_defaultDate, 23);
        var trade3 = PowerTrade.Create(_defaultDate, 24);
        var ex = Assert.Throws<ArgumentException>(() => _sut.Net([trade1, trade2, trade3], _defaultDate));
    }

    [Fact]
    public void Period_Duplicate()
    {
        var trade1 = PowerTrade.Create(_defaultDate, 24);
        trade1.Periods[1].Period = 1; // should have been 4
        var trade2 = PowerTrade.Create(_defaultDate, 24);
        var ex = Assert.Throws<ArgumentException>(() => _sut.Net([trade1, trade2], _defaultDate));
    }

    [Fact]
    public void Date_Inconsistent()
    {
        var trade1 = PowerTrade.Create(_defaultDate, 24);
        var trade2 = PowerTrade.Create(new DateTime(2024, 1, 1), 24);
        var trade3 = PowerTrade.Create(_defaultDate, 24);
        var ex = Assert.Throws<ArgumentException>(() => _sut.Net([trade1, trade2, trade3], _defaultDate));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(26)]
    public void Period_OutOfRange(int badPeriod)
    {
        var trade1 = PowerTrade.Create(_defaultDate, 24);
        var trade2 = PowerTrade.Create(_defaultDate, 24);
        trade1.Periods[3].Period = badPeriod;
        var ex = Assert.Throws<ArgumentException>(() => _sut.Net([trade1, trade2], _defaultDate));
    }

    [Fact]
    public void TotalVolumes_Correct()
    {
        PowerTrade CreateTrade(Func<double, int> volumeByPeriod)
        {
            var trade = PowerTrade.Create(_defaultDate, 24);

            for (var p = 1; p <= 24; p++)
            {
                var volume = volumeByPeriod(p);
                trade.Periods[p - 1] = new PowerPeriod { Period = p, Volume = volume };
            }
            return trade;
        }

        var trade1 = CreateTrade(p => 100);
        var trade2 = CreateTrade(p => p <= 11 ? 50 : -20);

        double[] expected = {
            150, 150, 150, 
            150, 150, 150, 
            150, 150, 150, 
            150, 150, 80, 
            80,  80,  80,
            80,  80,  80,  
            80,  80,  80,
            80,  80,  80,
        };

        double[] actual = [.. _sut.Net([trade1, trade2], _defaultDate)];

        Assert.Equal(expected, actual);
    }

}
