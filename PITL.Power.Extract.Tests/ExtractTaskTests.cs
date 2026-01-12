using Microsoft.Extensions.Logging;
using NSubstitute;
using Services;

namespace PITL.Power.Extract.Tests;
public  class ExtractTaskTests
{
    [Fact]
    public async Task Retry()
    {
        var powerService = Substitute.For<IPowerService>();

        var date = new DateTime(2026, 1, 12);

        powerService.GetTradesAsync(date)
            .Returns(
            _ => throw new Exception("Fail 1"),
            _ => throw new Exception("Fail 2"),
            _ => Task.FromResult<IEnumerable<PowerTrade>>([PowerTrade.Create(date, 24)])
            );

        var tradeNetter = Substitute.For<ITradeNetter>();

        var csvHelper = Substitute.For<ICsvHelper>();
        csvHelper.GetExtractCsvPath(Arg.Any<string>(), Arg.Any<DateTime>())
            .Returns(@"c:\temp\extract.csv");

        var extractTask = new ExtractTask(
            powerService,
            tradeNetter,
            csvHelper,
            Substitute.For<ILogger<ExtractTask>>()
            );

        await extractTask.RunWithRetryAsync(date, new ExtractOptions
        {
            OutputDirectory = @"c:\temp",
            RetryBaseDelaySeconds = 0,
            RetryCount = 5
        }, CancellationToken.None);

        tradeNetter.Received(1).Net(Arg.Any<PowerTrade[]>(), Arg.Any<DateTime>());
    }
}
