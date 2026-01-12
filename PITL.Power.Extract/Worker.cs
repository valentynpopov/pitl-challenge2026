using Microsoft.Extensions.Options;

namespace PITL.Power.Extract;

public sealed class Worker(ILogger<Worker> logger, IExtractTask task, 
    IOptions<ExtractOptions> extractOptions) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var options = extractOptions.Value;

        if (options.IntervalMinutes <= 0)
            throw new InvalidOperationException("Worker:IntervalMinutes must be > 0");

        logger.LogInformation(
            "PITL.Power.Extract started. IntervalMinutes={IntervalMinutes}", options.IntervalMinutes);

        var date = DateTime.UtcNow;

        // Run immediately
        await task.RunWithRetryAsync(date, options, cancellationToken);

        // Then run on interval
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(options.IntervalMinutes));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            date = DateTime.UtcNow;
            await task.RunWithRetryAsync(date, options, cancellationToken);
        }
    }
}
