using System.ComponentModel.DataAnnotations;

namespace Fabric.Server.Desfire.Application;

public sealed class DesfireEncodingOptions
{
    [Range(100, 60000)]
    public int SchedulerIntervalMilliseconds { get; set; } = 1000;

    [Range(1000, 3600000)]
    public int IdleSchedulerIntervalMilliseconds { get; set; } = 60000;

    [Range(1, 64)]
    public int MaxConcurrentRuns { get; set; } = 4;
}
