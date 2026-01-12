using System.ComponentModel.DataAnnotations;

namespace PITL.Power.Extract;

public sealed class ExtractOptions
{
    [Range(1, 1440)]
    public int IntervalMinutes { get; init; } = 5;

    [Range(0, 100)]
    public int RetryCount { get; init; } = 10;

    [Range(1, 3600)]
    public int RetryBaseDelaySeconds { get; init; } = 2;

    public string OutputDirectory { get; init; } = "";
}