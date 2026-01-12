using PITL.Power.Extract;
using Services;

await Host.CreateDefaultBuilder(args)
    .UseWindowsService(o => o.ServiceName = "PITL.Power.Extract")
    .ConfigureLogging((ctx, logging) =>
    {
        logging.ClearProviders();

        logging.AddConsole();

#if WINDOWS
        if (WindowsServiceHelpers.IsWindowsService())
        {
            logging.AddEventLog(eventLogSettings =>
            {
                eventLogSettings.SourceName = "PITL.Power.Extract";
                eventLogSettings.LogName = "Application";
            });
        }
#endif
    })
    .ConfigureServices((ctx, services) =>
    {
        services
            .AddOptions<ExtractOptions>()
            .Bind(ctx.Configuration.GetSection("Extract"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<Worker>();
        services.AddSingleton<IExtractTask, ExtractTask>();
        services.AddSingleton<IPowerService, PowerService>();
        services.AddSingleton<ICsvHelper, CsvHelper>();
        services.AddSingleton<ITradeNetter, TradeNetter>();
    })
    .RunConsoleAsync();
