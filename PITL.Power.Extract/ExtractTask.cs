using Services;

namespace PITL.Power.Extract;

public interface IExtractTask
{
    Task RunWithRetryAsync(DateTime date, ExtractOptions options, CancellationToken ct);
}


public class ExtractTask(IPowerService powerService, ITradeNetter tradeNetter, 
    ICsvHelper csvHelper, ILogger<ExtractTask> logger) : IExtractTask
{
    public async Task RunWithRetryAsync(DateTime date, ExtractOptions options, CancellationToken ct)
    {
        var trades = await GetTradesWithRetryAsync(date, options, ct);
        logger.LogInformation("Found {TradeCount} trades for date {Date:yyyy-MM-dd}", trades.Length, date.Date);

        double[] volumes;

        try
        {
            volumes = tradeNetter.Net(trades, date).ToArray();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when netting trades, probably cause by bad data. Retry is inlikely to help");
            throw;
        }
        logger.LogInformation("Netted {TradeCount} trades for date {Date:yyyy-MM-dd} into {PeriodCount} periods", trades.Length, date.Date, volumes.Length);

        var filePath = csvHelper.GetExtractCsvPath(options.OutputDirectory, date);
        var csv = csvHelper.CreateExtractCsv(volumes);

        logger.LogInformation("Creating {FilePath}", filePath);
        File.WriteAllText(filePath, csv);
    }

    private async Task<PowerTrade[]> GetTradesWithRetryAsync(DateTime date, ExtractOptions options, CancellationToken ct)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                attempt++;
                return (await powerService.GetTradesAsync(date)).ToArray();
            }
            catch (Exception ex)
            {
                if (attempt > options.RetryCount)
                {
                    logger.LogError(ex, "Task failed after {Attempts} attempts.", attempt - 1);
                    throw;
                }

                // exponential backoff
                var delaySeconds = options.RetryBaseDelaySeconds * (int)Math.Pow(2, attempt - 1);

                logger.LogWarning(ex, "Task failed (attempt {Attempt}/{Max}). Retrying in {DelaySeconds}s...",
                    attempt, options.RetryCount, delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct);
            }
        }
    }
}
