using Services;

namespace PITL.Power.Extract.Tests;

public class PowerServiceTests
{

    [Theory]
    // DST start - clocks go forward
    [InlineData(2024, 03, 31, 23)] 
    [InlineData(2025, 03, 30, 23)]
    // DST start - clocks go backward
    [InlineData(2024, 10, 27, 25)]
    [InlineData(2025, 10, 26, 25)]
    // regular days
    [InlineData(2024, 01, 15, 24)]
    [InlineData(2024, 07, 26, 24)]
    public async Task DaylightSavingTime(int year, int month, int day, int expectedPeriodCount)
    {
        var date = new DateTime(year, month, day);

        var sut = new PowerService();

        PowerTrade[]? trades = null;

        var attempt = 0;
        do
        {
            try
            {
                trades = (await sut.GetTradesAsync(date)).ToArray();                
                break;
            }
            catch
            {
                attempt++;
                continue;
            }
        } while (attempt <= 3);

        Assert.NotNull(trades);
        Assert.All(trades, t => Assert.Equal(expectedPeriodCount, t.Periods.Length));
    }
}
