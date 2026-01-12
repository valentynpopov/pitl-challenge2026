using Services;

namespace PITL.Power.Extract;

public interface ITradeNetter
{
    IEnumerable<double> Net(PowerTrade[] trades, DateTime expectedDate);
}

public class TradeNetter: ITradeNetter
{
    public IEnumerable<double> Net(PowerTrade[] trades, DateTime expectedDate)
    {
        if (trades.Length == 0) return [];

        var periodCount = trades[0].Periods.Length;
        if (periodCount is < 23 or > 25)
            throw new ArgumentException($"Found {periodCount} periods - should be between 23 and 25");

        var netted = new double[periodCount];

        foreach (var trade in trades)
        {
            if (trade.Date != expectedDate)
                throw new ArgumentException($"Trade date {trade.Date} was expected to be {expectedDate}");

            if (trade.Periods.Length != periodCount)
                throw new ArgumentException($"Inconsistent number of periods - {periodCount} and {trade.Periods.Length}");

            int periodsFoundMask = 0;

            foreach (var p in trade.Periods)
            {
                if (p.Period < 1 || p.Period > periodCount)
                    throw new ArgumentException($"Period {p.Period} was expected to be between 1 and {periodCount}");

                int bit = 1 << (p.Period - 1);
                if ((periodsFoundMask & bit) != 0)
                    throw new ArgumentException($"Duplicate period {p.Period} found");
                periodsFoundMask |= bit;

                netted[p.Period - 1] += p.Volume;
            }
        }
        return netted;
    }
}
