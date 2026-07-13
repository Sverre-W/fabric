using System.ComponentModel.DataAnnotations;

namespace Fabric.Server.Hardware.Application;

public sealed class HardwareConnectionOptions
{
    public const string SectionName = "Hardware:Connection";

    [Required]
    public TimeSpan StaleAfter { get; init; } = TimeSpan.FromSeconds(60);

    [Required]
    public TimeSpan OfflineAfter { get; init; } = TimeSpan.FromSeconds(120);
}
